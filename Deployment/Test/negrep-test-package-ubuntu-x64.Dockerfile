FROM ubuntu

WORKDIR /tmp

COPY Publish/negrep-x86_64.deb .

    # Configure web servers to bind to port 80 when present
ENV ASPNETCORE_URLS=http://+:80 \
    # Enable detection of running in a container
    DOTNET_RUNNING_IN_CONTAINER=true

RUN dpkg --install negrep-x86_64.deb
RUN dpkg --remove negrep
RUN test ! -L /usr/bin/negrep
RUN dpkg --install negrep-x86_64.deb

WORKDIR /usr/share/negrep/
RUN test -f NOTICE
CMD echo './examples/patterns.np:\n' \
    && cat ./examples/patterns.np \
    && echo \
    && cat ./NOTICE \
    && echo \
    && cat ./LICENSE.txt \
    && echo \
    && cat ./THIRD-PARTY-NOTICES.txt \
    && echo \
    && negrep -f ./examples/patterns.np ./examples/example.txt \
    && negrep --version
