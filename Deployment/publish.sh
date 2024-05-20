#! /bin/sh

# This script does the following steps:
#   * builds a docker image
#   * runs a docker container
#   * copies the result to the host OS
#   * deletes the docker container

# Usage: ./publish.sh [RUNTIME_ID]
# Tip: to run this script locally set NV_DOCK_TAG and NG_VERSION to whatever you want before ./publish.sh (e.g. export NV_DOCK_TAG="local" && export NG_VERSION="1.0.0" && ./publish.sh).
#      NV_DOCK_TAG is used as a tag for docker image.
#      NG_VERSION is used as a version for negrep binaries.

# Functions

publish_netcoreapp() {
    # Argument: RUNTIME_ID
    RUNTIME_ID=$1
    DOCKER_IMAGE_NAME="negrep-publish-${RUNTIME_ID}:${NV_DOCK_TAG}"

    echo
    echo "Building application for ${RUNTIME_ID}..."
    echo

    mkdir -p Publish

    sed 's/$NG_VERSION/'"$NG_VERSION"'/g' Deployment/Publish/negrep-publish-${RUNTIME_ID}.Dockerfile > Deployment/negrep-publish-${RUNTIME_ID}.Dockerfile.tmp

    docker build -t $DOCKER_IMAGE_NAME -f Deployment/negrep-publish-${RUNTIME_ID}.Dockerfile.tmp .
    CONTAINER_ID="$(docker run -d -ti $DOCKER_IMAGE_NAME)"
    docker cp ${CONTAINER_ID}:/tmp/publish/out/. Publish/
    docker kill $CONTAINER_ID
    docker rm -vf $CONTAINER_ID

    rm Deployment/negrep-publish-${RUNTIME_ID}.Dockerfile.tmp

    echo
    echo "The application for ${RUNTIME_ID} has been deployed successfully and saved to Publish."
}

# Script

set -e

linux_x64=1
RIDS_1="linux-x64"

windows_x64=2
RIDS_2="win-x64"

windows_x86=3
RIDS_3="win-x86"

osx_x64=4
RIDS_4="osx-x64"

RIDS="$linux_x64 $windows_x64 $windows_x86 $osx_x64"

if [ -z $1 ]; then
    for RUNTIME_ID in $RIDS; do
        RID="$(eval echo \$RIDS_$RUNTIME_ID)"
        publish_netcoreapp $RID
    done
else
    RUNTIME_ID="$(eval echo \$$1)"
    if [ -z $RUNTIME_ID ]; then
        echo "Unknown runtime identifier"
        exit 1
    else
        RID="$(eval echo \$RIDS_$RUNTIME_ID)"
        publish_netcoreapp $RID
    fi
fi
