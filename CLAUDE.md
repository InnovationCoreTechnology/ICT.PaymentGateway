# CLAUDE.md — Instructions for AI Agents

Read this file first, every session. Then read `docs/ROADMAP.md` to find your task.

## 1. What this project is
`ICT.PaymentGateway` is a Payment Gateway REST API (.NET 10, C# 14).
It takes payment requests from merchants, sends them to external Payment Service Providers (PSPs), and returns safe, consistent results.
It uses Clean Architecture + CQRS. See `docs/ARCHITECTURE.md` for the full design.

Owner: Innovation Core Technology. Repo: https://github.com/InnovationCoreTechnology/ICT.PaymentGateway

## 2. Solution layout
```
src/
  ICT.PaymentGateway.Domain          # Entities, value objects, domain rules. NO dependencies.
  ICT.PaymentGateway.Application     # Commands, queries, handlers, validators, interfaces. Depends on Domain.
  ICT.PaymentGateway.Infrastructure  # EF Core, Redis, PSP adapters. Depends on Application + Domain.
  ICT.PaymentGateway.API             # Controllers, middleware, DI setup. Composition root.
tests/
  ICT.PaymentGateway.Domain.UnitTests
  ICT.PaymentGateway.Application.UnitTests
  ICT.PaymentGateway.IntegrationTests
```
Solution file: `ICT.PaymentGateway.slnx`

## 3. Commands (run from repo root)
```
dotnet restore
dotnet build -c Release
dotnet test -c Release
dotnet format --verify-no-changes
dotnet run --project src/ICT.PaymentGateway.API
```
Before you say a task is done: build must pass, all tests must pass, and `dotnet format` must be clean.

## 4. Hard rules (never break)
1. **Dependency direction:** Domain <- Application <- Infrastructure <- API. Domain references nothing. Application never references Infrastructure or EF Core. Never add a reference that breaks this.
2. **No raw card data.** Never accept, log, store, or return a full card number (PAN), CVV, or expiry. Only tokens. This is a PCI-DSS rule.
3. **No secrets in Git.** No keys, passwords, connection strings with passwords. Use user-secrets or environment variables. `appsettings.json` has safe defaults only.
4. **Money = `decimal` amount + ISO 4217 currency code** in a `Money` value object. Never `double` or `float`.
5. **Every payment-creating endpoint requires an `Idempotency-Key` header.** Same key + same body returns the original result. Same key + different body returns 422.
6. **Errors use RFC 7807 Problem Details.** Never return raw exception messages or stack traces.
7. **Do not log sensitive data** (tokens, auth headers, customer personal data). Mask it.
8. **Ask before adding a NuGet package** or changing the tech stack. Explain why and offer alternatives. (Note: check the MediatR licence first — see `docs/ROADMAP.md`, Decisions.)
9. **Never push to `main`.** Work on a branch, open a pull request.
10. **Never delete or rewrite tests to make them pass.** Fix the code, or explain why the test is wrong.

## 5. How to work
1. Pick **one** task from `docs/ROADMAP.md`. Do not mix tasks.
2. Make a short plan (3-6 bullets) before writing code. For big changes, show the plan and wait.
3. Write the test first (or together with the code) for every behaviour change.
4. Keep changes small. One PR = one task.
5. Follow `docs/CONVENTIONS.md` for naming, style, and patterns.
6. When finished: tick the task in `docs/ROADMAP.md`, and update `docs/ARCHITECTURE.md` if the design changed.
7. If something is unclear, ask a short question. Do not guess on money, security, or data-model decisions.

## 6. Git rules
- Branch names: `feature/<short-name>`, `fix/<short-name>`, `chore/<short-name>`
- Commit messages: Conventional Commits, e.g. `feat(payments): add CreatePayment command`
- Small, focused commits. No unrelated formatting changes.

## 7. When you finish a task, reply with
1. What you changed (files, one line each)
2. How you tested it (commands + result)
3. Any risk, assumption, or follow-up needed
