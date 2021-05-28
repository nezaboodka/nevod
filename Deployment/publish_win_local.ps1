# Usage: pwsh ./publish_win.ps1 -WindowsRuntimeIdentifier

param (
    [string] [ValidateSet("x64", "x86")] $WindowsRuntimeIdentifier
)

# Functions

function Publish-NetCoreApp {
    param (
        [PARAMETER(Mandatory = $True, Position = 0)] [string] $RuntimeId
    )

    Remove-Item ./Publish/local -Recurse -Force -ErrorAction Ignore

    $NG_VERSION = ([Xml] (Get-Content Source/Negrep/Nezaboodka.Nevod.Negrep.csproj)).Project.PropertyGroup.Version
    $NG_VERSION = "$NG_VERSION".Trim()
    dotnet publish ./Source/Negrep -c Release -f netcoreapp3.1 --self-contained --runtime ${RuntimeId} --output ./Publish/local/${RuntimeId}/negrep
    
    Set-Location ./Publish/local/${RuntimeId}
    Rename-Item "negrep/Nezaboodka.Nevod.Negrep.exe" "negrep.exe"
    ./../../../Tools/win/zip ../../negrep-${NG_VERSION}-${RuntimeId}.zip negrep/*

    Set-Location ../../..
    Remove-Item ./Publish/local -Recurse -Force -ErrorAction Ignore

    Write-Host "The application has been deployed successfully and saved to Publish/negrep-${RuntimeId}.zip"
}

# Script

$RIDS = @{
    x64 = "win-x64";
    x86 = "win-x86";
}

if ($null -eq $RIDS[$WindowsRuntimeIdentifier])
{
    foreach ($runtime_id in $RIDS.Values)
    {
        Publish-NetCoreApp $runtime_id
    }
}
else
{
    Publish-NetCoreApp $RIDS[$WindowsRuntimeIdentifier]
}
