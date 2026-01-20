# Stage 1: Base image để chạy app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Stage 2: Build app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Quan trọng: Đường dẫn này phải khớp với cấu trúc thư mục của bạn
# Nếu Dockerfile nằm trong thư mục BlogApi, dùng: COPY ["BlogApi.csproj", "."]
COPY ["BlogApi.csproj", "."]
RUN dotnet restore "./BlogApi.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "BlogApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BlogApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Ép .NET 8 chạy cổng 8080 trong container
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "BlogApi.dll"]