# Travelio

Travelio is a B2B travel aggregator MVP that centralizes offers from multiple external providers, supports search, pre-booking holds, idempotent booking, and post-booking reconciliation.

See `docs/README_DESIGN.md` for architecture and design details.
See `docs/RUNNING.md` for local setup and execution instructions.

The local stack is started with `docker compose up --build`. It includes PostgreSQL, Redis, two provider simulators, the API, and the expiration worker.
