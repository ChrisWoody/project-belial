Param (
    [ValidateNotNullOrEmpty()]
    [string] $Name,

    [ValidateNotNullOrEmpty()]
    [ValidateSet("CentralUS", "EastUS", "EastUS2", "NorthCentralUS", "SouthCentralUS", "WestUS", "WestUS2",
                 "NorthEurope", "WestEurope", "EastAsia", "SoutheastAsia", "JapanEast", "JapanWest", 
                 "BrazilSouth", "AustraliaEast", "AustraliaSoutheast", "CentralIndia", "SouthIndia", "WestIndia")]
    [string] $Location = "AustraliaSoutheast",

    [ValidateNotNullOrEmpty()]
    [string] $SubscriptionId,

    [switch] $SkipExistenceCheck
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version "Latest"

try {
    
    $azureContext = $null
    try { $azureContext = $(Get-AzureRmContext).Account }
    catch { }
    if ([string]::IsNullOrEmpty($azureContext)) {
        Login-AzureRmAccount
    }
    
    "Selecting subscription $SubscriptionId"
    Select-AzureRmSubscription -SubscriptionId $SubscriptionId | Out-Null

    if(-Not $SkipExistenceCheck) {
        Write-Output "Checking if resource group $Name exists"
        Get-AzureRmResourceGroup -Name $Name -ev notPresent -ea 0
        if(-Not $notPresent) {
            Write-Host "Resource group already exists, taking no action"
            exit 0
        }

        Write-Output "Creating resource group $Name"
        New-AzureRmResourceGroup -Location $Location -Name $Name -Force | Out-Null
    }

    Write-Output "Deploying to ARM"
    $Parameters = @{
        "appName" = $Name;
    }
    $result = New-AzureRmResourceGroupDeployment -ResourceGroupName $Name -TemplateFile "$PSScriptRoot\provisionarm.json" -TemplateParameterObject $Parameters -Name ("$Name-" + (Get-Date -Format "yyyyMMdd-HHmm")) -Mode Incremental -Verbose
    if (-not $result -or $result.ProvisioningState -ne "Succeeded") {
        Write-Error "Unable to provision environment"
        exit 1
    }
    
    Write-Host "Environment provisioned"
    Write-Host Publish username: $result.Outputs["publishingUsername"].Value
    Write-Host Publish password: $result.Outputs["publishingPassword"].Value
    exit 0  
} catch {
    Write-Error $_ -ErrorAction Continue
    exit 1
}