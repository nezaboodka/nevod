# Usage: pwsh ./smoke_test_windows.ps1 -WindowsVersion

param (
    [string] [ValidateSet("windows_x86", "windows_x64")] $WindowsVersion
)

$ErrorActionPreference = "Stop"

$windows_versions = @{
    windows_x86 = "win-x86";
    windows_x64 = "win-x64";
}

function Test-WindowsZipPackage {
    param (
        [PARAMETER(Mandatory = $True, Position = 0)] [string] $WindowsVersion
    )

    Expand-Archive -Path "./Publish/negrep-${WindowsVersion}.zip" -DestinationPath .
    
    cd negrep

    echo '\n./examples/patterns.np:\n'
    cat ./examples/patterns.np
    cat ./NOTICE
    cat ./LICENSE.txt
    cat ./THIRD-PARTY-NOTICES.txt
    Invoke-NativeCommand ./negrep.exe -f ./examples/patterns.np ./examples/example.txt
    echo The official nevod.io site | Invoke-NativeCommand ./negrep.exe -p "@require 'basic/Basic.np'; @search Basic.*;"
    Invoke-NativeCommand ./negrep.exe --version
}

function Invoke-NativeCommand() {
    $command = $args[0]
    
    if ($args.Count -gt 1) {
        $commandArgs = $args[1..($args.Count - 1)]
    } else {
        $commandArgs = @()
    }

    & $command $commandArgs

    if ($LASTEXITCODE -ne 0) {
        throw "$command $commandArgs exited with code $LASTEXITCODE."
    }
}

if ($null -eq $windows_versions[$WindowsVersion])
{
    foreach ($windows_version in $windows_versions.Values)
    {
        Test-WindowsZipPackage $windows_version
    }
}
else
{
    Test-WindowsZipPackage $windows_versions[$WindowsVersion]
}
