# Architecture

## 1. Style
- **Clean Architecture** (Onion): business rules in the centre, technology on the outside.
- **CQRS**: Commands change state. Queries only read. They are separate classes and handlers.
- **Pluggable PSPs**: each payment provider (Stripe, Adyen, a bank gateway, etc.) is an adapter behind one interface.

## 2. Layers and dependencies
```
API  ->  Application  ->  Domain
API  ->  Infrastructure -> Application, Domain
```
| Layer | Contains | Must NOT contain |
|---|---|---|
| Domain | Entities, value objects (`Money`), enums, domain events, domain exceptions | Any NuGet framework reference, EF Core, HTTP |
| Application | Commands, queries, handlers, validators, pipeline behaviours, interfaces (`IPaymentRepository`, `IPspGateway`, `IUnitOfWork`, `IIdempotencyStore`), DTOs | EF Core, Redis, HttpClient, ASP.NET types |
| Infrastructure | EF Core `DbContext`, repositories, Redis, PSP adapters, clock, outbox | Business rules |
| API | Controllers, middleware, auth, DI registration, OpenAPI | Business rules, DB access |

## 3. Request flow (create payment)
1. Controller receives request and the `Idempotency-Key` header.
2. Controller sends a `CreatePaymentCommand` through the mediator.
3. Pipeline behaviours run in this order: **Logging -> Validation** (idempotency is handled inside `CreatePaymentCommandHandler` for now). The mediator is an in-house dispatcher (`Application/Common/Messaging`).
4. Handler builds the `Payment` aggregate, calls `IPspGateway` (via a PSP factory), saves, and returns a result.
5. Result is mapped to a response DTO. Errors map to RFC 7807 Problem Details.

## 4. Idempotency
- Key comes from the `Idempotency-Key` header, scoped per merchant.
- Redis lock (RedLock) stops two requests with the same key from running at once.
- The stored record holds: key, request hash, response, status, expiry.
- Same key + same hash -> return stored response. Same key + different hash -> `422`.
- **Current implementation (until roadmap 3.1):** stored in PostgreSQL (`idempotency_records`, unique on merchant + key) and the original payment is re-read on replay. Two truly simultaneous requests are stopped by the unique index (second gets `409`), but both may reach the PSP. The Redis lock fixes that.

## 5. Payment lifecycle (starting design — confirm before building)
`Created -> Pending -> Authorized -> Captured -> (Refunded | PartiallyRefunded)`
Failure paths: `Failed`, `Cancelled`, `Expired`.
Illegal transitions throw a domain exception. The state machine lives inside the `Payment` aggregate.

## 6. Security and compliance
- Card data is tokenised by the PSP or a vault. This API never sees a PAN.
- Authentication for merchants: API key or OAuth2 client credentials (decision pending).
- All secrets via user-secrets (local) or environment / secret manager (deployed).
- Audit log for every state change on a payment.
- Sensitive fields are masked in logs.

## 7. Cross-cutting
| Concern | Approach |
|---|---|
| Validation | FluentValidation in a pipeline behaviour |
| Errors | RFC 7807 via global exception handler |
| Logging | Structured logging with correlation ID on every request |
| Time | `TimeProvider` injected, never `DateTime.UtcNow` directly |
| Config | Options pattern with validation on start-up |
| Health | `/health/live` and `/health/ready` |
| API docs | OpenAPI (`Microsoft.AspNetCore.OpenApi`) + Scalar UI |

## 8. Testing strategy
| Project | What it tests | Tools |
|---|---|---|
| Domain.UnitTests | Entities, value objects, state transitions | xUnit |
| Application.UnitTests | Handlers, validators, behaviours with fakes | xUnit, NSubstitute (or Moq), FluentAssertions |
| IntegrationTests | Real API + real DB + real Redis | `WebApplicationFactory`, Testcontainers |

Target: Domain and Application at least 80% line coverage. Every bug fix gets a regression test.
