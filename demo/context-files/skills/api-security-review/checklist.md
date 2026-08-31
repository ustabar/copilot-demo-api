# API Security Review Checklist

Referenced by `SKILL.md`. Kept separate so the skill body stays short.

## Authorization

- [ ] Every mutating endpoint (`POST`, `PUT`, `PATCH`, `DELETE`) verifies the caller.
- [ ] Privileged operations check a scope, not just authentication.
- [ ] The authorization decision happens in the service layer, not only at the edge.
- [ ] No endpoint bypasses a check that a neighbouring endpoint enforces.

## Data exposure

- [ ] No endpoint serialises a domain record directly.
- [ ] `InternalNotes` appears in no response payload.
- [ ] Collection endpoints project each element, not just the wrapper.
- [ ] Error details do not echo inbound payloads verbatim.

## Input validation

- [ ] Every inbound payload passes through a validator.
- [ ] String lengths are bounded.
- [ ] Enum values are checked with `Enum.IsDefined`.
- [ ] Paging values are clamped rather than trusted.

## Error handling

- [ ] No stack trace, connection string or internal path reaches a response.
- [ ] Every service error code maps to an intentional status code.
- [ ] The fallback arm returns 500 and logs, rather than leaking the cause.

## Logging

- [ ] No email address, credential or internal note is written to a log.
- [ ] Authorization failures are logged with enough context to investigate.

## Dependencies

- [ ] No package with a known advisory.
- [ ] No new dependency introduced without a stated reason.
