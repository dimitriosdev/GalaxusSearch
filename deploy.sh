#!/bin/bash

# Production Deployment Script for Galaxus Product Search Application
# This script handles the complete deployment process for both frontend and backend

set -e  # Exit on any error

# Configuration
PROJECT_NAME="galaxus-product-search"
DOCKER_REGISTRY="${DOCKER_REGISTRY:-your-registry.com}"
ENVIRONMENT="${ENVIRONMENT:-production}"
VERSION="${VERSION:-$(date +%Y%m%d-%H%M%S)}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check prerequisites
check_prerequisites() {
    log_info "Checking prerequisites..."
    
    # Check if Docker is installed and running
    if ! command -v docker &> /dev/null; then
        log_error "Docker is not installed"
        exit 1
    fi
    
    if ! docker info &> /dev/null; then
        log_error "Docker daemon is not running"
        exit 1
    fi
    
    # Check if Node.js is installed
    if ! command -v node &> /dev/null; then
        log_error "Node.js is not installed"
        exit 1
    fi
    
    # Check if .NET is installed
    if ! command -v dotnet &> /dev/null; then
        log_error ".NET is not installed"
        exit 1
    fi
    
    log_success "All prerequisites met"
}

# Build frontend
build_frontend() {
    log_info "Building frontend..."
    
    cd frontend
    
    # Install dependencies
    npm ci --production
    
    # Build the application
    npm run build
    
    # Build Docker image
    docker build -t "${DOCKER_REGISTRY}/${PROJECT_NAME}-frontend:${VERSION}" .
    docker tag "${DOCKER_REGISTRY}/${PROJECT_NAME}-frontend:${VERSION}" "${DOCKER_REGISTRY}/${PROJECT_NAME}-frontend:latest"
    
    cd ..
    log_success "Frontend build completed"
}

# Build backend
build_backend() {
    log_info "Building backend..."
    
    cd backend
    
    # Restore dependencies
    dotnet restore
    
    # Build the application
    dotnet build --configuration Release --no-restore
    
    # Run tests if they exist
    if [ -d "tests" ]; then
        log_info "Running tests..."
        dotnet test --configuration Release --no-build --verbosity normal
    fi
    
    # Build Docker image
    docker build -t "${DOCKER_REGISTRY}/${PROJECT_NAME}-backend:${VERSION}" .
    docker tag "${DOCKER_REGISTRY}/${PROJECT_NAME}-backend:${VERSION}" "${DOCKER_REGISTRY}/${PROJECT_NAME}-backend:latest"
    
    cd ..
    log_success "Backend build completed"
}

# Push images to registry
push_images() {
    log_info "Pushing images to registry..."
    
    docker push "${DOCKER_REGISTRY}/${PROJECT_NAME}-frontend:${VERSION}"
    docker push "${DOCKER_REGISTRY}/${PROJECT_NAME}-frontend:latest"
    
    docker push "${DOCKER_REGISTRY}/${PROJECT_NAME}-backend:${VERSION}"
    docker push "${DOCKER_REGISTRY}/${PROJECT_NAME}-backend:latest"
    
    log_success "Images pushed to registry"
}

# Deploy to production (example for Railway)
deploy_railway() {
    log_info "Deploying to Railway..."
    
    if ! command -v railway &> /dev/null; then
        log_warning "Railway CLI not found. Installing..."
        npm install -g @railway/cli
    fi
    
    # Deploy backend
    log_info "Deploying backend service..."
    cd backend
    railway up --service backend
    cd ..
    
    # Deploy frontend
    log_info "Deploying frontend service..."
    cd frontend
    railway up --service frontend
    cd ..
    
    log_success "Deployment to Railway completed"
}

