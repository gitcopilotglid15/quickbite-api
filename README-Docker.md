# QuickBite API - Docker Setup

This directory contains the Docker configuration for the QuickBite API application.

## Prerequisites

- Docker Desktop installed and running
- .NET 8.0 SDK (for local development)

## Building and Running with Docker

### Option 1: Using Docker directly

```bash
# Build the Docker image
docker build -t quickbite-api .

# Run the container
docker run -d \
  --name quickbite-api \
  -p 5026:5026 \
  -v quickbite_data:/app/data \
  quickbite-api
```

### Option 2: Using Docker Compose (Recommended)

```bash
# Build and start the application
docker-compose up --build -d

# View logs
docker-compose logs -f

# Stop the application
docker-compose down

# Stop and remove volumes (caution: this will delete the database)
docker-compose down -v
```

## Application Details

- **Port**: 5026
- **Framework**: .NET 8.0 Web API
- **Database**: SQLite (persisted in Docker volume)
- **API Documentation**: Available at `http://localhost:5026/swagger`

## Environment Variables

- `ASPNETCORE_ENVIRONMENT`: Set to `Production` by default
- `ASPNETCORE_URLS`: Configured to listen on port 5026

## Health Check

The application includes a health check endpoint that can be used to monitor the container status:
- Health check URL: `http://localhost:5026/health`
- Check interval: 30 seconds
- Timeout: 10 seconds
- Retries: 3

## Data Persistence

The SQLite database is stored in a Docker volume (`quickbite_data`) to ensure data persistence across container restarts.

## Security Features

- Runs as non-root user (appuser) for enhanced security
- Minimal attack surface using official .NET runtime image
- Only necessary ports exposed

## Troubleshooting

### View container logs
```bash
docker logs quickbite-api
# or with docker-compose
docker-compose logs quickbite-api
```

### Execute commands in container
```bash
docker exec -it quickbite-api /bin/bash
```

### Check container health
```bash
docker inspect --format='{{.State.Health.Status}}' quickbite-api
```