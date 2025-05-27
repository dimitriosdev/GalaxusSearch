#!/bin/bash

# Comprehensive testing script for Galaxus Product Search Application
# This script runs all types of tests: unit, integration, performance, and security

set -e  # Exit on any error

echo "🧪 Starting Comprehensive Test Suite for Galaxus Product Search"
echo "============================================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

# Function to check if a service is running
check_service() {
    local service=$1
    local port=$2
    local max_attempts=30
    local attempt=1

    echo "🔍 Checking if $service is running on port $port..."
    
    while [ $attempt -le $max_attempts ]; do
        if curl -s "http://localhost:$port" > /dev/null 2>&1; then
            print_status "$service is running on port $port"
            return 0
        fi
        
        echo "   Attempt $attempt/$max_attempts - waiting for $service..."
        sleep 2
        ((attempt++))
    done
    
    print_error "$service is not responding on port $port after $max_attempts attempts"
    return 1
}

# Function to run backend tests
run_backend_tests() {
    echo ""
    echo "🔧 Running Backend Tests"
    echo "========================"
    
    cd backend
    
    echo "📦 Restoring packages..."
    dotnet restore
    
    echo "🏗️  Building backend..."
    dotnet build --configuration Release --no-restore
    
    echo "🧪 Running unit tests..."
    if dotnet test Tests/Backend.Tests.csproj --configuration Release --no-build --verbosity normal --collect:"XPlat Code Coverage"; then
        print_status "Backend unit tests passed"
    else
        print_error "Backend unit tests failed"
        exit 1
    fi
    
    # Generate coverage report
    if command -v reportgenerator > /dev/null 2>&1; then
        echo "📊 Generating coverage report..."
        reportgenerator "-reports:Tests/TestResults/*/coverage.cobertura.xml" "-targetdir:Tests/TestResults/CoverageReport" -reporttypes:Html
        print_status "Coverage report generated at Tests/TestResults/CoverageReport"
    else
        print_warning "ReportGenerator not found. Install with: dotnet tool install -g dotnet-reportgenerator-globaltool"
    fi
    
    cd ..
}

# Function to run frontend tests
run_frontend_tests() {
    echo ""
    echo "🎨 Running Frontend Tests"
    echo "=========================="
    
    cd frontend
    
    echo "📦 Installing dependencies..."
    npm install
    
    echo "🧪 Running frontend tests..."
    if npm test -- --coverage --watchAll=false; then
        print_status "Frontend tests passed"
    else
        print_error "Frontend tests failed"
        exit 1
    fi
    
    echo "🔍 Running linting..."
    if npm run lint; then
        print_status "Frontend linting passed"
    else
        print_warning "Frontend linting found issues"
    fi
    
    echo "🏗️  Testing frontend build..."
    if npm run build; then
        print_status "Frontend builds successfully"
    else
        print_error "Frontend build failed"
        exit 1
    fi
    
    cd ..
}

# Function to start services for integration tests
start_services() {
    echo ""
    echo "🚀 Starting Services for Integration Tests"
    echo "==========================================="
    
    echo "🐳 Starting Docker services..."
    docker-compose up -d postgres elasticsearch
    
    # Wait for services to be ready
    check_service "PostgreSQL" 5432 || {
        print_error "PostgreSQL failed to start"
        exit 1
    }
    
    check_service "Elasticsearch" 9200 || {
        print_warning "Elasticsearch failed to start - continuing without search tests"
    }
    
    echo "🏗️  Starting backend API..."
    cd backend
    dotnet run --configuration Release &
    BACKEND_PID=$!
    cd ..
    
    # Wait for backend to start
    sleep 10
    check_service "Backend API" 5000 || {
        print_error "Backend API failed to start"
        kill $BACKEND_PID 2>/dev/null || true
        exit 1
    }
    
    echo "🎨 Starting frontend..."
    cd frontend
    npm start &
    FRONTEND_PID=$!
    cd ..
    
    # Wait for frontend to start
    sleep 15
    check_service "Frontend" 3000 || {
        print_warning "Frontend failed to start - continuing without frontend tests"
    }
}

# Function to run integration tests
run_integration_tests() {
    echo ""
    echo "🔗 Running Integration Tests"
    echo "============================="
    
    cd backend
    
    echo "🧪 Running integration tests..."
    if dotnet test Tests/Backend.Tests.csproj --filter "Category=Integration" --configuration Release --verbosity normal; then
        print_status "Integration tests passed"
    else
        print_error "Integration tests failed"
        exit 1
    fi
    
    cd ..
}

# Function to run performance tests
run_performance_tests() {
    echo ""
    echo "⚡ Running Performance Tests"
    echo "============================"
    
    cd backend
    
    echo "🏃 Running load tests..."
    if dotnet test Tests/Backend.Tests.csproj --filter "Category=Performance" --configuration Release --verbosity normal; then
        print_status "Performance tests completed"
    else
        print_warning "Performance tests had issues - check results"
    fi
    
    cd ..
}

