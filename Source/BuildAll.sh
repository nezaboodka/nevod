
if [ -z "$1" ]
then
  echo "Usage:"
  echo "  Build-Becnhmark.Regex.sh <workspace-root-directory>"
  exit 1
fi

export ROOT=$(realpath $1)
export NEVOD_SOLUTION=$ROOT/Source/Nezaboodka.Nevod.sln
export BENCHMARK_SOURCE=$ROOT/Source/Benchmark.Regex
export BENCHMARK_BUILD=$ROOT/Build/Release/bin/Nezaboodka.Nevod.Benchmark.Regex

echo
echo Workspace: $ROOT
echo

#pushd "$ROOT/Source" >/dev/null
#dotnet build Nezaboodka.Nevod.sln -c Release
#popd >/dev/null

mkdir -p "$BENCHMARK_BUILD"
pushd "$BENCHMARK_BUILD" >/dev/null
cmake "$BENCHMARK_SOURCE"
cmake --build .
popd >/dev/null
