FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine

WORKDIR /tmp

RUN mkdir -p publish/nevod.negrep \
    && mkdir publish/out

COPY Deployment/Examples publish/nevod.negrep/Deployment/Examples
COPY LICENSE.txt publish/nevod.negrep/LICENSE.txt
COPY NOTICE publish/nevod.negrep/NOTICE
COPY Deployment/Publish/THIRD-PARTY-NOTICES.txt publish/nevod.negrep/Deployment/Publish/THIRD-PARTY-NOTICES.txt
COPY Source/ publish/nevod.negrep/Source

RUN dotnet publish /tmp/publish/nevod.negrep/Source/Negrep -c Release -f netcoreapp3.1 -r osx-x64 /p:Version=$NG_VERSION

WORKDIR /tmp/publish/out

RUN mkdir ./negrep \
    && cp -r ../nevod.negrep/Build/Release/bin/Nezaboodka.Nevod.Negrep/osx-x64/publish/* ./negrep \
    && mv negrep/Nezaboodka.Nevod.Negrep negrep/negrep

RUN tar -czf negrep-osx-x64.tar.gz negrep
RUN rm -r negrep
