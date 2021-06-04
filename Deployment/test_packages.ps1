# Usage: pwsh ./test_packages.ps1 -LinuxDistribution

param (
    [string] [ValidateSet("ubuntu_x64")] $LinuxDistribution
)

$linux_distributions = @{
    ubuntu_x64 = "ubuntu-x64";
}

function Test-Package {
    param (
        [PARAMETER(Mandatory = $True, Position = 0)] [string] $LinuxDistribution
    )

    $docker_image_name = "negrep-test-package-${LinuxDistribution}:local"

    Write-Host "${LinuxDistribution}:".ToUpper() -ForegroundColor Blue

    Write-Host "Building..." -ForegroundColor White
    docker build -t $docker_image_name -f Deployment/Test/negrep-test-package-${LinuxDistribution}.Dockerfile .
    Write-Host "Docker image has built successfully." -ForegroundColor White

    docker run -t --rm $docker_image_name
}

if ($null -eq $linux_distributions[$LinuxDistribution])
{
    foreach ($linux_distribution in $linux_distributions.Values)
    {
        Test-Package $linux_distribution
    }
}
else
{
    Test-Package $linux_distributions[$LinuxDistribution]
}
