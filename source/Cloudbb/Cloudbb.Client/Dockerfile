# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS base
USER $APP_UID
WORKDIR /app

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
SHELL ["/bin/bash", "-euo", "pipefail", "-c"]
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["Cloudbb.Client/Cloudbb.Client.csproj", "Cloudbb.Client/"]
RUN apt-get update && apt-get install python3 -y
RUN dotnet workload install wasm-tools
RUN dotnet restore "./Cloudbb.Client/Cloudbb.Client.csproj"
COPY . .

# version substitution (inject current CI version info into assembly version (csproj) and runtime version info (C# file) definitions)
RUN PROJECT_FILE_PATH='./Cloudbb.Client/Cloudbb.Client.csproj' && \
    DOTNET_CI_VERSION_FILE='./Cloudbb.Client/CloudbbClient.cs' && \
    if [ -f './CI_VERSION_INFO' ]; then export VERSION=$(cat './CI_VERSION_INFO'); else export VERSION="0.0.0"; fi && \
    export VERSION_PREFIX=$(echo "${VERSION}" | grep -Po '^[0-9]+\.[0-9]+\.[0-9]+') && \
    export VERSION_SUFFIX=$(echo "${VERSION}" | grep -Po '(?<=-)([A-Za-z0-9\-]+)') && \
    echo "Version Prefix is ${VERSION_PREFIX}" && \
    echo "Version Suffix is ${VERSION_SUFFIX}" && \
    sed -i "s/<VersionPrefix>0\.0\.0<\/VersionPrefix>/<VersionPrefix>${VERSION_PREFIX}<\/VersionPrefix>/" "${PROJECT_FILE_PATH}" && \
    sed -i "s/<VersionSuffix>CI-INJECTED<\/VersionSuffix>/<VersionSuffix>${VERSION_SUFFIX}<\/VersionSuffix>/" "${PROJECT_FILE_PATH}" && \
    echo 'csproj file after version substitution:' && cat "${PROJECT_FILE_PATH}" && \
    if [ ! -z "${DOTNET_CI_VERSION_FILE+x}" ] && [ -f "${DOTNET_CI_VERSION_FILE}" ]; then sed -i "s/private\s\+const\s\+string\s\+CI_DEPLOYMENT__VERSION_PREFIX\s\+=\s\+\"0\.0\.0\"/private const string CI_DEPLOYMENT__VERSION_PREFIX = \"${VERSION_PREFIX}\"/" "${DOTNET_CI_VERSION_FILE}"; fi && \
    if [ ! -z "${DOTNET_CI_VERSION_FILE+x}" ] && [ -f "${DOTNET_CI_VERSION_FILE}" ]; then sed -i "s/private\s\+const\s\+string\s\+CI_DEPLOYMENT__VERSION_SUFFIX\s\+=\s\+\"CI-INJECTED\"/private const string CI_DEPLOYMENT__VERSION_SUFFIX = \"${VERSION_SUFFIX}\"/" "${DOTNET_CI_VERSION_FILE}"; fi && \
    if [ ! -z "${DOTNET_CI_VERSION_FILE+x}" ] && [ -f "${DOTNET_CI_VERSION_FILE}" ]; then sed -i "s/private\s\+const\s\+string\s\+CI_DEPLOYMENT__DATETIME_UTC\s\+=\s\+\"1970-01-01\s\+00:00:00\"/private const string CI_DEPLOYMENT__DATETIME_UTC = \"$(date -u +'%Y-%m-%d %H:%M:%S')\"/" "${DOTNET_CI_VERSION_FILE}"; fi && \
    if [ ! -z "${DOTNET_CI_VERSION_FILE+x}" ] && [ -f "${DOTNET_CI_VERSION_FILE}" ]; then echo 'csharp version file after version substitution:' && cat "${DOTNET_CI_VERSION_FILE}"; fi && \
    echo 'Successfully injected version into project files.'

WORKDIR "/src/Cloudbb.Client"
RUN dotnet restore "./Cloudbb.Client.csproj" --no-cache
RUN dotnet build "./Cloudbb.Client.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Cloudbb.Client.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM nginx:1.29.4-trixie AS final
WORKDIR /var/www/web
COPY --from=publish /app/publish/wwwroot .
COPY "Cloudbb.Client/nginx.conf" /etc/nginx/nginx.conf
EXPOSE 80