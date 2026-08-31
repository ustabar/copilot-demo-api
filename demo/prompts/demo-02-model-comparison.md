# Demo 02 — the controlled prompt

Paste this **identical text** for every model run. Any variation invalidates the
comparison, which is the entire point of the demo.

The task is deliberately cross-cutting: it touches five files and has one dependency
that is not visible from the file you start in.

---

```text
Refactor the error channel in this service so failures are typed instead of stringly-typed.

Requirements:
1. Replace the `string ErrorCode` on Result<T> with a `DomainError` enum.
2. Update every call site in the service layer to use the enum.
3. Update the endpoint layer so each error still maps to the correct HTTP status code:
   not found -> 404, validation -> 400, duplicate email -> 409, forbidden -> 403.
4. Do not change any public HTTP contract. Status codes and response shapes stay identical.
5. Keep the existing tests passing.

Report which files you changed and why.
```

---

## The trap

`CustomerEndpoints.ToProblem` matches on **string constants**. If a model changes
`Result<T>` and the service layer but leaves `ToProblem` matching strings, the code
still compiles — the switch just falls through to the `_` arm and returns **500 for
every error**.

The build is green. The API is broken.

Only `EndpointTests` catches it:

- `GetCustomerById_Returns404_ForUnknownId`
- `CreateCustomer_Returns400_ForInvalidPayload`
- `CreateCustomer_Returns409_ForDuplicateEmail`
- `DeleteEnterpriseCustomer_Returns403_WithoutAdminScope`

## What you are counting

| Model tier | Correction rounds | Correct at the end? |
| --- | --- | --- |
| Fast | | |
| Balanced | | |
| Reasoning | | |

A "correction round" is any message you send after the first one to get the task
finished. Count it even if it is one word.

## Running order

```powershell
# before EVERY run
.\demo\scripts\reset.ps1 -SkipTests
# then start a NEW chat so no history carries over
```

## Expected shape of the result

Do not promise the room a specific outcome — run it honestly. In rehearsal the usual
pattern is that the fast tier misses `ToProblem` entirely, the balanced tier finds it
after one nudge, and the reasoning tier finds it unprompted. If your run differs, say
so; the measurement is the argument, not the prediction.
