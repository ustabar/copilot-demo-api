<#
    Restores the repository to a known-good state between demos.

    Discards every working-tree change, removes untracked files that the demos
    create (the .github context files, agent definitions and any scratch code),
    and confirms the suite is green again.
#>
[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)),
    [switch]$SkipTests
)

$ErrorActionPreference = 'Stop'
Push-Location $Root

try {
    if (Test-Path (Join-Path $Root '.git')) {
        Write-Host 'Discarding working-tree changes...' -ForegroundColor Cyan
        git checkout -- . 2>&1 | Out-Null
        git clean -fd --exclude=demo/scratch 2>&1 | Out-Null
    }
    else {
        Write-Host 'Not a git repository - removing demo-created files manually.' -ForegroundColor Yellow
        Remove-Item (Join-Path $Root '.github') -Recurse -Force -ErrorAction SilentlyContinue
    }

    if (-not $SkipTests) {
        Write-Host 'Running tests...' -ForegroundColor Cyan
        dotnet test --nologo -v q
        if ($LASTEXITCODE -ne 0) {
            throw 'Tests are not green after reset. Investigate before presenting.'
        }
    }

    Write-Host ''
    Write-Host 'Repository reset. Ready for the next demo.' -ForegroundColor Green
}
finally {
    Pop-Location
}
