# Usage: pwsh ./smoke_test.ps1 -LinuxDistribution

param (
    [string] [ValidateSet("centos_x64", "ubuntu_x64")] $LinuxDistribution
)

$linux_distributions = @{
    centos_x64 = "centos-x64";
    ubuntu_x64 = "ubuntu-x64";
}

function Test-NetCoreApp {
    param (
        [PARAMETER(Mandatory = $True, Position = 0)] [string] $LinuxDistribution
    )

    $docker_image_name = "negrep-test-${LinuxDistribution}:local"

    Write-Host "${LinuxDistribution}:".ToUpper() -ForegroundColor Blue

    $NG_VERSION = ([Xml] (Get-Content Source/Negrep/Nezaboodka.Nevod.Negrep.csproj)).Project.PropertyGroup.Version
    (Get-Content Deployment/Test/negrep-test-${LinuxDistribution}.Dockerfile) `
        -replace '\$NG_VERSION', "$NG_VERSION".Trim() | Set-Content Deployment/Test/negrep-test-${LinuxDistribution}.Dockerfile.tmp

    Write-Host "Building..." -ForegroundColor White
    docker build -t $docker_image_name -q -f Deployment/Test/negrep-test-${LinuxDistribution}.Dockerfile.tmp . | Out-Null
    Write-Host "Docker image has built successfully." -ForegroundColor White

    docker run -t --rm $docker_image_name

    Remove-Item Deployment/Test/negrep-test-${LinuxDistribution}.Dockerfile.tmp
}

if ($null -eq $linux_distributions[$LinuxDistribution])
{
    foreach ($linux_distribution in $linux_distributions.Values)
    {
        Test-NetCoreApp $linux_distribution
    }
}
else
{
    Test-NetCoreApp $linux_distributions[$LinuxDistribution]
}
