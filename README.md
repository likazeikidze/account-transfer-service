# Account Transfer Service

A simple full-stack application for simulating money transfers between accounts. Built as a technical assignment, with an eye toward being a real starting point for future development.

## Overview

The system maintains a set of accounts with balances. A transfer moves money from one account to another: the sender is debited, the receiver is credited, and the operation is rejected entirely if the sender doesn't have sufficient funds. No account balance can go negative. This is a simulation — no real payment rails are involved, it only updates balances stored in the database.

## Architecture

```
React (nginx) ──/api/──▶ .NET Core Web API ──▶ MS SQL Server
```

The backend follows a small layered architecture to keep it easy to extend:

```
Domain          (entities, exceptions, interfaces — no dependencies)
   ▲
Infrastructure  (EF Core DbContext, migrations, seed data, TransferService)
   ▲
Api             (controllers, DTOs, error handling, Program.cs)
```

New features (auth, multi-currency, scheduled transfers, etc.) plug into `Domain`/`Infrastructure` without touching controllers.

## Tech Stack

- **Backend:** .NET 8, ASP.NET Core Web API, Entity Framework Core
- **Database:** MS SQL Server 2022
- **Frontend:** React + TypeScript (Vite), Tailwind CSS, TanStack Query
- **Infra:** Docker, Docker Compose, nginx (serves the frontend and proxies `/api/`)

## Prerequisites

Only **Docker** and **Docker Compose** are required to run the project. (Optional, for local development outside Docker: .NET 8 SDK, Node.js 20+.)

## Quick Start

```bash
cp .env.example .env
# edit .env if you want a different SA password or ports

docker compose up --build
```

Once the containers are healthy:

- Frontend: http://localhost:3000
- API: http://localhost:5000/api/accounts
- API health check: http://localhost:5000/health

The database schema and seed data are applied automatically on API startup — no manual steps required.

## Configuration

All configuration is passed through environment variables, set via `.env` (copy `.env.example` to `.env` and adjust as needed):

