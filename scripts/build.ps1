$slnPath = Join-Path -Path $PSScriptRoot -ChildPath "..\src\Belial.sln"
dotnet build $slnPath

$websitePath = Join-Path -Path $PSScriptRoot -ChildPath "..\src\Belial.Blazor\Belial.Blazor.csproj"
dotnet publish $websitePath -c Release