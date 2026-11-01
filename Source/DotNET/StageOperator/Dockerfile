# The Stage operator image. Build from the repository root:
#   docker build -f Source/DotNET/StageOperator/Dockerfile -t ghcr.io/cratis/showrunner-stage-operator:dev .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first so the layer is cached until a project or package file changes.
COPY global.json Directory.Build.props Directory.Build.targets Directory.Packages.props stylecop.json .editorconfig ./
COPY Source/DotNET/Directory.Build.props Source/DotNET/SharedSpecificationInstance.cs Source/DotNET/
COPY Source/DotNET/StageOperator/StageOperator.csproj Source/DotNET/StageOperator/
RUN dotnet restore Source/DotNET/StageOperator/StageOperator.csproj

COPY Source/DotNET/StageOperator Source/DotNET/StageOperator
ARG VERSION=0.0.0
RUN dotnet publish Source/DotNET/StageOperator/StageOperator.csproj \
    --configuration Release --no-restore \
    -p:Version=${VERSION} -p:CratisProxiesOutputPath= \
    -o /out

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /out .
# The runtime image defines a non-root user; the operator needs no privileges.
USER $APP_UID
ENTRYPOINT ["dotnet", "Cratis.Showrunner.StageOperator.dll"]
