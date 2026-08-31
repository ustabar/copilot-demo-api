---
applyTo: "src/**"
---

# Backend Instructions

- Endpoints stay thin; business logic belongs in the service layer.
- All public endpoints validate input before reaching the service layer.
- Use the existing `Result<T>` type for error paths; do not throw for expected failures.
- Every new endpoint needs at least one positive and one negative test.
- Database and storage access goes through the repository interfaces only.
- Every error code returned by a service must have a matching arm in
  `CustomerEndpoints.ToProblem`. Adding one without the other silently produces a 500.
- Any endpoint that returns customer data must project through `CustomerResponse.From`.
