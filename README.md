# ICT.PaymentGateway

A payment gateway REST API built with **.NET 10 (C# 14)**, using **Clean Architecture** and **CQRS**.

> **Status:** Early scaffold. The solution structure exists; features are being built following [`docs/ROADMAP.md`](docs/ROADMAP.md).

## Goals
- **Clean Architecture:** the Domain has no dependency on frameworks, databases, or networks.
- **CQRS:** commands (change data) and queries (read data) are separate.
- **Idempotent payments:** the `Idempotency-Key` header, with a Redis lock, makes sure a network retry never charges a customer twice.
- **PCI-DSS aware:** the API never receives raw card numbers. Only tokens.
- **Standard errors:** RFC 7807 Problem Details.
- **Pluggable PSPs:** payment providers sit behind one interface (adapter pattern).

## Technology
| Area | Choice |
|---|---|
| Runtime | .NET 10, C# 14 |
| Mediator | MediatR or alternative (see licence note in `docs/ROADMAP.md`) |
| Validation | FluentValidation |
| Persistence | EF Core 10 (PostgreSQL or SQL Server, to be decided) |
| Cache and locks | Redis |
| API docs | OpenAPI + Scalar |
| Tests | xUnit, Testcontainers |

## Project structure
```text
ICT.PaymentGateway/
├── src/
│   ├── ICT.PaymentGateway.Domain          # Entities, value objects, rules
│   ├── ICT.PaymentGateway.Application     # Commands, queries, handlers, interfaces
│   ├── ICT.PaymentGateway.Infrastructure  # EF Core, Redis, PSP adapters
│   └── ICT.PaymentGateway.API             # Controllers, middleware, DI setup
├── tests/
│   ├── ICT.PaymentGateway.Domain.UnitTests
│   ├── ICT.PaymentGateway.Application.UnitTests
│   └── ICT.PaymentGateway.IntegrationTests
├── docs/                                  # Architecture, conventions, roadmap
├── CLAUDE.md                              # Instructions for AI agents
└── ICT.PaymentGateway.slnx
```

Dependency direction: `API -> Infrastructure -> Application -> Domain`

## Getting started
1. Install the **.NET 10 SDK**.
2. Clone the repo:
   ```bash
   git clone https://github.com/InnovationCoreTechnology/ICT.PaymentGateway.git
   cd ICT.PaymentGateway
   ```
3. Build and test:
   ```bash
   dotnet build
   dotnet test
   ```
4. Run the API:
   ```bash
   dotnet run --project src/ICT.PaymentGateway.API
   ```
   The API listens on `http://localhost:5151` (see `launchSettings.json`).

## Configuration and secrets
Never commit secrets. For local development use user-secrets:
```bash
cd src/ICT.PaymentGateway.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Default" "<your-connection-string>"
```

## Documentation
- [Architecture](docs/ARCHITECTURE.md)
- [Coding conventions](docs/CONVENTIONS.md)
- [Roadmap and task list](docs/ROADMAP.md)
- [AI agent instructions](CLAUDE.md)

## Contributing
1. Create a branch: `feature/<name>`.
2. Keep changes small, with tests.
3. Use Conventional Commits (`feat:`, `fix:`, `chore:`).
4. Open a pull request. CI must pass.
