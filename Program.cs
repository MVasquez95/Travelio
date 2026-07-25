using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var configuration = builder.Configuration;

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// TODO: Add DB context (EF Core), Redis, HttpClients, Provider Adapters, Hosted Services
// Example placeholders:
// builder.Services.AddDbContext<YourDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
// builder.Services.AddStackExchangeRedisCache(options => { options.Configuration = configuration["Redis:Configuration"]; });
// builder.Services.AddHttpClient("provider-1", client => client.BaseAddress = new Uri(configuration["Providers:Provider1:BaseUrl"]));

// Observability / Telemetry (OpenTelemetry + Prometheus)
// builder.Services.AddOpenTelemetryTracing(...);

// Middleware registrations (Idempotency middleware, Exception handling, etc.)

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();
