FROM mcr.microsoft.com/dotnet/core/sdk:3.1

WORKDIR /tmp

RUN mkdir -p publish/nevod.negrep \
    && mkdir publish/out

COPY Deployment/Examples publish/nevod.negrep/Deployment/Examples
COPY LICENSE.txt publish/nevod.negrep/LICENSE.txt
COPY NOTICE publish/nevod.negrep/NOTICE
COPY Deployment/Publish/THIRD-PARTY-NOTICES.txt publish/nevod.negrep/Deployment/Publish/THIRD-PARTY-NOTICES.txt
COPY Source/ publish/nevod.negrep/Source
RUN dotnet restore publish/nevod.negrep/Source/Negrep -r rhel.6-x64

RUN dotnet publish /tmp/publish/nevod.negrep/Source/Negrep -c Release -f netcoreapp3.1 -r rhel.6-x64 --no-restore /p:Version=$NG_VERSION

WORKDIR /tmp/publish/out
COPY Deployment/Publish/negrep-deb-package negrep-x86_64

RUN cp -r ../nevod.negrep/Build/Release/bin/Nezaboodka.Nevod.Negrep/rhel.6-x64/publish/* negrep-x86_64/usr/share/negrep \
    && mv negrep-x86_64/usr/share/negrep/Nezaboodka.Nevod.Negrep negrep-x86_64/usr/share/negrep/negrep \
    && rm -f negrep-x86_64/usr/share/negrep/.gitkeep

RUN tar -czf negrep-rhel.6-x64.tar.gz -C negrep-x86_64/usr/share negrep

RUN find negrep-x86_64/DEBIAN -type d | xargs chmod 755 \
    && chmod 755 negrep-x86_64/DEBIAN/* \
    && chmod 755 negrep-x86_64/usr/share/negrep/* \
    && chmod 644 negrep-x86_64/usr/share/negrep/*.so
RUN INSTALLED_SIZE=$(du negrep-x86_64 | tail -n 1 | grep -E -o '^[0-9]+') \
    && sed -i "s/{installed-size-value}/$INSTALLED_SIZE/" negrep-x86_64/DEBIAN/control
RUN sed -i "s/{current-version-value}/$NG_VERSION/" negrep-x86_64/DEBIAN/control
RUN cat negrep-x86_64/DEBIAN/control
RUN sed -i "s/{current-version-value}/$NG_VERSION/" negrep-x86_64/usr/share/doc/negrep/changelog
RUN CURRENT_DATETIME=$(date -R) \
    && sed -i "s/{current-datetime-value}/$CURRENT_DATETIME/" negrep-x86_64/usr/share/doc/negrep/changelog
RUN cat negrep-x86_64/usr/share/doc/negrep/changelog

WORKDIR /tmp/publish/out/negrep-x86_64
RUN md5sum `find . -type f | grep -v '^[.]/DEBIAN/'` > DEBIAN/md5sums

WORKDIR /tmp/publish/out
RUN dpkg-deb -b negrep-x86_64

RUN rm -r negrep-x86_64
