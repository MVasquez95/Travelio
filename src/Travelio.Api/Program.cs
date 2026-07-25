using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Travelio.Application.Interfaces;
using Travelio.Infrastructure.Data;
using Travelio.Infrastructure.Services;
using StackExchange.Redis;
using Prometheus;
using Travelio.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var configuration = builder.Configuration;

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks()
    .AddNpgSql(configuration.GetConnectionString("DefaultConnection")!)
    .AddRedis(configuration["Redis:Configuration"]!);
// Swagger/ OpenAPI generation is optional; enable by adding Swashbuckle and related configuration when ready.

// EF Core
builder.Services.AddDbContext<TravelioDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")).UseSnakeCaseNamingConvention());

builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(new ConfigurationOptions
{
    EndPoints = { configuration["Redis:Configuration"] ?? "localhost:6379" },
    AbortOnConnectFail = false
}));

// Register application services
builder.Services.AddHttpClient("provider1", client => { client.BaseAddress = new Uri(configuration["Providers:Provider1:BaseUrl"]!); client.Timeout = TimeSpan.FromSeconds(3); });
builder.Services.AddHttpClient("provider2", client => { client.BaseAddress = new Uri(configuration["Providers:Provider2:BaseUrl"]!); client.Timeout = TimeSpan.FromSeconds(3); });

builder.Services.AddTransient<ISearchService, SearchService>();
builder.Services.AddTransient<IPreBookingService, PreBookingService>();
builder.Services.AddTransient<IBookingService, BookingService>();
builder.Services.AddTransient<ProviderAdapter>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // Swagger disabled to avoid source-generator collisions in the template; enable Swashbuckle when ready.
}

app.UseRouting();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpMetrics();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.MapMetrics("/metrics");

app.Run();
