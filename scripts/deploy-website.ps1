# Helpful links
# https://docs.microsoft.com/en-us/rest/api/storageservices/service-sas-examples
# https://blogs.msdn.microsoft.com/maheshk/2017/04/06/azure-storage-rest-api-how-to-call-storage-apis-without-sdk/
# https://stackoverflow.com/questions/49138941/use-azure-table-sas-token-to-read-data-using-invoke-restmethod?rq=1
# https://stackoverflow.com/questions/11698525/powershell-possible-to-determine-a-files-mime-type

# Note, the '$web' container name in the storageUrl should be escaped: '`$web'

# Example
# .\scripts\deploy-website.ps1 -storageUrl "http://127.0.0.1:10000/devstoreaccount1/web/" -sasToken "?st=2018-12-09T08%3A15%3A03Z&se=2018-12-11T08%3A15%3A00Z&sp=rwdl&sv=2018-03-28&sr=c&sig=FJAgQNHlpF9eXOqA%2B%2F0yovA00bZjQPeydbDDQcWG%2F8M%3D"

Param
(
    [ValidateNotNullOrEmpty()]
    [string] $storageUrl,

    [ValidateNotNullOrEmpty()]
    [string] $sasToken,

    [ValidateNotNullOrEmpty()]
    [string] $pathToWebsite = ".\src\Belial.Blazor\bin\Release\netstandard2.0\publish\Belial.Blazor\dist\"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version "Latest"
Add-Type -AssemblyName "System.Web"

try
{
    $filesToUpload = Get-ChildItem -Path $pathToWebsite -Recurse -File -Name

    ForEach ($filename in $filesToUpload)
    {
        $blobUrl = "$storageUrl$filename$sasToken";
        $fileToUpload = "$pathToWebsite$filename";
        $fileLength = (Get-ChildItem -File $fileToUpload).Length
        
        $now = (Get-Date).ToUniversalTime().ToString("R")
        
        $headers = @{
            "Date"=$now;
            "x-ms-version"="2017-11-09";
            "Content-Length"=$fileLength;
            "x-ms-blob-type"="BlockBlob";
        }

        if ($filename -like "*wasm")
        {
            $headers.Add("Content-Type", "application/wasm")
        }
        else
        {
            $contentType = [System.Web.MimeMapping]::GetMimeMapping($filename)
            $headers.Add("Content-Type", $contentType)
        }

        $result = Invoke-RestMethod -Uri $blobUrl -Method Put -Headers $headers -InFile $fileToUpload

        Write-Host "Uploaded $filename"
    }

    exit 0
}
catch
{
    Write-Error $_ -ErrorAction Continue
    exit 1
}