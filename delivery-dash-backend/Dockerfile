# syntax=docker/dockerfile:1.7

# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files first for better layer caching on restore
COPY DeliveryDash.sln ./
COPY DeliveryDash.API/DeliveryDash.API.csproj            DeliveryDash.API/
COPY DeliveryDash.Application/DeliveryDash.Application.csproj  DeliveryDash.Application/
COPY DeliveryDash.Domain/DeliveryDash.Domain.csproj      DeliveryDash.Domain/
COPY DeliveryDash.Infrastructure/DeliveryDash.Infrastructure.csproj  DeliveryDash.Infrastructure/
COPY DeliveryDash.Benchmarks/DeliveryDash.Benchmarks.csproj  DeliveryDash.Benchmarks/

RUN dotnet restore DeliveryDash.API/DeliveryDash.API.csproj

# Copy the rest and publish
COPY . .
RUN dotnet publish DeliveryDash.API/DeliveryDash.API.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create uploads dir (ephemeral on Render free tier; mount a volume for persistence)
RUN mkdir -p /app/Uploads && chown -R app:app /app
USER app

COPY --from=build --chown=app:app /app/publish .

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Development \
    DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080

ENTRYPOINT ["dotnet", "DeliveryDash.API.dll"]
