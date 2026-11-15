# syntax=docker/dockerfile:1

####################################
# Showrunner Understudy (backup job)
# The job image runs the Stage Manager assembly; the job's args are `understudy backup|keys|vault-snapshot`. mongodump sits next to it.
# The operator is published on the GitHub runner first, as the Stage Manager image is.
#   dotnet publish Source/DotNET/StageManager/StageManager.csproj -c Release -p:CratisProxiesSkip=true -o Source/DotNET/StageManager/out
####################################

FROM mcr.microsoft.com/dotnet/runtime:11.0
ARG VERSION=0.0.0

LABEL org.opencontainers.image.title="Showrunner Understudy" \
      org.opencontainers.image.source="https://github.com/Cratis/Showrunner" \
      org.opencontainers.image.version="${VERSION}"

# mongodump comes from the MongoDB database tools, which Debian does not package.
USER root
ARG TARGETARCH
ARG MONGO_TOOLS_VERSION=100.10.0
RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates curl \
    && case "${TARGETARCH:-amd64}" in arm64) arch=arm64 ;; *) arch=x86_64 ;; esac \
    && curl -fsSL "https://fastdl.mongodb.org/tools/db/mongodb-database-tools-debian12-${arch}-${MONGO_TOOLS_VERSION}.tgz" -o /tmp/tools.tgz \
    && tar -xzf /tmp/tools.tgz -C /tmp \
    && cp /tmp/mongodb-database-tools-*/bin/mongodump /usr/local/bin/mongodump \
    && rm -rf /tmp/tools.tgz /tmp/mongodb-database-tools-* /var/lib/apt/lists/* \
    && apt-get purge -y curl && apt-get autoremove -y

WORKDIR /app
COPY Source/DotNET/StageManager/out/ ./

USER $APP_UID

ENTRYPOINT ["dotnet", "/app/Cratis.Showrunner.StageManager.dll"]
