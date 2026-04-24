#Requires -Version 5.0
$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

dotnet run --project (Join-Path $PSScriptRoot "build\_build.csproj") -c Release -- @args
exit $LASTEXITCODE
