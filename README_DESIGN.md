Travelio - Diseño y artefactos (MVP)

Contenido generado en esta iteración:
- openapi.yaml: especificación OpenAPI para los endpoints principales (search, prebookings, bookings, callbacks).
- ddl.sql: DDL PostgreSQL con tablas principales (providers, offers_normalized, pre_bookings, bookings, idempotency_keys, raw_provider_responses, audit_events).
- docker-compose.yml: stack para desarrollo local (Postgres, Redis, 2 provider simulators, travelio-api placeholder, Prometheus, Grafana).
- provider_simulator_1.py / provider_simulator_2.py: dos simuladores FastAPI con comportamiento intencionalmente distinto (latencia/errores) para pruebas de integración y contract testing.
- sequence.md: diagrama de secuencia (Mermaid) con flujos Search, Pre-book, Booking y expiración.
- plan.md (sesión): plan de trabajo y siguientes pasos (en carpeta de sesión de Copilot).

Instrucciones rápidas para desarrollo local
1. Levantar infra mínima (Postgres + Redis + providers simulados):
   - python -m pip install fastapi uvicorn
   - ejecutar (modo local sin docker):
     * python provider_simulator_1.py --port 9001
     * python provider_simulator_2.py --port 9002
   - o usar docker-compose.yml (requiere docker): docker-compose up
2. Importar esquema en Postgres:
   - psql -h localhost -U travelio -d travelio -f ddl.sql
3. Poner en marcha Travelio API (pendiente): crear proyecto .NET 8 Web API y configurar conexiones a Postgres y Redis.

Siguientes entregables recomendados
- Esqueleto .NET 8 Web API con módulos: Api, Domain, Application, Infrastructure, Workers.
- Implementar adapters HTTP a provider-simulators y pruebas de integración.
- Añadir Prometheus config y dashboards Grafana.

Notas
- Los simuladores están diseñados para que el equipo de backend pruebe resiliencia: control de timeouts, reintentos y circuit breakers.
- El docker-compose actual monta el repo en los contenedores de simulador (read-only) para poder ejecutar los scripts desde el host.

Soy un asistente de IA usando Copilot CLI runtime en VS Code.