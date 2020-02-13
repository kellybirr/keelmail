# Build environment image
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as build-env
WORKDIR /build
COPY . .

# Setup Builder Environments
ARG project=Keelmail

# Build & Publish
WORKDIR /build
RUN dotnet publish ${project}.csproj --no-cache -c Release -r linux-x64 --self-contained false -o /app

# Create container image from publish output
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 as runtime-env

WORKDIR /app
COPY --from=build-env /app .

ENV ASPNETCORE_URLS http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "Keelmail.dll"]