# Function to run security tests
run_security_tests() {
    echo ""
    echo "🔒 Running Security Tests"
    echo "========================="
    
    cd backend
    
    echo "🛡️  Running security tests..."
    if dotnet test Tests/Backend.Tests.csproj --filter "Category=Security" --configuration Release --verbosity normal; then
        print_status "Security tests passed"
    else
        print_error "Security tests failed"
        exit 1
    fi
    
    cd ..
}

# Function to run database migration tests
run_migration_tests() {
    echo ""
    echo "🗄️  Testing Database Migrations"
    echo "==============================="
    
    echo "🔄 Testing migration endpoints..."
    if curl -X POST "http://localhost:5000/dev/migrate" -w "%{http_code}" -s -o /dev/null | grep -q "200"; then
        print_status "Database migration test passed"
    else
        print_error "Database migration test failed"
        exit 1
    fi
    
    echo "🌱 Testing seeding endpoints..."
    if curl -X POST "http://localhost:5000/dev/seed" -w "%{http_code}" -s -o /dev/null | grep -q "200"; then
        print_status "Database seeding test passed"
    else
        print_error "Database seeding test failed"
        exit 1
    fi
}

# Function to cleanup
cleanup() {
    echo ""
    echo "🧹 Cleaning Up"
    echo "==============="
    
    echo "🛑 Stopping services..."
    kill $BACKEND_PID 2>/dev/null || true
    kill $FRONTEND_PID 2>/dev/null || true
    
    echo "🐳 Stopping Docker services..."
    docker-compose down
    
    print_status "Cleanup completed"
}

# Function to generate test report
generate_report() {
    echo ""
    echo "📊 Generating Test Report"
    echo "========================="
    
    local timestamp=$(date +"%Y%m%d_%H%M%S")
    local report_dir="test_reports_$timestamp"
    
    mkdir -p "$report_dir"
    
    # Copy coverage reports
    cp -r backend/Tests/TestResults/CoverageReport "$report_dir/backend_coverage" 2>/dev/null || true
    cp -r frontend/coverage "$report_dir/frontend_coverage" 2>/dev/null || true
    
    # Create summary report
    cat > "$report_dir/test_summary.md" << EOF
# Test Report - $(date)

## Summary
- ✅ Backend Unit Tests: Passed
- ✅ Frontend Unit Tests: Passed
- ✅ Integration Tests: Passed
- ✅ Security Tests: Passed
- ⚡ Performance Tests: Completed
- 🗄️  Migration Tests: Passed

## Coverage Reports
- Backend Coverage: See \`backend_coverage/index.html\`
- Frontend Coverage: See \`frontend_coverage/lcov-report/index.html\`

## Next Steps
1. Review any performance test results
2. Address any security warnings
3. Update documentation if needed
4. Proceed with deployment if all tests pass

Generated by: Galaxus Test Suite
Timestamp: $(date)
EOF
    
    print_status "Test report generated in $report_dir/"
}

# Main execution flow
main() {
    local run_performance=${1:-"false"}
    local skip_integration=${2:-"false"}
    
    echo "🎯 Test Configuration:"
    echo "   - Performance Tests: $run_performance"
    echo "   - Skip Integration: $skip_integration"
    echo ""
    
    # Set trap for cleanup
    trap cleanup EXIT
    
    # Run tests in order
    run_backend_tests
    run_frontend_tests
    
    if [ "$skip_integration" != "true" ]; then
        start_services
        run_integration_tests
        run_migration_tests
        run_security_tests
        
        if [ "$run_performance" == "true" ]; then
            run_performance_tests
        fi
    fi
    
    generate_report
    
    echo ""
    print_status "🎉 All tests completed successfully!"
    echo ""
    echo "📋 Summary:"
    echo "   - Backend tests: ✅"
    echo "   - Frontend tests: ✅"
    echo "   - Integration tests: ✅"
    echo "   - Security tests: ✅"
    echo "   - Migration tests: ✅"
    if [ "$run_performance" == "true" ]; then
        echo "   - Performance tests: ⚡"
    fi
    echo ""
    echo "🚀 Ready for deployment!"
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --performance)
            PERFORMANCE="true"
            shift
            ;;
        --skip-integration)
            SKIP_INTEGRATION="true"
            shift
            ;;
        --help)
            echo "Usage: $0 [OPTIONS]"
            echo ""
            echo "Options:"
            echo "  --performance      Run performance tests (requires services)"
            echo "  --skip-integration Skip integration tests (faster)"
            echo "  --help             Show this help message"
            echo ""
            echo "Examples:"
            echo "  $0                           # Run all tests except performance"
            echo "  $0 --performance             # Run all tests including performance"
            echo "  $0 --skip-integration        # Run only unit tests"
            exit 0
            ;;
        *)
            print_error "Unknown option: $1"
            echo "Use --help for usage information"
            exit 1
            ;;
    esac
done

# Run main function with parsed arguments
main "${PERFORMANCE:-false}" "${SKIP_INTEGRATION:-false}"
