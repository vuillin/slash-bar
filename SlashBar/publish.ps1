# Local Release publish → zip under ../publish/
$ErrorActionPreference = "Stop"

if ($env:DOTNET_ROOT) {
    $env:PATH = "$($env:DOTNET_ROOT);$env:PATH"
}

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) {
    Write-Error "dotnet not found. Install the .NET SDK or set DOTNET_ROOT."
}

$repoRoot = Split-Path $PSScriptRoot -Parent
$project = Join-Path $PSScriptRoot "SlashBar.csproj"
$outDir = Join-Path $repoRoot "publish\win-x64"
$publishRoot = Join-Path $repoRoot "publish"

$csprojXml = [xml](Get-Content $project -Raw)
$version = $csprojXml.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
if (-not $version) { $version = "0.0.0" }

$zipName = "SlashBar-v$version-win-x64.zip"
$zipPath = Join-Path $publishRoot $zipName

if (Test-Path $outDir) {
    Remove-Item $outDir -Recurse -Force
}
New-Item -ItemType Directory -Path $outDir -Force | Out-Null
New-Item -ItemType Directory -Path $publishRoot -Force | Out-Null

Write-Host "Publishing SlashBar $version (win-x64, self-contained)..."
& dotnet publish $project `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishReadyToRun=true `
    -o $outDir
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Write-Host "Zipping → $zipPath"
Compress-Archive -Path (Join-Path $outDir "*") -DestinationPath $zipPath -Force

Write-Host "Done."
Write-Host "  Output: $outDir"
Write-Host "  Zip:    $zipPath"
