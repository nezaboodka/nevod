#! /bin/sh

# Usage: ./smoke_test.sh [LINUX_DISTRIBUTION]
# Tip: to run this script locally set NV_DOCK_TAG to whatever you want before ./smoke_test.sh (e.g. export NV_DOCK_TAG="local" && ./smoke_test.sh).
#      NV_DOCK_TAG is used as a tag for docker image.

# Functions

test_netcoreapp() {
    # Argument: LINUX_DISTRIBUTION
    LINUX_DISTRIBUTION=$1
    DOCKER_IMAGE_NAME="negrep-test-${LINUX_DISTRIBUTION}:${NV_DOCK_TAG}"

    echo "${LINUX_DISTRIBUTION}:" | awk '{print toupper($0)}'

    NG_VERSION=$(grep '<Version>' < Source/Negrep/Nezaboodka.Nevod.Negrep.csproj | sed 's/.*<Version>\(.*\)<\/Version>/\1/' | tr -d '\r')
    sed 's/$NG_VERSION/'"$NG_VERSION"'/g' Deployment/Test/negrep-test-${LINUX_DISTRIBUTION}.Dockerfile > Deployment/Test/negrep-test-${LINUX_DISTRIBUTION}.Dockerfile.tmp

    echo "Building..."
    docker build -t $DOCKER_IMAGE_NAME -q -f Deployment/Test/negrep-test-${LINUX_DISTRIBUTION}.Dockerfile.tmp . > /dev/null
    echo "Docker image has built successfully."

    docker run -t --rm $DOCKER_IMAGE_NAME

    rm Deployment/Test/negrep-test-${LINUX_DISTRIBUTION}.Dockerfile.tmp
}

# Script

set -e

centos_x64=1
LINUX_DISTS_1="centos-x64"

ubuntu_x64=2
LINUX_DISTS_2="ubuntu-x64"

LINUX_DISTS="$centos_x64 $ubuntu_x64"

if [[ -z $1 ]]; then
    for LINUX_DISTRIBUTION in $LINUX_DISTS; do
        L_DIST="$(eval echo \$LINUX_DISTS_$LINUX_DISTRIBUTION)"
        test_netcoreapp $L_DIST
    done
else
    LINUX_DISTRIBUTION="$(eval echo \$$1)"
    if [[ -z $LINUX_DISTRIBUTION ]]; then
        echo "Unknown Linux distribution"
        exit 1
    else
        L_DIST="$(eval echo \$LINUX_DISTS_$LINUX_DISTRIBUTION)"
        test_netcoreapp $L_DIST
    fi
fi
