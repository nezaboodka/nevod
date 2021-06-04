#! /bin/sh

# Usage: ./test_packages.sh [LINUX_DISTRIBUTION]
# Tip: to run this script locally set NV_DOCK_TAG to whatever you want before ./test_packages.sh (e.g. export NV_DOCK_TAG="local" && ./test_packages.sh).
#      NV_DOCK_TAG is used as a tag for docker image.

# Functions

test_package() {
    # Argument: LINUX_DISTRIBUTION
    LINUX_DISTRIBUTION=$1
    DOCKER_IMAGE_NAME="negrep-test-package-${LINUX_DISTRIBUTION}:${NV_DOCK_TAG}"

    echo "${LINUX_DISTRIBUTION}:" | awk '{print toupper($0)}'

    echo "Building..."
    docker build -t $DOCKER_IMAGE_NAME -q -f Deployment/Test/negrep-test-package-${LINUX_DISTRIBUTION}.Dockerfile . > /dev/null
    echo "Docker image has built successfully."

    docker run -t --rm $DOCKER_IMAGE_NAME
}

# Script

set -e

ubuntu_x64=1
LINUX_DISTS_1="ubuntu-x64"

LINUX_DISTS="$ubuntu_x64"

if [ -z $1 ]; then
    for LINUX_DISTRIBUTION in $LINUX_DISTS; do
        L_DIST="$(eval echo \$LINUX_DISTS_$LINUX_DISTRIBUTION)"
        test_package $L_DIST
    done
else
    LINUX_DISTRIBUTION="$(eval echo \$$1)"
    if [ -z $LINUX_DISTRIBUTION ]; then
        echo "Unknown Linux distribution"
        exit 1
    else
        L_DIST="$(eval echo \$LINUX_DISTS_$LINUX_DISTRIBUTION)"
        test_package $L_DIST
    fi
fi
