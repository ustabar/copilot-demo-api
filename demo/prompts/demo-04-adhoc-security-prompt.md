# Demo 04 — the long ad-hoc prompt (the "before" state)

This is the prompt you paste **before** any instruction files or skills exist. Its
length is the point. Read the first few lines aloud so the room registers how long it is.

---

```text
I need you to do a security review of this API. Please look at authentication and
authorization first, and check whether every endpoint that changes or deletes data
actually verifies the caller has permission to do it. Then check the input validation
- I want to know if there is any endpoint where a caller can send a payload that
reaches the service layer without being validated, including things like overly long
strings, missing required fields, or values outside the allowed range. Also look at
whether any internal or sensitive field can be returned to a caller who should not
see it, because we have a field on the customer record that is for internal use only
and must never appear in an API response. Check the error handling as well - I want
to know if any error response leaks a stack trace, a connection string, an internal
path, or any other detail about how the system is built. Look at the logging too and
tell me if we are writing anything sensitive to the logs. Check the dependencies for
anything obviously out of date or risky. Please do not change any files, I only want
a report. For each thing you find, give me the severity, the file and line, what the
actual risk is, and what you would do about it. Sort them by severity with the worst
first. If there is anything you could not review, say so explicitly rather than
leaving it out silently. Use the repository's existing conventions when you suggest
a fix, and do not suggest introducing a new library unless there is no alternative.
```

---

## Why it needs a correction round

The prompt never states the repository's conventions, so the model has no way to know:

- that the project uses a `Result<T>` type and does not throw for expected failures,
- that `CustomerResponse.From` is the only sanctioned way to project a customer outward,
- that `X-Scope` is the demo stand-in for a real identity provider.

It will therefore either flag the `X-Scope` header as the finding (it is a known
demo affordance, not the bug) or propose a fix that throws an exception. Either way
you spend a round correcting it.

That round is the cost you remove in the next two steps.

---

## The "after" state

Once `.github/copilot-instructions.md`, the path-specific instructions and the
`api-security-review` skill are installed, the entire prompt above collapses to:

```text
Run the API security review skill on this repository.
```

Compare the two outputs side by side. Same or better quality, from one line of input,
with the conventions applied and the read-only boundary enforced.

---

## The attachment comparison

Stage any 100+ page PDF plus a short extract of the relevant section, then ask the
**identical question** of both:

```text
According to this document, what is the stated retention period for customer records,
and which section states it?
```

Ask the room the diagnostic question afterwards:

> What percentage of that document did the model genuinely have to read?
