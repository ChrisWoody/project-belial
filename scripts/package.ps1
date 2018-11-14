#Structure: https://docs.microsoft.com/en-us/azure/azure-functions/deployment-zip-push

$packagePath = Join-Path $PSScriptRoot -ChildPath "PackageTemp"

if (Test-Path $packagePath) {
    Remove-Item $packagePath -Force -Recurse
}

New-Item $packagePath -ItemType Directory | Out-Null;

$binPath = Join-Path $PSScriptRoot -ChildPath "..\src\Belial\bin\Debug\netcoreapp2.1" | Resolve-Path

Get-ChildItem "$binPath" | Copy-Item -Destination "$packagePath" -Recurse -Container

if (Test-Path "$packagePath\local.settings.json") {
    Remove-Item "$packagePath\local.settings.json"
}

if (Test-Path "$packagePath\bin\runtimes\") {
    Remove-Item "$packagePath\bin\runtimes\" -Recurse
}

Compress-Archive -Path $packagePath\* -DestinationPath "$packagePath\webapp.zip" -Force