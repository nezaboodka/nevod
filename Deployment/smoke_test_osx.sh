#! /bin/sh

set -e

tar -xf Publish/negrep-osx-x64.tar.gz
cd negrep
echo '\n./examples/patterns.np:\n'
cat ./examples/patterns.np
echo
cat ./NOTICE
echo
cat ./LICENSE.txt
echo
cat ./THIRD-PARTY-NOTICES.txt
echo
./negrep -f ./examples/patterns.np ./examples/example.txt
echo The official nevod.io site | ./negrep -p "@require 'basic/Basic.np'; @search Basic.*;"
./negrep --version
