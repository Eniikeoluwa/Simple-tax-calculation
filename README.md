# Nova API - Docker Deployment Guide

This guide explains how to containerize and deploy the Nova API application to Render.

## Docker Setup

The application has been containerized with the following files:
- `Dockerfile` - Multi-stage Docker build configuration
- `.dockerignore` - Excludes unnecessary files from Docker context
- `docker-compose.yml` - Local development setup with PostgreSQL
- `render.yaml` - Render deployment configuration

## Local Development with Docker

### Prerequisites
- Docker and Docker Compose installed
- .NET 9.0 SDK (for local development without Docker)

### Running Locally with Docker Compose

1. **Clone the repository** (if not already done)
   ```bash
   git clone <your-repo-url>
   cd novastart
   ```

2. **Build and run with Docker Compose**
   ```bash
   docker-compose up --build
   ```

3. **Access the application**
   - API: http://localhost:8080
   - Health check: http://localhost:8080/health
   - Swagger (if enabled): http://localhost:8080/swagger

### Building Docker Image Manually

```bash
# Build the Docker image
docker build -t nova-api .

# Run the container
docker run -p 8080:8080 \
  -e DATABASE_URL="your-database-connection-string" \
  -e JWT_SECRET_KEY="your-jwt-secret-key" \
  nova-api
```

## Deploying to Render

### Option 1: Using render.yaml (Recommended)

1. **Push your code to GitHub** (if not already done)

2. **Connect to Render**
   - Go to [Render Dashboard](https://dashboard.render.com)
   - Click "New" → "Blueprint"
   - Connect your GitHub repository
   - Render will automatically detect the `render.yaml` file

3. **Configure Environment Variables** (if needed)
   The `render.yaml` file includes basic configuration, but you may want to set:
   - Custom JWT secret key
   - Additional environment-specific settings

### Option 2: Manual Render Setup

1. **Create a Web Service**
   - Go to Render Dashboard
   - Click "New" → "Web Service"
   - Connect your GitHub repository

2. **Configure the Service**
   - **Name**: nova-api
   - **Environment**: Docker
   - **Region**: Choose your preferred region
   - **Branch**: main (or your deployment branch)
   - **Dockerfile Path**: ./Dockerfile

3. **Set Environment Variables**
   ```
   ASPNETCORE_ENVIRONMENT=Production
   DATABASE_URL=[Your PostgreSQL connection string]
   JWT_SECRET_KEY=[Generate a secure 32+ character key]
   JWT_ISSUER=NovaAPI
   JWT_AUDIENCE=NovaClients
   ```

4. **Create PostgreSQL Database** (if needed)
   - In Render Dashboard, click "New" → "PostgreSQL"
   - Choose your plan and region
   - Copy the connection string to your web service's `DATABASE_URL`

### Environment Variables Reference

| Variable | Description | Required | Default |
|----------|-------------|----------|---------|
| `ASPNETCORE_ENVIRONMENT` | Application environment | Yes | Production |
| `DATABASE_URL` | PostgreSQL connection string | Yes | - |
| `JWT_SECRET_KEY` | JWT signing key (32+ chars) | Yes | - |
| `JWT_ISSUER` | JWT token issuer | No | NovaAPI |
| `JWT_AUDIENCE` | JWT token audience | No | NovaClients |

## Application Features

### Health Check
The application includes a health check endpoint at `/health` that returns:
```json
{
  "status": "healthy",
  "timestamp": "2025-10-26T12:00:00.000Z"
}
```

### Security Features
- JWT authentication
- CORS policy configured
- Production-ready configuration
- Non-root user in Docker container

### Database
- Uses PostgreSQL with Entity Framework Core
- Supports Supabase or any PostgreSQL provider
- Connection string configurable via environment variables

## Troubleshooting

### Common Issues

1. **Database Connection Issues**
   - Verify `DATABASE_URL` format
   - Ensure database server is accessible
   - Check firewall settings

2. **JWT Configuration Issues**
   - Ensure `JWT_SECRET_KEY` is at least 32 characters
   - Verify issuer and audience configuration

3. **Docker Build Issues**
   - Check `.dockerignore` includes necessary files
   - Verify all project references are correct

### Viewing Logs

**Local Docker:**
```bash
docker-compose logs nova-api
```

**Docker container:**
```bash
docker logs <container-name>
```

**Render:**
- View logs in the Render Dashboard under your service

## Production Considerations

1. **Security**
   - Use strong JWT secret keys
   - Configure CORS for your specific domains
   - Enable HTTPS (handled by Render)

2. **Database**
   - Use connection pooling
   - Configure appropriate timeout settings
   - Consider database backup strategies

3. **Monitoring**
   - Use the health check endpoint for monitoring
   - Set up logging aggregation
   - Monitor application metrics

## Support

For deployment issues:
1. Check the application logs
2. Verify environment variable configuration
3. Test the Docker image locally first
4. Consult Render documentation for platform-specific issues