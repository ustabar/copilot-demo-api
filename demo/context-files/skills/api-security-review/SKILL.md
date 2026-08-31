---
name: api-security-review
description: Reviews API implementations for common security and configuration risks. Read-only.
---

# API Security Review

When this skill is selected:

1. Identify the API technology and project structure.
2. Locate authentication and authorization components.
3. Review request validation.
4. Review secret and credential handling.
5. Check error responses for sensitive information.
6. Review logging behaviour for sensitive data.
7. Check that every endpoint returning customer data projects through
   `CustomerResponse.From` rather than serialising the domain record.
8. Check that every service error code has a matching arm in `ToProblem`.
9. Produce findings.

## Boundaries

- Do not modify any file.
- Do not run destructive commands.
- If information is missing, list the assumptions separately.
- `X-Scope` is a documented demo affordance standing in for an identity provider.
  Report weaknesses in how it is *used*, not its existence.

## Output format

For each finding, report:

- Severity (High / Medium / Low)
- Evidence (the specific code or configuration)
- Affected file and line
- Risk if unaddressed
- Recommended remediation

Sort findings by severity, highest first.

End with a section titled "Scope not covered" listing anything you did not review.
