# Travelio

Travelio es un MVP B2B de agregación turística. Centraliza proveedores con contratos distintos, normaliza ofertas y cubre el ciclo de búsqueda, hold temporal, confirmación idempotente, consulta y cancelación de reservas.

## Qué demuestra

- Monolito modular .NET 8 con capas `Api`, `Application`, `Domain`, `Infrastructure` y un proceso `Workers`.
- Adaptadores para dos proveedores FastAPI intencionalmente diferentes.
- PostgreSQL como fuente de verdad, Redis para cache y locks distribuidos.
- Idempotencia por `clientId + Idempotency-Key`, expiración de holds y reconciliación de resultados inciertos.
- Health checks, métricas Prometheus y CI con GitHub Actions.

## Inicio rápido

```powershell
docker compose up -d --build
Invoke-WebRequest http://localhost:8080/health
```

La API queda en `http://localhost:8080`, Prometheus en `http://localhost:9090` y Grafana en `http://localhost:3000` (usuario `admin`, contraseña `admin`).

Consulta [docs/RUNNING.md](docs/RUNNING.md) para la guía completa y [docs/architecture.md](docs/architecture.md) para la explicación de componentes y decisiones.
