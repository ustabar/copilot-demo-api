<#
    Demo 04 - install the .github context files.

    Demo 04 starts from a repository with NO instruction files, so the before-state
    is honest. Run this only when you reach the "add repository instructions" step,
    or run it with -All beforehand if you would rather paste from the editor.

    Everything it installs lives in demo/context-files/, so nothing is generated
    on the fly and you can read the files to the audience.
#>
[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)),
    [switch]$Instructions,
    [switch]$Skill,
    [switch]$Agents,
    [switch]$All
)

$ErrorActionPreference = 'Stop'

$src = Join-Path $Root 'demo\context-files'
$dst = Join-Path $Root '.github'

if (-not ($Instructions -or $Skill -or $Agents -or $All)) {
    Write-Host 'Nothing selected. Use -Instructions, -Skill, -Agents or -All.' -ForegroundColor Yellow
    exit 1
}

New-Item -ItemType Directory -Force -Path $dst | Out-Null

if ($Instructions -or $All) {
    Copy-Item (Join-Path $src 'copilot-instructions.md') $dst -Force
    New-Item -ItemType Directory -Force -Path (Join-Path $dst 'instructions') | Out-Null
    Copy-Item (Join-Path $src 'instructions\*') (Join-Path $dst 'instructions') -Force
    Write-Host 'Installed .github/copilot-instructions.md and .github/instructions/' -ForegroundColor Green
}

if ($Skill -or $All) {
    $skillDst = Join-Path $dst 'skills\api-security-review'
    New-Item -ItemType Directory -Force -Path $skillDst | Out-Null
    Copy-Item (Join-Path $src 'skills\api-security-review\*') $skillDst -Recurse -Force
    Write-Host 'Installed .github/skills/api-security-review/' -ForegroundColor Green
}

if ($Agents -or $All) {
    $agentDst = Join-Path $dst 'agents'
    New-Item -ItemType Directory -Force -Path $agentDst | Out-Null
    Copy-Item (Join-Path $src 'agents\*') $agentDst -Force
    Write-Host 'Installed .github/agents/' -ForegroundColor Green
}

Write-Host ''
Write-Host 'Reload the VS Code window so Copilot picks up the new files.' -ForegroundColor Cyan
