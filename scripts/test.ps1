$testProjectPath = Join-Path -Path $PSScriptRoot -ChildPath "..\src\Belial.Tests\Belial.Tests.csproj"
dotnet test $testProjectPath