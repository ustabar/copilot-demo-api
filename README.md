# Contoso Customer API — demo repository

The working application behind the five live demos in **GitHub Copilot Deep Dive:
Models, Context and Chains**. It is a small, honest ASP.NET Core service — not a toy —
with deliberate seams placed where each demo needs one.

```
dotnet test      # 32 tests, green, under 5 seconds from a warm build
dotnet run --project src/Contoso.CustomerApi
```

---

## Why this repository exists

Every demo in the runbook needs the same three things: a codebase small enough to read
on a projector, a test suite fast enough to run live, and a real defect to find. Those
requirements pull against each other, so nothing here is accidental.

- **23 seeded customers.** With a page size of 10 that gives three pages and a partial
  last page — the exact shape that exposes off-by-one pagination bugs.
- **Deterministic GUIDs** (`00000000-0000-0000-0000-{index:D12}`) so scripts and tests
  can name a specific record.
- **32 tests in ~300 ms** so you can run the suite live without losing the room.
- **A stringly-typed error channel** that couples the service layer to the HTTP status
  mapping in a way the compiler cannot check.

---

## Architecture

```
src/Contoso.CustomerApi/
├── Program.cs                      Host, DI registration, endpoint mapping
├── Endpoints/
│   └── CustomerEndpoints.cs        HTTP binding + ToProblem status mapping
├── Services/
│   ├── CustomerService.cs          Application logic, returns Result<T>
│   └── PagingDefaults.cs           Paging bounds and clamping
├── Repositories/
│   ├── ICustomerRepository.cs
│   └── InMemoryCustomerRepository.cs   Seed data lives here
├── Validation/
│   └── CustomerValidator.cs
└── Models/
    ├── Customer.cs                 Domain record — carries InternalNotes
    ├── CustomerDtos.cs             Request/response DTOs — no InternalNotes
    ├── PagedResult.cs
    └── Result.cs                   Result<T> + ErrorCodes string constants

tests/Contoso.CustomerApi.Tests/
├── PaginationTests.cs              Boundary tests — Demo 03 breaks these
├── CustomerServiceTests.cs         Behaviour + authorization rules
└── EndpointTests.cs                HTTP contract — Demo 02 breaks these
```

### The two rules that matter

1. **`Customer` is never serialised directly.** It carries `InternalNotes`, which must
   not leave the process. Everything outbound goes through `CustomerResponse.From`.
2. **Every service error code needs an arm in `ToProblem`.** That switch matches on
   *string constants*, so a mismatch compiles cleanly and silently returns 500.

Both rules are enforced only by tests and review — never by the compiler. That is what
makes them useful demo material.

---

## API surface

| Method | Route | Notes |
| --- | --- | --- |
| `GET` | `/health` | Liveness |
| `GET` | `/api/customers?page=&pageSize=&country=` | Paged, 1-based, page size clamped to 100 |
| `GET` | `/api/customers/{id}` | 404 when absent |
| `POST` | `/api/customers` | 400 invalid, 409 duplicate email |
| `DELETE` | `/api/customers/{id}` | 403 for Enterprise without `X-Scope: customers.admin` |

`X-Scope` is a deliberate stand-in for a real identity provider so the authorization
branch is reachable without an auth stack. Treat the branch as real; the header itself
is a documented affordance, not the vulnerability.

---

## Which demo uses which seam

| Demo | Seam | Where |
| --- | --- | --- |
| **01** Baseline | An unimplemented method to complete, plus a real feature issue | `CustomerService.cs` scratch area; `demo/issues/demo-01-tier-filter.md` |
| **02** Model selection | Typed-error refactor whose status mapping is easy to miss | `Result.cs` + `CustomerEndpoints.ToProblem` |
| **03** Harness and loop | Off-by-one in the paging skip calculation | `CustomerService.GetCustomersAsync` |
| **04** Skills and context | Repository starts with no `.github/`; files staged for live install | `demo/context-files/` |
| **05** Chain and multi-model | Unauthorized export endpoint that leaks `InternalNotes` | Planted into `CustomerEndpoints.cs` |

---

## Demo scripts

All scripts are idempotent and safe to re-run.

```powershell
# Demo 03 — introduce the pagination off-by-one
.\demo\scripts\break-pagination.ps1
# build stays green; 5 PaginationTests go red

# Demo 05 — add the reviewable weakness
.\demo\scripts\plant-weakness.ps1
# build green, all 32 tests still pass — the suite does not catch it

# Demo 04 / 05 — install context files when you reach that step
.\demo\scripts\install-context.ps1 -Instructions
.\demo\scripts\install-context.ps1 -Skill
.\demo\scripts\install-context.ps1 -Agents
.\demo\scripts\install-context.ps1 -All

# between every demo
.\demo\scripts\reset.ps1
.\demo\scripts\reset.ps1 -SkipTests    # faster, when you are mid-sequence
```

`reset.ps1` discards working-tree changes, removes the installed `.github` folder, and
re-runs the suite to prove you are back at a known-good state.

### Proving the Demo 05 leak

```powershell
dotnet run --project src/Contoso.CustomerApi --urls http://localhost:5199

# clean — projected through CustomerResponse.From
curl "http://localhost:5199/api/customers?pageSize=100"

# leaks InternalNotes — serialises the domain record, and has no auth check
curl "http://localhost:5199/api/customers/export"
```

---

## Demo assets

```
demo/
├── prompts/
│   ├── demo-02-model-comparison.md      The controlled prompt + the scorecard
│   └── demo-04-adhoc-security-prompt.md The long "before" prompt
├── issues/
│   ├── demo-01-tier-filter.md           Paste into GitHub before Demo 01
│   └── demo-05-soft-delete.md           Paste into GitHub before Demo 05
├── context-files/                       Staged OUTSIDE .github on purpose
│   ├── copilot-instructions.md
│   ├── instructions/backend.instructions.md
│   ├── skills/api-security-review/{SKILL.md, checklist.md}
│   └── agents/{planner, implementer, test-engineer, security-reviewer}.agent.md
└── scripts/
    ├── break-pagination.ps1
    ├── plant-weakness.ps1
    ├── install-context.ps1
    └── reset.ps1
```

Context files live outside `.github/` so Demo 04 can start from a genuinely empty
repository and add them in front of the audience.

---

## Before you present

```powershell
git clone <this repo> && cd copilot-demo-api
dotnet test                        # must be green
.\demo\scripts\reset.ps1           # confirms the reset path works
```

Then work through Appendix C of `github-copilot-demo-runbook.docx`.

The SDK is pinned in `global.json` to 10.0.303 with `latestPatch` roll-forward, so a
preview SDK on the machine cannot change behaviour on the day.

---

## Notes

- No external dependencies beyond the ASP.NET Core shared framework and
  `Microsoft.AspNetCore.Mvc.Testing`. Nothing to restore from a private feed.
- Storage is in-memory and reseeded per process, so a botched demo cannot corrupt state.
- The repository has no CI workflow by design — Demo 05 adds the pull request and the
  branch protection gate is configured on GitHub, not in the repository.
