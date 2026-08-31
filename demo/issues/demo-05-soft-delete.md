# Issue for Demo 05 — paste this into GitHub

**Title**

```text
Add soft-delete support for customer records
```

**Body**

```markdown
## Context

Deleting a customer today removes the record permanently. Finance has asked for a
30-day recovery window so an accidental deletion can be reversed, and compliance
wants deletions to be auditable rather than silent.

## Requirement

Replace the hard delete with a soft delete.

- `DELETE /api/customers/{id}` marks the record deleted instead of removing it.
- Soft-deleted records are excluded from `GET /api/customers` and return 404 on
  `GET /api/customers/{id}`.
- The existing authorization rule is unchanged: deleting an **Enterprise** customer
  still requires the `customers.admin` scope.
- Record who performed the deletion and when.

## Acceptance criteria

- [ ] A deleted customer no longer appears in the list or by id.
- [ ] `DELETE` still returns 204 on success.
- [ ] Deleting an Enterprise customer without `customers.admin` still returns 403.
- [ ] Deletion metadata (timestamp, actor) is recorded on the customer record.
- [ ] Existing tests continue to pass.

## Out of scope

- The restore endpoint. That is a follow-up issue.
- Any change to create or list behaviour beyond excluding deleted records.
```

---

## Why this issue

It is genuinely multi-stage work: it needs a design decision (where the deleted flag
lives and how it propagates through the repository), a careful implementation, new
tests, and a security review — which is exactly the shape a four-agent chain is for.

It also deliberately touches the authorization branch, so the security reviewer has a
real reason to look at that area and find the planted export endpoint next to it.

## Presenter sequence

```powershell
.\demo\scripts\reset.ps1 -SkipTests
.\demo\scripts\plant-weakness.ps1     # adds the reviewable export endpoint
dotnet test                            # confirm still green - this matters
.\demo\scripts\install-context.ps1 -All
```

Then run the chain: planner → approve → implementer → test engineer → security reviewer → PR.

Do **not** merge. The blocked merge is the closing image.
