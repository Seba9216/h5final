# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY WebSocketServer.Core/WebSocketServer.Core.csproj WebSocketServer.Core/
COPY WebSocketServer.Server/WebSocketServer.Server.csproj WebSocketServer.Server/
RUN dotnet restore WebSocketServer.Server/WebSocketServer.Server.csproj

COPY . .
RUN dotnet publish WebSocketServer.Server/WebSocketServer.Server.csproj -c Release -o /app/publish

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5057
ENTRYPOINT ["dotnet", "WebSocketServer.Server.dll"]
