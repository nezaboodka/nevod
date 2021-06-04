FROM ubuntu

WORKDIR /tmp

COPY Publish/negrep-rhel.6-x64.tar.gz /tmp
RUN tar -xf ./negrep-rhel.6-x64.tar.gz -C /usr/share
RUN test -f /usr/share/negrep/NOTICE
RUN ln -s /usr/share/negrep/negrep /usr/bin/negrep

    # Configure web servers to bind to port 80 when present
ENV ASPNETCORE_URLS=http://+:80 \
    # Enable detection of running in a container
    DOTNET_RUNNING_IN_CONTAINER=true

CMD cd /usr/share/negrep/ \
    && echo '\n./examples/patterns.np:\n' \
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
