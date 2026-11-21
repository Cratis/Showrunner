# syntax=docker/dockerfile:1

####################################
# Showrunner Watchdog (alerting job)
# The job image runs the watchdog assembly; the job's args are `cluster`, `test-alert`, `runners`, `agent-harness`,
# `volumes`, `probe` or `probe-external` (the last one needs no cluster, so CI can run it from this same image).
# The watchdog is published on the GitHub runner first, as the Stage Manager and Understudy images are.
#   dotnet publish Source/DotNET/Operations.Watchdog/Operations.Watchdog.csproj -c Release -p:CratisProxiesSkip=true -o Source/DotNET/Operations.Watchdog/out
####################################

FROM mcr.microsoft.com/dotnet/runtime:11.0
ARG VERSION=0.0.0

LABEL org.opencontainers.image.title="Showrunner Watchdog" \
      org.opencontainers.image.source="https://github.com/Cratis/Showrunner" \
      org.opencontainers.image.version="${VERSION}"

WORKDIR /app
COPY Source/DotNET/Operations.Watchdog/out/ ./

USER $APP_UID

ENTRYPOINT ["dotnet", "/app/Cratis.Showrunner.Operations.Watchdog.dll"]
