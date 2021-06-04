# This script does the following steps:
#   * builds a docker image
#   * runs a docker container
#   * copies the result to the host OS
#   * deletes the docker container
#
# Usage: pwsh ./publish.ps1 -RuntimeIdentifier

param (
    [string] [ValidateSet("linux_x64", "windows_x64", "windows_x86")] $RuntimeIdentifier
)

# Functions

function Publish-NetCoreApp {
    param (
        [PARAMETER(Mandatory = $True, Position = 0)] [string] $RuntimeId
    )

    $docker_image_name = "negrep-publish-${RuntimeId}:local"

    Write-Host "`n"
    Write-Host "Building application for ${RUNTIME_ID}..."
    Write-Host "`n"

    New-Item -ItemType Directory -Force -Path Publish

    (Get-Content Deployment/Publish/negrep-publish-${RuntimeId}.Dockerfile) `
        -replace '\$NG_VERSION', $NG_VERSION | Set-Content Deployment/negrep-publish-${RuntimeId}.Dockerfile.tmp

    docker build -t $docker_image_name -f Deployment/negrep-publish-${RuntimeId}.Dockerfile.tmp .
    $container_id = docker run -d -ti $docker_image_name
    docker cp ${container_id}:/tmp/publish/out/. Publish/
    docker kill $container_id
    docker rm -vf $container_id

    Remove-Item Deployment/negrep-publish-${RuntimeId}.Dockerfile.tmp

    Write-Host "`n"
    Write-Host "The application for ${RuntimeId} has been deployed successfully and saved to Publish."
}

# Script

$RIDS = @{
    linux_x64 = "rhel.6-x64";
    windows_x64 = "win-x64";
    windows_x86 = "win-x86";
}

if ($null -eq $RIDS[$RuntimeIdentifier])
{
    foreach ($runtime_id in $RIDS.Values)
    {
        Publish-NetCoreApp $runtime_id
    }
}
else
{
    Publish-NetCoreApp $RIDS[$RuntimeIdentifier]
}
