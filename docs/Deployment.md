# Deployment Guide

## Prerequisites
*   Docker Desktop (with Docker Compose v2+)
*   .NET 9 SDK

## Quick Start (Local)

1.  **Start Infrastructure**:
    ```bash
    docker compose up -d
    ```
    This spins up OTel Agent, Gateway, Prometheus, Loki, Tempo, Grafana, and NGINX.

2.  **Start Applications**:
    ```bash
    cd src
    dotnet run --project Service.API/Service.API.csproj &
    dotnet run --project Service.Worker/Service.Worker.csproj &
    ```

## Accessing the Stack
*   **Grafana**: [https://localhost](https://localhost) (Login: `admin`/`admin`)
*   **Prometheus**: [http://localhost:9090](http://localhost:9090)
*   **Tempo**: [http://localhost:3200](http://localhost:3200)

## Reset / Clean Up
To stop all containers and remove the network:
```bash
pkill dotnet
docker compose down
```
