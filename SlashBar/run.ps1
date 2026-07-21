# SDK local (dossier ../dotnet), pas celui du système
$sdk = "C:\Users\STAGE 2025\Documents\temp\slash-bar\dotnet"
$env:DOTNET_ROOT = $sdk
$env:PATH = "$sdk;$env:PATH"

Set-Location $PSScriptRoot
Write-Host "SDK:" -NoNewline
& "$sdk\dotnet.exe" --version
& "$sdk\dotnet.exe" run
