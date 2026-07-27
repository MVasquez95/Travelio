# Guía de ejecución y demo

## Requisitos

- Docker Desktop en ejecución.
- Opcional para desarrollo local: .NET SDK 8.

## Levantar el stack

Desde la raíz del repositorio:

```powershell
docker compose up -d --build
docker compose ps
Invoke-WebRequest http://localhost:8080/health
```

El esquema PostgreSQL se inicializa desde `ddl.sql` cuando se crea el volumen por primera vez. Para reiniciar **todos los datos locales**:

```powershell
docker compose down -v
docker compose up -d --build
```

## Endpoints

| Servicio | URL |
| --- | --- |
| API | `http://localhost:8080` |
| Health | `http://localhost:8080/health` |
| Métricas | `http://localhost:8080/metrics` |
| Prometheus | `http://localhost:9090` |
| Grafana | `http://localhost:3000` |

## Flujo manual

1. `POST /api/v1/search` con `origin`, `destination`, `startDate`, `endDate` y `passengers`.
2. Selecciona una oferta cuyo campo `bookable` sea `true`.
3. `POST /api/v1/prebookings` con `clientId`, `offerId` y `searchId`.
4. `POST /api/v1/bookings` con el header `Idempotency-Key` y el `preBookingId` recibido.
5. Repite exactamente el paso anterior: debe devolver el mismo `bookingId`.
6. Consulta `GET /api/v1/bookings/{bookingId}` o cancela con `POST /api/v1/bookings/{bookingId}/cancel`.

Ejemplo de búsqueda:

```powershell
$body = @{ origin='LIM'; destination='CUZ'; startDate='2026-10-01T00:00:00Z'; endDate='2026-10-05T00:00:00Z'; passengers=2 } | ConvertTo-Json
Invoke-RestMethod -Method Post -Uri http://localhost:8080/api/v1/search -ContentType application/json -Body $body
```

## Verificación de código

```powershell
dotnet build src/Travelio.slnx --no-restore
dotnet test src/Travelio.Tests/Travelio.Tests.csproj --no-restore
```

## Ejecutar simuladores sin Docker

Docker es el camino recomendado. Si necesitas depurar un proveedor desde el host, crea un entorno virtual aislado:

```powershell
cd provider-simulator
py -3.11 -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install -r requirements.txt
python provider_simulator_1.py --port 9001
```

En otra terminal activada ejecuta `python provider_simulator_2.py --port 9002`.
