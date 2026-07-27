# Flujo de negocio

1. **Search:** consulta Provider 1 y Provider 2 en paralelo. Travelio tolera un fallo parcial, normaliza el resultado, guarda las ofertas con UUID internos y cachea la respuesta.
2. **Pre-booking:** valida la oferta, toma un lock Redis, llama al endpoint de hold específico del proveedor y persiste su referencia y expiración.
3. **Booking:** valida cliente y vigencia del hold, registra la clave idempotente, confirma con el proveedor y actualiza el estado a `confirmed`, `failed` o `unknown`.
4. **Post-booking:** permite consultar y cancelar una reserva confirmada.
5. **Worker:** libera holds vencidos y reintenta bookings inciertos sin duplicar la operación externa.