# Deploy using Docker Compose (for self-hosted)
deploy_docker_compose() {
    log_info "Deploying using Docker Compose..."
    
    # Create production docker-compose file
    cat > docker-compose.prod.yml << EOF
version: '3.8'

services:
  frontend:
    image: ${DOCKER_REGISTRY}/${PROJECT_NAME}-frontend:${VERSION}
    ports:
      - "3000:3000"
    environment:
      - NEXT_PUBLIC_API_URL=\${API_URL}
      - NEXT_PUBLIC_ENVIRONMENT=production
    depends_on:
      - backend
    restart: unless-stopped

  backend:
    image: ${DOCKER_REGISTRY}/${PROJECT_NAME}-backend:${VERSION}
    ports:
      - "5119:5119"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=\${DATABASE_URL}
      - Elasticsearch__Uri=\${ELASTICSEARCH_URL}
    depends_on:
      - postgres
      - elasticsearch
    restart: unless-stopped

  postgres:
    image: postgres:15
    environment:
      - POSTGRES_DB=galaxus
      - POSTGRES_USER=\${POSTGRES_USER}
      - POSTGRES_PASSWORD=\${POSTGRES_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
    restart: unless-stopped

  elasticsearch:
    image: elasticsearch:8.11.0
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false
      - "ES_JAVA_OPTS=-Xms512m -Xmx512m"
    volumes:
      - elasticsearch_data:/usr/share/elasticsearch/data
    restart: unless-stopped

volumes:
  postgres_data:
  elasticsearch_data:
EOF
    
    # Start services
    docker-compose -f docker-compose.prod.yml up -d
    
    log_success "Docker Compose deployment completed"
}

# Health check
health_check() {
    log_info "Performing health checks..."
    
    local backend_url="${BACKEND_URL:-http://localhost:5119}"
    local frontend_url="${FRONTEND_URL:-http://localhost:3000}"
    
    # Check backend health
    log_info "Checking backend health..."
    for i in {1..30}; do
        if curl -f "${backend_url}/health" > /dev/null 2>&1; then
            log_success "Backend health check passed"
            break
        fi
        if [ $i -eq 30 ]; then
            log_error "Backend health check failed"
            exit 1
        fi
        sleep 10
    done
    
    # Check frontend
    log_info "Checking frontend..."
    for i in {1..30}; do
        if curl -f "${frontend_url}" > /dev/null 2>&1; then
            log_success "Frontend health check passed"
            break
        fi
        if [ $i -eq 30 ]; then
            log_error "Frontend health check failed"
            exit 1
        fi
        sleep 10
    done
    
    log_success "All health checks passed"
}

# Rollback function
rollback() {
    local previous_version=$1
    if [ -z "$previous_version" ]; then
        log_error "Previous version not specified"
        exit 1
    fi
    
    log_warning "Rolling back to version: $previous_version"
    
    # Update images to previous version
    docker tag "${DOCKER_REGISTRY}/${PROJECT_NAME}-frontend:${previous_version}" "${DOCKER_REGISTRY}/${PROJECT_NAME}-frontend:latest"
    docker tag "${DOCKER_REGISTRY}/${PROJECT_NAME}-backend:${previous_version}" "${DOCKER_REGISTRY}/${PROJECT_NAME}-backend:latest"
    
    # Restart services
    docker-compose -f docker-compose.prod.yml up -d
    
    log_success "Rollback completed"
}

# Main deployment flow
main() {
    local deployment_type="${1:-docker-compose}"
    
    log_info "Starting deployment process..."
    log_info "Deployment type: $deployment_type"
    log_info "Version: $VERSION"
    
    check_prerequisites
    build_backend
    build_frontend
    
    case $deployment_type in
        "railway")
            deploy_railway
            ;;
        "docker-compose")
            deploy_docker_compose
            ;;
        "build-only")
            log_info "Build-only mode, skipping deployment"
            ;;
        *)
            log_error "Unknown deployment type: $deployment_type"
            echo "Usage: $0 [railway|docker-compose|build-only]"
            exit 1
            ;;
    esac
    
    if [ "$deployment_type" != "build-only" ]; then
        health_check
    fi
    
    log_success "Deployment completed successfully!"
    log_info "Version: $VERSION"
    log_info "Frontend: $FRONTEND_URL"
    log_info "Backend: $BACKEND_URL"
}

# Handle script arguments
case "${1:-}" in
    "rollback")
        rollback "$2"
        ;;
    "health-check")
        health_check
        ;;
    *)
        main "$@"
        ;;
esac
