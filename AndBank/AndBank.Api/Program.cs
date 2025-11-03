using AndBank.Domain.Repositories;
using AndBank.Infrastructure.Data;
using AndBank.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

var conn = builder.Configuration.GetConnectionString("Postgres") 
           ?? Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
           ?? "Host=localhot;Port=5432;Database=postgres;Username=postgres;Password=dev123";

builder.Services.AddDbContext<AndBankDbContext>(options =>
    options.UseNpgsql(conn, b => b.MigrationsAssembly("AndBank.Infrastructure")));

builder.Services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
builder.Services.AddScoped<IPositionRepository, PositionRepository>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "Position API", Version = "v1" }));

var app = builder.Build();
app.UseSwagger(); app.UseSwaggerUI();
app.MapControllers();
app.Run();