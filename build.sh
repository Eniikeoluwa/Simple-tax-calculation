#!/bin/bash

# Build script for Nova API Docker container

set -e

echo "🏗️  Building Nova API Docker image..."

# Build the Docker image
docker build -t nova-api:latest .

echo "✅ Docker image built successfully!"

echo "🧪 Testing the image..."

# Test that the image can start
docker run --rm -d \
  --name nova-api-test \
  -p 8080:8080 \
  -e DATABASE_URL="Host=localhost;Database=test;Username=test;Password=test" \
  -e JWT_SECRET_KEY="test-secret-key-that-is-at-least-32-characters-long" \
  nova-api:latest

echo "⏳ Waiting for container to start..."
sleep 5

# Check if the health endpoint responds
if curl -f http://localhost:8080/health > /dev/null 2>&1; then
    echo "✅ Health check passed!"
else
    echo "❌ Health check failed!"
    docker logs nova-api-test
fi

# Clean up
echo "🧹 Cleaning up test container..."
docker stop nova-api-test

echo "🎉 Build and test completed!"
echo "📝 To run the container manually:"
echo "   docker run -p 8080:8080 \\"
echo "     -e DATABASE_URL=\"your-database-url\" \\"
echo "     -e JWT_SECRET_KEY=\"your-jwt-secret\" \\"
echo "     nova-api:latest"