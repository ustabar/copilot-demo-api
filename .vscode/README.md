# Workspace MCP configuration

This folder pins **one** MCP server for the demo, scoped deliberately.

```jsonc
// .vscode/mcp.json
"github": {
  "type": "http",
  "url": "https://api.githubcopilot.com/mcp/",
  "headers": {
    "X-MCP-Toolsets": "repos,issues",   // not the full surface
    "X-MCP-Readonly": "true"            // no write tools at all
  }
}
```

## Why it is scoped

| Configuration | Tools exposed |
| --- | --- |
| Default (all toolsets) | **47** |
| `repos,issues` + readonly | **19** |

Every enabled tool contributes its definition to the request **before you type a single
word**. Loading 47 tool definitions on every turn to use one of them is exactly the
fixed cost that Part 03 of the session is about. Scoping the toolset is the same
discipline as scoping an attachment.

It is also least privilege: during Demo 03 the agent reads live repository and issue
context through this server, and it is structurally incapable of writing anything.

## What this means for the demos

**Demo 03** — the agent calls a read tool here (`issue_read`, `get_file_contents`,
`search_code`) to pull live context before it edits anything. That tool call is the
visual centrepiece of the demo. Pause on it.

**Demo 05** — the pull request is created from the terminal with `gh pr create`, not
through MCP. Two reasons: the server is readonly by design, and a terminal command is
more visible on a projector than a silent tool call.

## Before presenting

VS Code loads **user-level** MCP servers as well as this workspace file. Check
`%APPDATA%\Code\User\mcp.json` — if you have a long list registered there, disable the
ones you are not using for the session. A cluttered tool picker undermines the exact
point you are making on screen.

Verify the server responds:

1. Open the Chat view, switch to Agent mode.
2. Click the tools icon and confirm `github` is listed and enabled.
3. Ask: `Using the github tool, summarise issue #2 in this repository.`
4. You should see the tool call render, then a summary of the tier-filtering issue.

If the server does not respond, fall back to the built-in file and terminal tools and
say so out loud — unavailability is itself the lesson from slide 13.
