# Coding Conventions

## 1. C# style
- File-scoped namespaces (`namespace X;`).
- `Nullable` is on. No `!` (null-forgiving) unless you add a comment explaining why.
- Prefer `record` for commands, queries, DTOs, and value objects.
- Use `sealed` on classes that are not designed for inheritance.
- Async methods end with `Async` and take a `CancellationToken` as the last parameter. Pass it all the way down.
- No `async void`. No `.Result` or `.Wait()`.
- Use `TimeProvider`, not `DateTime.UtcNow`.
- One public type per file. File name = type name.

## 2. Naming
| Thing | Example |
|---|---|
| Command | `CreatePaymentCommand`, handler `CreatePaymentCommandHandler`, validator `CreatePaymentCommandValidator` |
| Query | `GetPaymentByIdQuery` |
| Interface | `IPaymentRepository` |
| PSP adapter | `StripePspGateway`, `AdyenPspGateway` |
| Test class | `CreatePaymentCommandHandlerTests` |
| Test method | `Handle_WhenKeyAlreadyUsed_ReturnsStoredResponse` |

## 3. Folder layout inside Application
```
Application/
  Common/            # behaviours, interfaces, result types
  Payments/
    Commands/CreatePayment/   # command + handler + validator together
    Queries/GetPaymentById/
  Refunds/
```
Group by feature, not by technical type.

## 4. API rules
- Routes: lowercase, plural, versioned: `/api/v1/payments`.
- Controllers stay thin: map request -> send to mediator -> map result. No business logic.
- Use correct status codes: `201` create, `200` read, `202` accepted async, `400` validation, `401/403` auth, `404` not found, `409` conflict, `422` idempotency mismatch.
- Never expose domain entities. Return response DTOs.
- Every endpoint has OpenAPI summary and response types.

## 5. Error handling
- Expected failures (validation, not found, declined) -> `Result` / `Error` type, mapped to Problem Details.
- Unexpected failures -> exception, caught by the global handler, logged, generic Problem Details returned.

## 6. Data (EF Core)
- Entity configuration in separate `IEntityTypeConfiguration<T>` classes.
- Money stored as `decimal(19,4)` + `char(3)` currency.
- Migrations live in Infrastructure. Name them clearly: `AddPaymentsTable`.
- No lazy loading. No EF types leaking out of Infrastructure.

## 7. Tests
- Arrange / Act / Assert, with blank lines between.
- One behaviour per test. No logic (`if`, `for`) inside tests.
- Do not call real PSPs in tests. Use fakes or a stub server.
- Integration tests use Testcontainers, never a shared developer database.

## 8. Packages
- Keep versions in one place (`Directory.Packages.props`, once central package management is set up).
- Ask the owner before adding any new package.
