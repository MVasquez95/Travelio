# Flujo de negocio

1. **Search:** Travelio consulta ambos proveedores en paralelo, tolera resultados parciales, normaliza y persiste las ofertas con un UUID interno. El resultado se cachea en Redis.
2. **Pre-booking:** el cliente usa el UUID interno. Travelio toma un lock Redis, solicita el hold al proveedor y persiste la referencia y expiración real del proveedor.
3. **Booking:** se valida cliente, vigencia del hold e `Idempotency-Key`; se crea una reserva pendiente y se confirma con el proveedor. El resultado final es `confirmed`, `failed` o `unknown`.
4. **Post-booking:** se puede consultar una reserva confirmada o solicitar su cancelación. El worker libera holds vencidos.
