FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine3.18

RUN apk add --no-cache \
        ca-certificates \
        bash \
        zip

WORKDIR /tmp

RUN mkdir -p publish/nevod.negrep \
    && mkdir publish/out

COPY Deployment/Examples publish/nevod.negrep/Deployment/Examples
COPY LICENSE.txt publish/nevod.negrep/LICENSE.txt
COPY NOTICE publish/nevod.negrep/NOTICE
COPY Deployment/Publish/THIRD-PARTY-NOTICES.txt publish/nevod.negrep/Deployment/Publish/THIRD-PARTY-NOTICES.txt
COPY Source/ publish/nevod.negrep/Source

RUN dotnet publish /tmp/publish/nevod.negrep/Source/Negrep -c Release -f net8.0 -r win-x86 /p:Version=$NG_VERSION

WORKDIR /tmp/publish/out

RUN mkdir ./negrep \
    && cp -r ../nevod.negrep/Build/Release/bin/Nezaboodka.Nevod.Negrep/win-x86/publish/* ./negrep \
    && mv negrep/Nezaboodka.Nevod.Negrep.exe negrep/negrep.exe

RUN zip -r negrep-win-x86.zip negrep
RUN rm -r negrep
