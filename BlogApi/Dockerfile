# Stage 1: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Ép Kestrel listen IPv4 + IPv6
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# Stage 2: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY BlogApi.csproj ./
RUN dotnet restore BlogApi.csproj

COPY . .
RUN dotnet publish BlogApi.csproj -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: final
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BlogApi.dll"]
