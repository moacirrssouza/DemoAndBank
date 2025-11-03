using AndBank.Application.DTOs;
using AndBank.Domain.Repositories;
using AndBank.Infrastructure.Data;
using AndBank.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Retry;
using System.Text.Json;


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
    })
    .Build();

using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;
var httpFactory = services.GetRequiredService<IHttpClientFactory>();
var bulkImporter = services.GetRequiredService<NpgsqlBulkImporter>();

var client = httpFactory.CreateClient("andbank");

AsyncRetryPolicy<HttpResponseMessage> retryPolicy = Policy
    .Handle<HttpRequestException>()
    .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
    .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

var streamResponse = await retryPolicy.ExecuteAsync(() => client.GetAsync("candidate/positions", HttpCompletionOption.ResponseHeadersRead));
streamResponse.EnsureSuccessStatusCode();

await using var stream = await streamResponse.Content.ReadAsStreamAsync();

var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
};

var batch = new List<AndBank.Domain.Entities.Position>();
const int batchSize = 5000;

await foreach (var dto in JsonSerializer.DeserializeAsyncEnumerable<PositionDto>(stream, options))
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
        batch.Clear();
    }
}

if (batch.Count > 0)
{
    await bulkImporter.BulkInsertAsync(batch);
    batch.Clear();
}