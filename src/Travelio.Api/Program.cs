using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Travelio.Application.Interfaces;
using Travelio.Infrastructure.Data;
using Travelio.Infrastructure.Services;
using Travelio.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var configuration = builder.Configuration;

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// Swagger/ OpenAPI generation is optional; enable by adding Swashbuckle and related configuration when ready.

// EF Core
builder.Services.AddDbContext<TravelioDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

// Register application services
builder.Services.AddHttpClient("provider1", client => client.BaseAddress = new Uri(configuration["Providers:Provider1:BaseUrl"]));
builder.Services.AddHttpClient("provider2", client => client.BaseAddress = new Uri(configuration["Providers:Provider2:BaseUrl"]));

builder.Services.AddTransient<ISearchService, SearchService>();
builder.Services.AddTransient<IPreBookingService, PreBookingService>();
builder.Services.AddTransient<IBookingService, BookingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // Swagger disabled to avoid source-generator collisions in the template; enable Swashbuckle when ready.
}

app.UseRouting();

// Idempotency middleware intercepts POST /api/v1/bookings to ensure safe retries
app.UseMiddleware<IdempotencyMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.Run();
