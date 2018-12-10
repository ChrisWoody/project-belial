$slnPath = Join-Path -Path $PSScriptRoot -ChildPath "..\src\Belial.sln"
dotnet build $slnPath

dotnet publish "..\src\Belial.Blazor\Belial.Blazor.csproj" -c Release -o out