#! /bin/bash

# Usage: ./publish_osx.sh [--publish] [--install] [--delete]
# When no parameters are provided '--publish' and '--install' are set automatically

# Parameters
COMMANDS=$@

if [[ -z $COMMANDS ]]; then
    COMMANDS=("--publish" "--install")
fi

NEGREP_SOURCE_BIN_PATH="Build/Release/bin/Nezaboodka.Nevod.Negrep/osx-x64/publish/"
NEGREP_PUBLISH_BIN_PATH="Publish/osx-x64/"
NEGREP_INSTALL_BIN_PATH="/usr/local/opt/negrep/"
NEGREP_SYMLINK_BIN_PATH="/usr/local/bin/negrep"

DEFAULT_COLOR="\033[0m"
RED_COLOR="\033[0;31m"
GREEN_COLOR="\033[0;32m"
BLUE_COLOR="\033[1;34m"

# Functions
function publish()
{
    echo -e "${BLUE_COLOR}Stage: publish ${DEFAULT_COLOR}"
    mkdir -p "${NEGREP_PUBLISH_BIN_PATH}"
    dotnet publish Source/Negrep -c Release -f netcoreapp3.1 -r osx-x64
    cp -r "${NEGREP_SOURCE_BIN_PATH}" "${NEGREP_PUBLISH_BIN_PATH}"
    echo -e "negrep has been published successfully [${GREEN_COLOR}${NEGREP_PUBLISH_BIN_PATH}${DEFAULT_COLOR}]"
}

function install()
{
    echo -e "${BLUE_COLOR}Stage: install ${DEFAULT_COLOR}"
    mkdir -p "${NEGREP_INSTALL_BIN_PATH}"
    cp -r "${NEGREP_PUBLISH_BIN_PATH}" "${NEGREP_INSTALL_BIN_PATH}"
    mv "${NEGREP_INSTALL_BIN_PATH}Nezaboodka.Nevod.Negrep" "${NEGREP_INSTALL_BIN_PATH}negrep"
    ln -s "${NEGREP_INSTALL_BIN_PATH}negrep" "${NEGREP_SYMLINK_BIN_PATH}"
    echo -e "negrep has been installed to [${GREEN_COLOR}${NEGREP_INSTALL_BIN_PATH}${DEFAULT_COLOR}]"
    echo -e "The symlink has been created [${GREEN_COLOR}${NEGREP_SYMLINK_BIN_PATH}${DEFAULT_COLOR} -> ${GREEN_COLOR}${NEGREP_INSTALL_BIN_PATH}negrep${DEFAULT_COLOR}]"
}

function delete()
{
    echo -e "${BLUE_COLOR}Stage: delete${DEFAULT_COLOR}"
    rm -r "${NEGREP_INSTALL_BIN_PATH}"
    rm "${NEGREP_SYMLINK_BIN_PATH}"
    echo -e "negrep has been deleted successfully"
}

# Internal Functions
function __has_element () {
    local e match="$1"
    shift
    for e; do [[ "$e" == "$match" ]] && return 0; done
    return 1
}

# Script
set -e

declare -r -a COMMAND_NAMES=(
    "--publish"
    "--install"
    "--delete"
)

for command_name in ${COMMANDS[@]}; do
    if __has_element $command_name ${COMMAND_NAMES[@]}; then
        ${command_name:2}
    else
        echo -e "${RED_COLOR}Unknown command [${GREEN_COLOR}${command_name}${RED_COLOR}]${DEFAULT_COLOR}"
        exit 1
    fi
done
