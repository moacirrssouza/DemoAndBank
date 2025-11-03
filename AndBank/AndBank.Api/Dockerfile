FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AndBank.Api/AndBank.Api.csproj", "AndBank.Api/"]
COPY ["AndBank.Infrastructure/AndBank.Infrastructure.csproj", "AndBank.Infrastructure/"]
COPY ["AndBank.Domain/AndBank.Domain.csproj", "AndBank.Domain/"]
RUN dotnet restore "./AndBank.Api/AndBank.Api.csproj"
COPY . .
WORKDIR "/src/AndBank.Api"
RUN dotnet build "./AndBank.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AndBank.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AndBank.Api.dll"]