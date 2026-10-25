# Roadmap and Task List

Pick **one** unchecked task at a time, top to bottom. Tick it when done.

## Decisions to confirm with the owner first
These are open. Agents must ask, not guess.
1. **Database:** DECIDED: PostgreSQL (Npgsql + EF Core).
2. **Mediator library:** DECIDED: small in-house dispatcher in Application (no package, no licence risk).
3. **First PSP to integrate:** Stripe, Adyen, a bank gateway, or an India-focused one? Start with a **fake PSP** (task 3.2) until decided.
4. **Merchant authentication:** API key or OAuth2 client credentials?
5. **Hosting target:** Docker / Kubernetes / Azure / AWS?
6. **Test libraries:** DECIDED: NSubstitute + Shouldly (+ Testcontainers.PostgreSql for integration tests).

## Phase 0 — Clean up the scaffold
- [x] 0.1 Run `dotnet build` and `dotnet test` and confirm they pass on a clean clone.
- [x] 0.2 Delete template files: `WeatherForecastController.cs`, `WeatherForecast.cs`, all `Class1.cs`, the weather request in `ICT.PaymentGateway.API.http`.
- [x] 0.3 Replace `Application.UnitTests/IntegrationTest1.cs` and remove the `Aspire.Hosting.Testing` package. It is an Aspire template and does not belong here. Add one simple passing test instead.
- [x] 0.4 Make test projects consistent: same package versions, `IsTestProject` set, add `ProjectReference` to the project each one tests (they have none today).
- [x] 0.5 Set up central package management (`Directory.Packages.props`) and remove `Version=` from each `.csproj`.
- [x] 0.6 Confirm `Directory.Build.props`, `.editorconfig`, `global.json` work (build with zero warnings).
- [ ] 0.7 Confirm GitHub Actions CI runs green.

## Phase 1 — Foundation
- [x] 1.1 Add mediator + pipeline behaviours (Logging, Validation) in Application.
- [x] 1.2 Add `Result` / `Error` types and the global exception handler returning Problem Details.
- [ ] 1.3 Add `Money` value object with unit tests (currency, rounding, negatives, add/subtract). (Money exists with validation tests; add/subtract still to do.)
- [x] 1.4 Add `/health/live` and `/health/ready`.
- [ ] 1.5 Add correlation ID middleware and structured logging.
- [x] 1.6 Add OpenAPI + Scalar UI (development only).
- [ ] 1.7 Add Dockerfile and `docker-compose.yml` (API + database + Redis).

## Phase 2 — Core payment domain
- [x] 2.1 `Payment` aggregate with status state machine and unit tests for every legal and illegal transition.
- [ ] 2.2 `Merchant` and `PaymentMethodToken` entities. (Not started: the API uses a temporary X-Merchant-Id header.)
- [x] 2.3 EF Core `DbContext`, configurations, first migration, repositories, unit of work.
- [x] 2.4 `CreatePaymentCommand` + validator + handler (using a fake PSP).
- [x] 2.5 `GetPaymentByIdQuery`.
- [ ] 2.6 `POST /api/v1/payments` and `GET /api/v1/payments/{id}` with integration tests (Testcontainers). (Endpoints and tests are written; integration tests still need a run on a machine with Docker.)

## Phase 3 — Idempotency and PSP adapters
- [ ] 3.1 Idempotency behaviour with Redis lock + stored response + request hash.
- [ ] 3.2 `IPspGateway` interface, PSP factory, and a `FakePspGateway` for tests and local use.
- [ ] 3.3 Real PSP adapter #1 (after decision 3), with timeouts, retries, circuit breaker.
- [ ] 3.4 Capture, cancel/void.
- [ ] 3.5 Refunds (full and partial).
- [x] 3.6 Subscriptions: Basic / Classic / Gold plans, pay to unlock (30-day term), `[RequiresPlan]` gating. Integration tests still need a run with Docker; prices in `appsettings.json` are placeholders.

## Phase 4 — Security and operations
- [ ] 4.1 Merchant authentication (after decision 4).
- [ ] 4.2 Rate limiting.
- [ ] 4.3 Webhook receiver from PSP with signature check, plus outgoing merchant webhooks (outbox pattern).
- [ ] 4.4 Audit log for payment state changes.
- [ ] 4.5 Metrics and tracing (OpenTelemetry).
- [ ] 4.6 Security review checklist (PCI-DSS scope, log masking, secrets).
