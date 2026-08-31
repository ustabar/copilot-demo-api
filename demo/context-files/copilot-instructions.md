# Repository Instructions

## Architecture

This is a layered ASP.NET Core minimal API.

- `Endpoints/` binds HTTP and translates results. No business logic.
- `Services/` holds application logic and returns `Result<T>`.
- `Repositories/` is the only place that touches storage.
- `Validation/` validates inbound payloads before they reach a service.
- `Models/` holds domain records and the request/response DTOs.

## Rules

- Follow the existing architecture and coding conventions.
- Do not introduce a new dependency without explaining the reason.
- Never place secrets, tokens, or passwords in source files.
- Run the relevant tests after every implementation change.
- Prefer minimal and reversible changes.
- Do not modify production configuration without explicit approval.
- Summarize changed files and test results at the end of the task.

## Conventions that are easy to get wrong

- Expected failures return `Result<T>.Failure(...)`. Do **not** throw for them.
- A customer is projected outward only through `CustomerResponse.From`. Never
  serialise the `Customer` record directly - it carries `InternalNotes`, which must
  never leave the process.
- Pagination is 1-based. Page 1 is the first page.
- `X-Scope` is a demo stand-in for a real identity provider. Treat the authorization
  branch it feeds as real; do not report the header itself as the vulnerability.