| Variable          | Description                                   | Default                |
| ----------------- | ---------------------------------------------- | ----------------------- |
| `SQL_SA_PASSWORD` | SQL Server `sa` password (must meet [SQL Server complexity rules](https://learn.microsoft.com/en-us/sql/relational-databases/security/password-policy)) | `YourStrong!Passw0rd` |
| `API_PORT`        | Host port the API is published on              | `5000`                  |
| `FRONTEND_PORT`   | Host port the frontend is published on         | `3000`                  |

The API's connection string is built from `SQL_SA_PASSWORD` inside `docker-compose.yml` (`ConnectionStrings__DefaultConnection`) — nothing is hardcoded in the image.

## API Reference

| Method | Path              | Description                                  |
| ------ | ----------------- | --------------------------------------------- |
| GET    | `/api/accounts`    | List all accounts and balances                |
| GET    | `/api/accounts/{id}` | Get a single account                        |
| POST   | `/api/transfers`   | Perform a transfer                            |
| GET    | `/api/transfers`   | List completed transfers, newest first        |
| GET    | `/health`          | Health check                                  |

### Example: successful transfer

```bash
curl -X POST http://localhost:5000/api/transfers \
  -H "Content-Type: application/json" \
  -d '{"senderAccountId": "<id>", "receiverAccountId": "<id>", "amount": 50.00}'
```

### Example: insufficient funds

```bash
curl -X POST http://localhost:5000/api/transfers \
  -H "Content-Type: application/json" \
  -d '{"senderAccountId": "<low-balance-account-id>", "receiverAccountId": "<id>", "amount": 999999}'
```

Returns `422 Unprocessable Entity`:

```json
{
  "title": "Insufficient funds",
  "status": 422,
  "detail": "Account '...' does not have sufficient funds for this transfer.",
  "errorCode": "INSUFFICIENT_FUNDS",
  "traceId": "..."
}
```

### Example: unknown account

Returns `404 Not Found` with `errorCode: "ACCOUNT_NOT_FOUND"`.

Model validation errors (missing fields, non-positive amount, same sender/receiver) return `400 Bad Request` with the standard ASP.NET Core `ValidationProblemDetails` shape.

## Database & Migrations

Schema is managed with EF Core Code First migrations, located in `backend/src/AccountTransferService.Infrastructure/Data/Migrations`. On startup, the API applies pending migrations automatically (with a short retry loop in case SQL Server isn't accepting connections yet) and seeds four accounts if the database is empty:

| Account #  | Owner          | Balance  |
| ---------- | -------------- | -------- |
| ACC-1001   | Lika Zeikidze  | $1,000.00 |
| ACC-1002   | Bob Smith      | $500.00   |
| ACC-1003   | Charlie Davis  | $250.00 (useful for triggering insufficient-funds) |
| ACC-1004   | Diana Lee      | $0.00 (zero-balance edge case) |

To add a new migration during development:

```bash
cd backend
dotnet ef migrations add <Name> \
  --project src/AccountTransferService.Infrastructure \
  --startup-project src/AccountTransferService.Api \
  --output-dir Data/Migrations
```

To reset the database, stop the stack and remove the named volume: `docker compose down -v`.

## Local Development (without Docker)

Backend:

```bash
cd backend
dotnet run --project src/AccountTransferService.Api
```

Requires SQL Server reachable at the connection string in `appsettings.json` (defaults to `localhost,1433`). Easiest way to get one running is `docker compose up sqlserver`.

Frontend:

```bash
cd frontend
npm install
npm run dev
```

`vite.config.ts` proxies `/api` to `http://localhost:5000` in dev mode, matching the default `API_PORT`.

## Testing

```bash
cd backend
dotnet test
```

Unit tests cover `TransferService`: successful debit/credit, insufficient funds, unknown sender/receiver, and that a successful transfer is recorded in history. Tests run against EF Core's SQLite in-memory provider rather than the InMemory provider, because the transfer logic relies on `ExecuteUpdateAsync` and explicit relational transactions — neither of which the InMemory provider supports.

True concurrent-transfer behavior (see below) isn't exercised by these tests, since SQLite's single-writer model doesn't reproduce SQL Server's row-locking semantics; that's verified manually against the real database (see Design Decisions).

## Design Decisions & Trade-offs

- **Guid IDs.** Avoids exposing sequential, guessable account IDs through the API.
- **Atomic transfer logic.** The naive approach — load both accounts, check the balance in application code, then save — has a race: two concurrent transfers debiting the same account could both pass the balance check before either commits, overdrawing the account. Instead, `TransferService` performs the debit as `Accounts.Where(a => a.Id == senderId && a.Balance >= amount).ExecuteUpdateAsync(...)`, pushing the check-and-decrement into a single atomic SQL `UPDATE ... WHERE` statement. If zero rows are affected, the transfer is rejected as insufficient funds. The whole operation runs inside an explicit database transaction, and the two accounts involved are always touched in a fixed order (lower `Id` first) regardless of transfer direction, to avoid deadlocking with a concurrent transfer running in the opposite direction.
- **ProblemDetails + errorCode.** Errors follow RFC 7807 `ProblemDetails` with an added `errorCode` field, so the frontend can branch on a stable machine-readable code (`ACCOUNT_NOT_FOUND`, `INSUFFICIENT_FUNDS`) instead of parsing prose.
- **Only successful transfers are persisted** to the `Transfers` table; rejected attempts return an error but aren't logged to an audit trail (see Future Extensions).
- **No authentication.** Out of scope per the assignment — accounts are open/visible to anyone using the app.
- **SQL Server 2019 image, not 2022.** The `2022-latest` Linux container crashes on startup (`GetEndOfLibOSVmRange` assertion) on some Docker Desktop/host configurations. `2019-latest` is a well-known, stable workaround and has no impact on the schema or EF Core provider used.

## Future Extensions

- Authentication/authorization and per-user account ownership
- Multi-currency support with conversion
- Audit log of rejected/failed transfer attempts, not just successful ones
- Pagination on the transfer history endpoint
- Idempotency keys on `POST /api/transfers` to make retries safe

## Known Limitations

- Single currency (USD) for all seeded accounts; no conversion logic.
- Transfer history has no pagination — fine for a demo dataset, would need it at scale.
- Concurrency correctness (no negative balances under concurrent load) is guaranteed by the database-level atomic update, but isn't covered by an automated concurrency test — see Testing above.
