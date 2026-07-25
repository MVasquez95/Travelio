# Arquitectura del MVP

Travelio es un monolito modular desplegado como dos procesos: `Travelio.Api` atiende las solicitudes HTTP y `Travelio.Workers` ejecuta tareas de expiración. Ambos comparten PostgreSQL, Redis y los contratos del dominio.

- **API / Application:** expone Search, Pre-booking y Booking; coordina los casos de uso sin conocer formatos específicos de proveedores.
- **Infrastructure:** contiene los adaptadores HTTP de proveedores, EF Core/PostgreSQL y Redis. Cada proveedor conserva su contrato propio detrás de `ProviderAdapter`.
- **PostgreSQL:** fuente de verdad para búsquedas, ofertas normalizadas, holds, reservas e idempotencia.
- **Redis:** cachea búsquedas por 30 segundos y protege el pre-booking mediante un lock distribuido por oferta.
- **Worker:** marca holds vencidos y solicita al proveedor liberar su reserva.

La confirmación usa `clientId + Idempotency-Key` y una huella del payload. Una repetición con el mismo payload devuelve la reserva creada; con otro payload se rechaza. Si el proveedor agota el timeout, la reserva queda en `unknown`, estado diseñado para una futura reconciliación.
