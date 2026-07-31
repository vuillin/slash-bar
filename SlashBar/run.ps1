# Dev launcher — uses `dotnet` from PATH (or DOTNET_ROOT if set).
$ErrorActionPreference = "Stop"

if ($env:DOTNET_ROOT) {
    $env:PATH = "$($env:DOTNET_ROOT);$env:PATH"
}

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) {
    Write-Error "dotnet not found. Install the .NET SDK or set DOTNET_ROOT."
}

Set-Location $PSScriptRoot
Write-Host "SDK: " -NoNewline
& dotnet --version
& dotnet run
