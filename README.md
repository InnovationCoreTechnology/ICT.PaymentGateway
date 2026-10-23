# ICT.PaymentGateway.API

A high-performance, enterprise Payment Gateway REST API built with **.NET 10**, strictly following **Clean Architecture** (Onion Architecture) and **CQRS (Command Query Responsibility Segregation)** design patterns.

---

## 🏛 Architectural Principles

- **Clean Architecture**: Domain-centric design where the enterprise core remains independent of external frameworks, databases, and network dependencies.
- **CQRS Pattern**: Strict separation of mutating actions (Commands) from read operations (Queries) via MediatR pipeline behaviors.
- **Transactional Idempotency**: Header-enforced (`Idempotency-Key`) distributed locking via Redis to ensure network retries never double-charge customers.
- **PCI-DSS Compliant Workflows**: Zero raw Primary Account Number (PAN) ingestion; uses tokenized card and payment method representations.
- **Standardized Diagnostic Responses**: Consistent error contracts using RFC 7807 (Problem Details for HTTP APIs).
- **Pluggable Payment Service Providers (PSP)**: Adapter pattern to support external payment aggregators (Stripe, Adyen, Banking Gateways).

---

## 💻 Technology Stack

- **Runtime & Language**: .NET 10 | C# 14
- **Patterns**: Clean Architecture, CQRS, Repository & Unit of Work, Factory Pattern
- **Messaging & Mediator**: MediatR with pipeline behaviors (Validation, Logging, Idempotency)
- **Validation**: FluentValidation
- **Persistence**: Entity Framework Core 10 (PostgreSQL / SQL Server)
- **Caching & Locking**: Redis (Distributed Caching & RedLock)
- **API Documentation**: OpenAPI / Scalar / Swagger

---

## 📂 Layer Breakdown

```text
ICT.PaymentGateway/
├── src/
│   ├── ICT.PaymentGateway