# syntax=docker/dockerfile:1

####################################
# Showrunner Stage Manager (operator)
# Runtime image: the operator is published on the GitHub runner first, as the AuthProxy image is.
#   dotnet publish Source/DotNET/StageManager/StageManager.csproj -c Release -p:CratisProxiesOutputPath= -o Source/DotNET/StageManager/out
####################################

FROM mcr.microsoft.com/dotnet/runtime:10.0
ARG VERSION=0.0.0

LABEL org.opencontainers.image.title="Showrunner Stage Manager" \
      org.opencontainers.image.source="https://github.com/Cratis/Showrunner" \
      org.opencontainers.image.version="${VERSION}"

WORKDIR /app
COPY Source/DotNET/StageManager/out/ ./

# The runtime image ships a non-root user; the chart also runs the pod as non-root with a read-only file system.
USER $APP_UID

ENTRYPOINT ["dotnet", "/app/Cratis.Showrunner.StageManager.dll"]
