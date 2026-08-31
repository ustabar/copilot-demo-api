# Issue for Demo 01 — paste this into GitHub

**Title**

```text
Add tier filtering to GET /api/customers
```

**Body**

```markdown
## Context

The customer list endpoint supports paging and a `country` filter. The support team
also needs to narrow the list by tier when they are triaging escalations, and they
currently pull the whole list and filter client-side.

## Requirement

Add an optional `tier` query parameter to `GET /api/customers`.

- `GET /api/customers?tier=Enterprise` returns only Enterprise customers.
- The filter combines with the existing `country` filter.
- Paging metadata must describe the **filtered** set, not the whole set.
- An unrecognised tier value returns 400, not an empty page.

## Acceptance criteria

- [ ] `tier` is optional; omitting it preserves today's behaviour exactly.
- [ ] `tier` and `country` can be combined.
- [ ] `totalCount` and `totalPages` reflect the filtered set.
- [ ] An invalid tier value returns 400 with a validation problem response.
- [ ] Tests cover: tier only, tier + country, invalid tier, and tier omitted.

## Out of scope

- No change to the response shape.
- No change to the delete or create endpoints.
```

---

## Why this issue

It is a real feature request with unambiguous acceptance criteria, which is what
agent mode needs to succeed. A vague issue makes agent mode look worse than it is,
and that is not an honest demonstration.

It also lands in the same method Demo 03 will break, so the room has already seen
`GetCustomersAsync` by the time pagination matters.

## Note for the presenter

Do **not** merge the result. Demo 01 ends after the agent reports back; reset with
`.\demo\scripts\reset.ps1` before Demo 02.
