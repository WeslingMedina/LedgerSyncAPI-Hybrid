# Etapa base para el runtime de .NET
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Etapa de construcción
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar y restaurar dependencias de cada proyecto
COPY LedgerSyncAPI.sln ./
COPY Application/Application.csproj Application/
COPY Domain/Domain.csproj Domain/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY WebAPI/WebAPI.csproj WebAPI/

RUN dotnet restore "LedgerSyncAPI.sln"
RUN dotnet add Infrastructure/Infrastructure.csproj package MySqlConnector --version 2.4.0
# Copiar el resto del código y compilar
COPY . .
WORKDIR /src/WebAPI
RUN dotnet build -c Release -o /app/build

# Publicación
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Imagen final para ejecución
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebAPI.dll"]