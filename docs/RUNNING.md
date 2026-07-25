Running Travelio MVP (local development)

Prerequisites
- .NET 8 SDK installed
- Docker (optional, for docker-compose)
- Python 3.11 (optional, to run provider simulators directly)

Quickstart (using docker-compose)
1. Start infra and provider simulators:
   docker-compose up --build
   This will start Postgres (5432), Redis (6379), provider simulators (9001,9002), Prometheus (9090) and Grafana (3000).

2. Create database schema (once Postgres is ready):
   psql -h localhost -U travelio -d travelio -f ddl.sql

3. Build and run the API locally:
   dotnet build src\Travelio.slnx
   dotnet run --project src\Travelio.Api\Travelio.Api.csproj

   The API listens on the default Kestrel port (check console output), typically http://localhost:5000

Manual (without Docker)
1. Run provider simulators locally:
   python provider_simulator_1.py --port 9001
   python provider_simulator_2.py --port 9002

2. Run Postgres and Redis locally (or use Docker for those services).

3. Build and run the API as above.

Testing the endpoints
- Search:
  POST /api/v1/search
  Body example:
  {
    "origin": "BOG",
    "destination": "MDE",
    "startDate": "2026-10-01",
    "endDate": "2026-10-05",
    "passengers": 2
  }

- Prebooking:
  POST /api/v1/prebookings
  Body example: { "clientId": "partner-1", "offerId": "<offer id from search>", "searchId": "<search id>" }

- Booking (idempotent):
  POST /api/v1/bookings
  Headers: Idempotency-Key: <uuid>
  Body example: { "clientId": "partner-1", "preBookingId": "<prebooking id>" }

Next steps
- Implement Idempotency middleware to validate and persist Idempotency-Key at middleware level.
- Implement Redis-based distributed locks for pre-bookings.
- Implement HostedService worker to expire pre-bookings and reconcile with providers.
- Add integration tests that start provider simulators and exercise end-to-end flows.

Contact
- This repo was scaffolded by an AI assistant using Copilot CLI runtime in VS Code.
