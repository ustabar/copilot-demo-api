<#
    Demo 03 - break the pagination boundary.

    Introduces a genuine off-by-one: the skip calculation forgets that pages are
    1-based. Page 1 then skips the first page of results entirely, so the first
    record a caller ever sees is record 11.

    The build stays green. Four tests go red. That is the shape you want on stage:
    the compiler cannot help you, only the suite can.
#>
[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
)

$ErrorActionPreference = 'Stop'

$file = Join-Path $Root 'src\Contoso.CustomerApi\Services\CustomerService.cs'
if (-not (Test-Path $file)) { throw "Cannot find CustomerService.cs at $file" }

$original = 'var skip = (p - 1) * size;'
$broken   = 'var skip = p * size;'

$content = Get-Content $file -Raw

if ($content -notmatch [regex]::Escape($original)) {
    if ($content -match [regex]::Escape($broken)) {
        Write-Host 'Already broken - nothing to do.' -ForegroundColor Yellow
        exit 0
    }
    throw "Could not find the skip calculation to break. Has CustomerService.cs been edited?"
}

$content.Replace($original, $broken) | Set-Content $file -Encoding utf8 -NoNewline

Write-Host 'Pagination broken.' -ForegroundColor Green
Write-Host "  $original   ->   $broken"
Write-Host ''
Write-Host 'Verify with:  dotnet test' -ForegroundColor Cyan
Write-Host 'Expect 5 failing tests, all in PaginationTests:' -ForegroundColor Cyan
Write-Host '  FirstPage_ReturnsFirstItems_NotSkippedOnes'
Write-Host '  SecondPage_ContinuesExactlyWhereFirstPageEnded'
Write-Host '  LastPage_ReturnsRemainder'
Write-Host '  EveryPage_TogetherCoversTheWholeSetExactlyOnce'
Write-Host '  CountryFilter_PagesTheFilteredSetNotTheWholeSet'
Write-Host ''
Write-Host 'Note that the build stays green and EndpointTests still passes.' -ForegroundColor Cyan
Write-Host 'Only the boundary tests catch it - that is the point.' -ForegroundColor Cyan
