using AndBank.Application.DTOs;
using AndBank.Domain.Repositories;
using AndBank.Infrastructure.Data;
using AndBank.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System.Text.Json;
using NCrontab;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, cfg) =>
    {
        cfg.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddHttpClient("andbank", client =>
        {
            client.BaseAddress = new Uri("https://api.andbank.com.br/");
            client.Timeout = TimeSpan.FromMinutes(10);
            client.DefaultRequestHeaders.Add("X-Test-Key", "9MsyhgyioqtMLUiUFRNm");
        });

        var configuration = context.Configuration;
        var cs = configuration.GetConnectionString("Default") ?? "Host=postgres;Port=5432;Database=postgres;Username=postgres;Password=dev#123456";

        services.AddSingleton(new NpgsqlBulkImporter(cs));
        services.AddScoped<IPositionRepository, PositionRepository>();
        services.AddHttpClient();
        services.AddLogging();
        services.AddHostedService<PositionImportScheduler>();
    })
    .Build();

await host.RunAsync();

public class PositionImportScheduler : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PositionImportScheduler> _logger;
    private readonly CrontabSchedule _schedule;
    private DateTime _nextRun;
    private const string Schedule = "48 18 * * *";

    public PositionImportScheduler(IServiceProvider serviceProvider, ILogger<PositionImportScheduler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _schedule = CrontabSchedule.Parse(Schedule, new CrontabSchedule.ParseOptions { IncludingSeconds = false });
        _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Position Import Scheduler iniciado. Próxima execução: {NextRun}", _nextRun);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;

            if (now >= _nextRun)
            {
                _logger.LogInformation("Iniciando importação de posições...");

                try
                {
                    await ImportPositionsAsync(stoppingToken);
                    _logger.LogInformation("Importação concluída com sucesso");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro durante a importação de posições");
                }

                _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
                _logger.LogInformation("Próxima execução agendada para: {NextRun}", _nextRun);
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task ImportPositionsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var httpFactory = services.GetRequiredService<IHttpClientFactory>();
        var bulkImporter = services.GetRequiredService<NpgsqlBulkImporter>();
        var logger = services.GetRequiredService<ILogger<PositionImportScheduler>>();
        var client = httpFactory.CreateClient("andbank");

        AsyncRetryPolicy<HttpResponseMessage> retryPolicy = Policy
            .Handle<HttpRequestException>(ex => !ex.Message.Contains("404"))
            .OrResult<HttpResponseMessage>(r =>
                !r.IsSuccessStatusCode &&
                r.StatusCode != System.Net.HttpStatusCode.NotFound &&
                r.StatusCode != System.Net.HttpStatusCode.Unauthorized)
            .WaitAndRetryAsync(3, attempt =>
            {
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                logger.LogWarning("Tentativa {Attempt} falhou. Aguardando {Delay}s antes de tentar novamente", attempt, delay.TotalSeconds);
                return delay;
            });

        logger.LogInformation("Buscando dados da API...");
        var streamResponse = await retryPolicy.ExecuteAsync(() =>
            client.GetAsync("candidate/positions", HttpCompletionOption.ResponseHeadersRead, cancellationToken));

        if (!streamResponse.IsSuccessStatusCode)
        {
            var statusCode = (int)streamResponse.StatusCode;
            var reasonPhrase = streamResponse.ReasonPhrase;

            logger.LogError(
                "Erro ao acessar a API. Status Code: {StatusCode} ({ReasonPhrase}). " +
                "URL: {Url}. Verifique se o endpoint está correto.",
                statusCode, reasonPhrase, client.BaseAddress + "candidate/positions");

            throw new HttpRequestException(
                $"Erro {statusCode}: {reasonPhrase}. Endpoint pode estar incorreto ou indisponível.");
        }

        await using var stream = await streamResponse.Content.ReadAsStreamAsync(cancellationToken);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var batch = new List<AndBank.Domain.Entities.Position>();
        const int batchSize = 5000;
        int totalProcessed = 0;

        await foreach (var dto in JsonSerializer.DeserializeAsyncEnumerable<PositionDto>(stream, options, cancellationToken))
        {
            if (dto is null) continue;

            var p = new AndBank.Domain.Entities.Position
            {
                PositionId = dto.PositionId,
                ProductId = dto.ProductId,
                ClientId = dto.ClientId,
                Date = dto.Date,
                Value = dto.Value,
                Quantity = dto.Quantity
            };

            batch.Add(p);

            if (batch.Count >= batchSize)
            {
                await bulkImporter.BulkInsertAsync(batch);
                totalProcessed += batch.Count;
                logger.LogInformation("Processados {Count} registros (total: {Total})", batch.Count, totalProcessed);
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            await bulkImporter.BulkInsertAsync(batch);
            totalProcessed += batch.Count;
            logger.LogInformation("Processados {Count} registros finais (total: {Total})", batch.Count, totalProcessed);
            batch.Clear();
        }

        logger.LogInformation("Importação finalizada. Total de registros processados: {Total}", totalProcessed);
    }
}