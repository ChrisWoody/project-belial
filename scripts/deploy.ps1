Param (
    [ValidateNotNullOrEmpty()]
    [string] $Username,

    [ValidateNotNullOrEmpty()]
    [string] $Password,

    [ValidateNotNullOrEmpty()]
    [string] $appServiceName,

    [ValidateNotNullOrEmpty()]
    [string] $PackagePath = "packageTemp\webapp.zip"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version "Latest"

try {
    $PackagePath = Resolve-Path (Join-Path -Path $PSScriptRoot -ChildPath $PackagePath)
    $appServiceUrl = "$($appServiceName).scm.azurewebsites.net"
    
    $msDeployArgs =
    '-verb:sync ' +
    "-source:package='$PackagePath' " + 
    "-dest:ContentPath=.,ComputerName=https://$appServiceUrl/msdeploy.axd?site=$appServiceName,UserName=$Username,Password=$Password,AuthType='Basic',includeAcls='False' " +
    "-retryAttempts:5 -retryInterval:5000 -verbose"
    
    $commandLine = '&"C:\Program Files\IIS\Microsoft Web Deploy V3\msdeploy.exe" --% ' + $msDeployArgs
    $result = Invoke-Expression $commandLine
    Write-Host $result
    #if ($result -notlike '*The synchronization completed*') {
    #    exit 1
    #}

    exit 0
}
catch {
    Write-Error $_ -ErrorAction Continue
    exit 1
}
