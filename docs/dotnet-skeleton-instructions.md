Travelio - Esqueleto .NET 8 Web API (instrucciones para generar localmente)

Objetivo
- Crear un proyecto modular (monolito modular) con las siguientes capas:
  - Travelio.Api (entry Web API)
  - Travelio.Application (use-cases, DTOs, orchestrators)
  - Travelio.Domain (entities, value objects, domain services)
  - Travelio.Infrastructure (EF Core, Redis, HttpClients, ProviderAdapters)
  - Travelio.Workers (background hosted services)
  - Travelio.Tests (unit & integration tests)

Comandos recomendados (ejecutar desde el directorio raíz del repo):

# Crear carpeta src y moverse
mkdir src
cd src

# Crear solución
dotnet new sln -n Travelio

# Crear proyectos
dotnet new webapi -n Travelio.Api
dotnet new classlib -n Travelio.Application
dotnet new classlib -n Travelio.Domain
dotnet new classlib -n Travelio.Infrastructure
dotnet new worker -n Travelio.Workers
dotnet new xunit -n Travelio.Tests

# Añadir proyectos a la solución
dotnet sln add Travelio.Api/Travelio.Api.csproj
dotnet sln add Travelio.Application/Travelio.Application.csproj
dotnet sln add Travelio.Domain/Travelio.Domain.csproj
dotnet sln add Travelio.Infrastructure/Travelio.Infrastructure.csproj
dotnet sln add Travelio.Workers/Travelio.Workers.csproj
dotnet sln add Travelio.Tests/Travelio.Tests.csproj

# Añadir referencias de proyecto
cd Travelio.Api
dotnet add reference ../Travelio.Application/Travelio.Application.csproj
cd ../Travelio.Application
dotnet add reference ../Travelio.Domain/Travelio.Domain.csproj
cd ../Travelio.Infrastructure
dotnet add reference ../Travelio.Domain/Travelio.Domain.csproj

# Volver al root de src
cd ..

# Instalar paquetes recomendados
cd Travelio.Api
dotnet add package Microsoft.EntityFrameworkCore --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.0
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.0
dotnet add package StackExchange.Redis --version 2.7.0
dotnet add package OpenTelemetry.Exporter.Prometheus.AspNetCore
cd ../Travelio.Infrastructure
dotnet add package Polly --version 8.0.0

# Notas
- Configurar EF Core migrations en Travelio.Infrastructure o en un proyecto separado de Migrations.
- Usar IHttpClientFactory con named clients y políticas Polly para cada proveedor adapter.
- Implementar IdempotencyKey store en la DB y a nivel de middleware para /bookings.

Archivos plantilla (pegar en los proyectos creados):

Program.cs (Travelio.Api) - plantilla mínima para .NET 8

---------------------------------
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Configuration, logging, telemetry, etc.
builder.Services.AddControllers();

// TODO: add DB, Redis, HttpClients, DI for application services
// builder.Services.AddDbContext<...>();
// builder.Services.AddStackExchangeRedisCache(...);
// builder.Services.AddHttpClient("provider-1", client => { /* base */ }).AddPolicyHandler(...);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
---------------------------------

appsettings.json (Travelio.Api) - plantilla de configuración

{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=travelio;Username=travelio;Password=travelio"
  },
  "Redis": {
    "Configuration": "localhost:6379"
  },
  "Providers": {
    "Provider1": { "BaseUrl": "http://host.docker.internal:9001" },
    "Provider2": { "BaseUrl": "http://host.docker.internal:9002" }
  }
}


Próximos pasos recomendados (después de generar proyecto):
- Implementar DTOs y contratos (usar openapi.yaml para referencia).
- Crear middleware de Idempotency-Key para /bookings.
- Implementar Provider Adapters en Travelio.Infrastructure.
- Crear HostedService para expiraciones en Travelio.Workers.
- Añadir pruebas de integración que lancen los provider simulators.

Si quieres, puedo generar los archivos de plantilla Program.cs y appsettings.json directamente en el repo (en root) o intentar ejecutar dotnet new aquí si autorizas la ejecución de comandos en este entorno.