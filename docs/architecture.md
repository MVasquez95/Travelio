# Arquitectura del MVP

Travelio usa un **monolito modular**. Es la decisión adecuada para este MVP: conserva límites claros y buenas prácticas sin el coste operativo de microservicios prematuros.

| Componente | Responsabilidad |
| --- | --- |
| `Travelio.Api` | API REST, validación, errores, health y métricas. |
| `Travelio.Application` | Contratos DTO e interfaces de casos de uso. |
| `Travelio.Domain` | Entidades de negocio: oferta, hold, booking, proveedor e idempotencia. |
| `Travelio.Infrastructure` | EF Core/PostgreSQL, Redis y adaptadores HTTP de proveedores. |
| `Travelio.Workers` | Expira holds y reconcilia bookings en estado `unknown`. |
| `provider-simulator` | Dos proveedores FastAPI con formatos, latencias y endpoints distintos. |

## Datos y resiliencia

- **PostgreSQL** conserva búsquedas, ofertas normalizadas, holds, reservas e idempotencia.
- **Redis** cachea búsquedas durante 30 segundos y aplica un lock por oferta al crear un hold.
- Cada proveedor queda detrás de `ProviderAdapter`; la API no expone sus formatos internos.
- Los timeouts o resultados inciertos producen el estado `unknown`; el worker reintenta la confirmación con la misma clave de idempotencia.
- Prometheus recoge métricas HTTP desde `/metrics`; `/health` comprueba PostgreSQL y Redis.

## Decisión de idempotencia

La unicidad se define por `clientId + Idempotency-Key`. Travelio guarda además el hash del payload: una repetición idéntica devuelve la reserva existente; reutilizar la clave con otro payload se rechaza.
