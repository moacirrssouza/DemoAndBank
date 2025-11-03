FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AndBank.ConsoleApp/AndBank.ConsoleApp.csproj", "AndBank.ConsoleApp/"]
COPY ["AndBank.Application/AndBank.Application.csproj", "AndBank.Application/"]
COPY ["AndBank.Infrastructure/AndBank.Infrastructure.csproj", "AndBank.Infrastructure/"]
COPY ["AndBank.Domain/AndBank.Domain.csproj", "AndBank.Domain/"]
RUN dotnet restore "./AndBank.ConsoleApp/AndBank.ConsoleApp.csproj"
COPY . .
WORKDIR "/src/AndBank.ConsoleApp"
RUN dotnet build "./AndBank.ConsoleApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AndBank.ConsoleApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AndBank.ConsoleApp.dll"]