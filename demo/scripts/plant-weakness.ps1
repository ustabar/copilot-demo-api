<#
    Demo 05 - plant a reviewable weakness.

    Adds a "legacy export" endpoint that looks like ordinary maintenance code and
    passes every existing test, because no test covers it. It has two real defects:

      1. It serialises the raw Customer record, so InternalNotes leaks to any caller.
      2. It has no authorization check at all, unlike the delete endpoint next to it.

    This is what the security-reviewer agent should find. The test-engineer agent
    will report green, which is exactly the point: a green suite is not a review.
#>
[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
)

$ErrorActionPreference = 'Stop'

$file = Join-Path $Root 'src\Contoso.CustomerApi\Endpoints\CustomerEndpoints.cs'
if (-not (Test-Path $file)) { throw "Cannot find CustomerEndpoints.cs at $file" }

$content = Get-Content $file -Raw

if ($content -match 'MapGet\("/export"') {
    Write-Host 'Weakness already planted - nothing to do.' -ForegroundColor Yellow
    exit 0
}

$anchor = '        return app;'
if ($content -notmatch [regex]::Escape($anchor)) {
    throw 'Could not find the insertion point in CustomerEndpoints.cs.'
}

$planted = @'
        // Legacy export used by the finance batch job.
        // TODO: migrate the batch job to the v2 reporting API and retire this.
        group.MapGet("/export", async (
            CustomerService service,
            CancellationToken ct) =>
        {
            var result = await service.GetCustomersAsync(1, PagingDefaults.MaxPageSize, null, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value!.Items)
                : ToProblem(result.ErrorCode, result.ErrorMessage);
        })
        .WithName("ExportCustomers");

        return app;
'@

$content.Replace($anchor, $planted) | Set-Content $file -Encoding utf8 -NoNewline

Write-Host 'Weakness planted in CustomerEndpoints.cs.' -ForegroundColor Green
Write-Host ''
Write-Host 'Two defects, neither caught by the test suite:' -ForegroundColor Cyan
Write-Host '  1. Returns the raw Customer record - InternalNotes is exposed.'
Write-Host '  2. No authorization check, unlike the delete endpoint beside it.'
Write-Host ''
Write-Host 'Confirm the suite is still green:  dotnet test' -ForegroundColor Cyan
Write-Host 'Then let the security-reviewer agent find it.' -ForegroundColor Cyan
