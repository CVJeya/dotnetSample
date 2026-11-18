# Deployment Guide: Monolithic & Microservices with Azure DevOps & Docker

A comprehensive reference for deploying applications using both monolithic and microservices architectures with Docker containers and Azure DevOps CI/CD pipelines.

---

## Table of Contents

1. [Docker Fundamentals](#docker-fundamentals)
2. [Monolithic Architecture Deployment](#monolithic-architecture-deployment)
3. [Microservices Architecture Deployment](#microservices-architecture-deployment)
4. [Azure DevOps Pipelines](#azure-devops-pipelines)
5. [Container Orchestration](#container-orchestration)
6. [Monitoring & Logging](#monitoring--logging)
7. [Security & Best Practices](#security--best-practices)

---

# PART 1: DOCKER FUNDAMENTALS

## 1.1 Docker Concepts

**Definition:** Docker is a containerization platform that packages applications, dependencies, and runtime into isolated containers that run consistently across environments.

**Key Concepts:**

```
┌─────────────────────────────────────┐
│          Docker Image               │ ← Blueprint (read-only)
│  ┌─────────────────────────────────┐│
│  │ Application Code                ││
│  │ Dependencies (npm, nuget, etc)  ││
│  │ Runtime (Node, .NET, Python)    ││
│  │ OS Layer (Alpine, Ubuntu)       ││
│  └─────────────────────────────────┘│
└─────────────────────────────────────┘
           ↓ docker run
┌─────────────────────────────────────┐
│      Docker Container               │ ← Running instance
│  ┌─────────────────────────────────┐│
│  │ Isolated environment            ││
│  │ Own file system, processes      ││
│  │ Network interfaces              ││
│  │ Port mappings                   ││
│  └─────────────────────────────────┘│
└─────────────────────────────────────┘
```

---

## 1.2 Dockerfile for .NET Application

**Definition:** A Dockerfile contains instructions to build a Docker image. Multi-stage builds optimize image size.

**Syntax:**

```dockerfile
# Stage 1: Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["MyApi/MyApi.csproj", "MyApi/"]
COPY ["MyApi.Infrastructure/MyApi.Infrastructure.csproj", "MyApi.Infrastructure/"]
RUN dotnet restore "MyApi/MyApi.csproj"

# Copy entire source
COPY . .
WORKDIR "/src/MyApi"

# Build application
RUN dotnet build "MyApi.csproj" -c Release -o /app/build

# Stage 2: Publish stage
FROM build AS publish
RUN dotnet publish "MyApi.csproj" -c Release -o /app/publish

# Stage 3: Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published app from publish stage
COPY --from=publish /app/publish .

# Expose port
EXPOSE 80
EXPOSE 443

# Environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "MyApi.dll"]
```

**Dockerfile for React Application:**

```dockerfile
# Stage 1: Build
FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci --only=production && npm cache clean --force

COPY . .
RUN npm run build

# Stage 2: Serve
FROM node:20-alpine
WORKDIR /app

RUN npm install -g serve

COPY --from=build /app/dist ./dist

EXPOSE 3000

HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:3000 || exit 1

CMD ["serve", "-s", "dist", "-l", "3000"]
```

---

## 1.3 Docker Compose for Local Development

**Definition:** Docker Compose manages multi-container applications using a YAML configuration file.

**docker-compose.yml:**

```yaml
version: '3.9'

services:
  # API Service
  api:
    build:
      context: .
      dockerfile: MyApi/Dockerfile
    container_name: myapp-api
    ports:
      - "5001:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__Default=Server=sqlserver;Database=MyDb;User Id=sa;Password=YourPassword123;
    depends_on:
      - sqlserver
    networks:
      - myapp-network
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost/health"]
      interval: 30s
      timeout: 3s
      retries: 3

  # React Frontend
  frontend:
    build:
      context: ./clients/dashboard
      dockerfile: Dockerfile
    container_name: myapp-frontend
    ports:
      - "3000:3000"
    environment:
      - VITE_API_BASE_URL=http://localhost:5001
    depends_on:
      - api
    networks:
      - myapp-network

  # SQL Server Database
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: myapp-db
    ports:
      - "1433:1433"
    environment:
      - SA_PASSWORD=YourPassword123
      - ACCEPT_EULA=Y
    volumes:
      - sqlserver_data:/var/opt/mssql
    networks:
      - myapp-network
    healthcheck:
      test: ["CMD", "/opt/mssql-tools/bin/sqlcmd", "-S", "localhost", "-U", "sa", "-P", "YourPassword123", "-Q", "SELECT 1"]
      interval: 10s
      timeout: 5s
      retries: 5

  # Redis Cache (optional)
  redis:
    image: redis:7-alpine
    container_name: myapp-redis
    ports:
      - "6379:6379"
    networks:
      - myapp-network
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 3s
      retries: 3

networks:
  myapp-network:
    driver: bridge

volumes:
  sqlserver_data:
```

**Docker Compose Commands:**

```bash
# Start all services
docker-compose up -d

# Stop all services
docker-compose down

# View logs
docker-compose logs -f api

# Rebuild images
docker-compose up -d --build

# Run database migrations
docker-compose exec api dotnet ef database update

# Remove volumes (WARNING: deletes data)
docker-compose down -v
```

---

## 1.4 Docker Image Building & Pushing

**Syntax:**

```bash
# Build image
docker build -t myapp-api:1.0.0 -f MyApi/Dockerfile .

# Tag image for registry
docker tag myapp-api:1.0.0 myregistry.azurecr.io/myapp-api:1.0.0

# Login to Azure Container Registry
az acr login --name myregistry

# Push to registry
docker push myregistry.azurecr.io/myapp-api:1.0.0

# List local images
docker images

# Remove image
docker rmi myapp-api:1.0.0

# Run container
docker run -d -p 5001:80 myapp-api:1.0.0

# View running containers
docker ps

# Stop container
docker stop <container-id>

# View logs
docker logs <container-id>

# Execute command in container
docker exec <container-id> dotnet ef database update
```

---

# PART 2: MONOLITHIC ARCHITECTURE DEPLOYMENT

## 2.1 Monolithic Application Structure

**Definition:** A single codebase and deployment unit containing all business logic, API, and dependencies.

**Project Structure:**

```
MonolithicApp/
├── .github/
│   └── workflows/                    # GitHub Actions
├── .docker/
│   └── Dockerfile                    # Single Dockerfile
├── src/
│   ├── MyApp.Api/                   # API layer
│   │   ├── Controllers/
│   │   ├── Program.cs
│   │   └── MyApp.Api.csproj
│   ├── MyApp.Application/           # Business logic
│   ├── MyApp.Infrastructure/        # Data access
│   └── MyApp.Domain/                # Domain models
├── clients/
│   ├── dashboard/                    # React frontend
│   ├── admin/                        # Admin portal
│   └── Dockerfile.frontend
├── docker-compose.yml
├── azure-pipelines.yml               # Azure DevOps pipeline
└── README.md
```

---

## 2.2 Docker File Structure for Monolith

**Dockerfile (Multi-stage):**

```dockerfile
# ============================================
# Stage 1: Build Backend (.NET API)
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dotnet-build
WORKDIR /src

COPY ["src/MyApp.Api/MyApp.Api.csproj", "MyApp.Api/"]
COPY ["src/MyApp.Application/MyApp.Application.csproj", "MyApp.Application/"]
COPY ["src/MyApp.Infrastructure/MyApp.Infrastructure.csproj", "MyApp.Infrastructure/"]
COPY ["src/MyApp.Domain/MyApp.Domain.csproj", "MyApp.Domain/"]

RUN dotnet restore "MyApp.Api/MyApp.Api.csproj"
COPY . .
WORKDIR "/src/MyApp.Api"
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

# ============================================
# Stage 2: Build Frontend (React)
# ============================================
FROM node:20-alpine AS node-build
WORKDIR /app

COPY clients/dashboard/package*.json ./
RUN npm ci

COPY clients/dashboard/ .
RUN npm run build

# ============================================
# Stage 3: Runtime - Combine Both
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy .NET app
COPY --from=dotnet-build /app/publish .

# Install Node runtime for serving frontend
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy React build
COPY --from=node-build /app/dist ./wwwroot

EXPOSE 80 443

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

ENTRYPOINT ["dotnet", "MyApp.Api.dll"]
```

---

## 2.3 Local Development with Docker Compose

**docker-compose.yml for Monolith:**

```yaml
version: '3.9'

services:
  app:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: monolith-app
    ports:
      - "5000:80"
      - "5001:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__Default=Server=sqlserver;Database=MonolithDb;User Id=sa;Password=YourPassword123;TrustServerCertificate=true
      - Redis__Connection=redis:6379
      - Jwt__Secret=your-secret-key-min-32-characters
    depends_on:
      sqlserver:
        condition: service_healthy
      redis:
        condition: service_healthy
    networks:
      - mono-network
    volumes:
      - ./src:/app/src:ro
      - ./clients:/app/clients:ro

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: mono-sqlserver
    ports:
      - "1433:1433"
    environment:
      - SA_PASSWORD=YourPassword123
      - ACCEPT_EULA=Y
    volumes:
      - sqlserver_data:/var/opt/mssql
    networks:
      - mono-network
    healthcheck:
      test: ["CMD", "/opt/mssql-tools/bin/sqlcmd", "-S", "localhost", "-U", "sa", "-P", "YourPassword123", "-Q", "SELECT 1"]
      interval: 10s
      timeout: 5s
      retries: 5

  redis:
    image: redis:7-alpine
    container_name: mono-redis
    ports:
      - "6379:6379"
    networks:
      - mono-network
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 3s
      retries: 3

networks:
  mono-network:
    driver: bridge

volumes:
  sqlserver_data:
```

---

## 2.4 Azure DevOps Pipeline for Monolith

**azure-pipelines.yml:**

```yaml
trigger:
  branches:
    include:
      - main
      - develop
  paths:
    include:
      - 'src/**'
      - 'clients/**'
      - 'Dockerfile'
      - 'azure-pipelines.yml'

pool:
  vmImage: 'ubuntu-latest'

variables:
  dockerRegistryServiceConnection: 'myregistry'
  imageRepository: 'myapp-monolith'
  containerRegistry: 'myregistry.azurecr.io'
  dockerfilePath: '$(Build.SourcesDirectory)/Dockerfile'
  tag: '$(Build.BuildId)'

stages:
  # ================================================
  # Stage 1: Build & Test
  # ================================================
  - stage: Build
    displayName: Build and Test
    jobs:
      - job: BuildBackend
        displayName: Build .NET Backend
        steps:
          - task: UseDotNet@2
            inputs:
              packageType: 'sdk'
              version: '8.0.x'

          - task: DotNetCoreCLI@2
            displayName: 'Restore NuGet packages'
            inputs:
              command: 'restore'
              projects: '**/MyApp.Api.csproj'

          - task: DotNetCoreCLI@2
            displayName: 'Build backend'
            inputs:
              command: 'build'
              projects: '**/MyApp.Api.csproj'
              arguments: '--configuration Release'

          - task: DotNetCoreCLI@2
            displayName: 'Run unit tests'
            inputs:
              command: 'test'
              projects: '**/*Tests.csproj'
              arguments: '--configuration Release --no-build'

      - job: BuildFrontend
        displayName: Build React Frontend
        steps:
          - task: NodeTool@0
            inputs:
              versionSpec: '20.x'

          - task: Npm@1
            displayName: 'Install dependencies'
            inputs:
              command: 'install'
              workingDir: 'clients/dashboard'

          - task: Npm@1
            displayName: 'Build React app'
            inputs:
              command: 'custom'
              customCommand: 'run build'
              workingDir: 'clients/dashboard'

          - task: Npm@1
            displayName: 'Run frontend tests'
            inputs:
              command: 'test'
              workingDir: 'clients/dashboard'
              arguments: '--coverage --watchAll=false'

  # ================================================
  # Stage 2: Build & Push Docker Image
  # ================================================
  - stage: DockerBuild
    displayName: Build Docker Image
    dependsOn: Build
    condition: succeeded()
    jobs:
      - job: BuildPushImage
        displayName: Build and Push Docker Image
        steps:
          - task: Docker@2
            displayName: 'Build Docker image'
            inputs:
              command: 'build'
              Dockerfile: '$(dockerfilePath)'
              tags: |
                $(containerRegistry)/$(imageRepository):$(tag)
                $(containerRegistry)/$(imageRepository):latest

          - task: Docker@2
            displayName: 'Push image to Azure Container Registry'
            inputs:
              command: 'push'
              containerRegistryType: 'Azure Container Registry'
              containerRegistry: '$(dockerRegistryServiceConnection)'
              repository: '$(imageRepository)'
              tags: |
                $(tag)
                latest

  # ================================================
  # Stage 3: Deploy to Dev Environment
  # ================================================
  - stage: DeployDev
    displayName: Deploy to Dev
    dependsOn: DockerBuild
    condition: succeeded()
    jobs:
      - deployment: DeployToDev
        displayName: Deploy to Azure App Service (Dev)
        environment: 'Development'
        strategy:
          runOnce:
            deploy:
              steps:
                - task: AzureWebAppContainer@1
                  displayName: 'Deploy to Azure App Service'
                  inputs:
                    azureSubscription: 'MyAzureSubscription'
                    appName: 'myapp-dev'
                    containers: '$(containerRegistry)/$(imageRepository):$(tag)'
                    containerCommand: ''

                - task: AzureCLI@2
                  displayName: 'Run database migrations'
                  inputs:
                    azureSubscription: 'MyAzureSubscription'
                    scriptType: 'bash'
                    scriptLocation: 'inlineScript'
                    inlineScript: |
                      az container create \
                        --resource-group MyResourceGroup \
                        --name migrate-$(Build.BuildId) \
                        --image $(containerRegistry)/$(imageRepository):$(tag) \
                        --environment-variables \
                          ConnectionStrings__Default='$(DevDbConnectionString)' \
                          ASPNETCORE_ENVIRONMENT='Development' \
                        --command-line 'dotnet ef database update --project src/MyApp.Api/MyApp.Api.csproj'

  # ================================================
  # Stage 4: Deploy to Staging
  # ================================================
  - stage: DeployStaging
    displayName: Deploy to Staging
    dependsOn: DeployDev
    condition: succeeded()
    jobs:
      - deployment: DeployToStaging
        displayName: Deploy to Azure Container Instances (Staging)
        environment: 'Staging'
        strategy:
          runOnce:
            deploy:
              steps:
                - task: AzureContainerApps@1
                  displayName: 'Deploy to Azure Container Apps'
                  inputs:
                    azureSubscription: 'MyAzureSubscription'
                    containerAppName: 'myapp-staging'
                    resourceGroup: 'MyResourceGroup'
                    imageToDeploy: '$(containerRegistry)/$(imageRepository):$(tag)'
                    containerPort: 80
                    environmentVariables: |
                      ASPNETCORE_ENVIRONMENT=Staging
                      ConnectionStrings__Default=$(StagingDbConnectionString)

  # ================================================
  # Stage 5: Deploy to Production
  # ================================================
  - stage: DeployProd
    displayName: Deploy to Production
    dependsOn: DeployStaging
    condition: succeeded()
    jobs:
      - deployment: ApprovalGate
        displayName: Manual Approval
        environment: 'Production'
        strategy:
          runOnce:
            deploy:
              steps:
                - script: echo "Waiting for approval..."

      - deployment: DeployToProd
        displayName: Deploy to AKS (Production)
        environment: 'Production'
        dependsOn: ApprovalGate
        condition: succeeded()
        strategy:
          runOnce:
            deploy:
              steps:
                - task: KubernetesManifest@0
                  displayName: 'Deploy to AKS'
                  inputs:
                    action: 'deploy'
                    kubernetesServiceConnection: 'AKS-Production'
                    namespace: 'default'
                    manifests: |
                      $(Pipeline.Workspace)/k8s/deployment.yaml
                      $(Pipeline.Workspace)/k8s/service.yaml

                - task: Kubernetes@1
                  displayName: 'Verify deployment'
                  inputs:
                    kubernetesServiceConnection: 'AKS-Production'
                    command: 'rollout status'
                    arguments: 'deployment/myapp-monolith -n default'
```

---

# PART 3: MICROSERVICES ARCHITECTURE DEPLOYMENT

## 3.1 Microservices Project Structure

**Definition:** Multiple independent services, each with own repository, Dockerfile, and deployment pipeline.

**Repository Structure:**

```
monorepo/
├── services/
│   ├── UserService/
│   │   ├── UserService.Api/
│   │   ├── UserService.Domain/
│   │   ├── Dockerfile
│   │   └── azure-pipelines.yml
│   ├── OrderService/
│   │   ├── OrderService.Api/
│   │   ├── OrderService.Domain/
│   │   ├── Dockerfile
│   │   └── azure-pipelines.yml
│   ├── ProductService/
│   │   ├── ProductService.Api/
│   │   ├── ProductService.Domain/
│   │   ├── Dockerfile
│   │   └── azure-pipelines.yml
│   └── NotificationService/
│       ├── NotificationService.Api/
│       ├── Dockerfile
│       └── azure-pipelines.yml
├── clients/
│   ├── dashboard/
│   │   └── Dockerfile
│   └── admin/
│       └── Dockerfile
├── shared/
│   └── Shared.Libraries/
├── k8s/
│   ├── namespace.yaml
│   ├── configmap.yaml
│   ├── secrets.yaml
│   ├── user-service/
│   │   ├── deployment.yaml
│   │   └── service.yaml
│   ├── order-service/
│   │   ├── deployment.yaml
│   │   └── service.yaml
│   ├── api-gateway/
│   │   ├── deployment.yaml
│   │   └── service.yaml
│   └── ingress.yaml
├── docker-compose.yml
└── README.md
```

---

## 3.2 Individual Service Dockerfile

**UserService/Dockerfile:**

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["services/UserService/UserService.Api/UserService.Api.csproj", "UserService.Api/"]
COPY ["services/UserService/UserService.Domain/UserService.Domain.csproj", "UserService.Domain/"]
COPY ["shared/Shared.Libraries/Shared.Libraries.csproj", "Shared.Libraries/"]

RUN dotnet restore "UserService.Api/UserService.Api.csproj"
COPY . .
WORKDIR "/src/services/UserService/UserService.Api"
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80 443

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

ENTRYPOINT ["dotnet", "UserService.Api.dll"]
```

---

## 3.3 Docker Compose for Microservices (Local)

**docker-compose.yml:**

```yaml
version: '3.9'

services:
  # API Gateway
  api-gateway:
    build:
      context: .
      dockerfile: services/ApiGateway/Dockerfile
    container_name: gateway
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - UserService__Url=http://user-service:80
      - OrderService__Url=http://order-service:80
      - ProductService__Url=http://product-service:80
    depends_on:
      - user-service
      - order-service
      - product-service
    networks:
      - microservices

  # User Service
  user-service:
    build:
      context: .
      dockerfile: services/UserService/Dockerfile
    container_name: user-service
    ports:
      - "5001:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__Default=Server=sqlserver-users;Database=UserDb;User Id=sa;Password=YourPassword123;
      - RabbitMq__Host=rabbitmq
      - RabbitMq__Username=guest
      - RabbitMq__Password=guest
    depends_on:
      sqlserver-users:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy
    networks:
      - microservices

  # Order Service
  order-service:
    build:
      context: .
      dockerfile: services/OrderService/Dockerfile
    container_name: order-service
    ports:
      - "5002:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__Default=Server=sqlserver-orders;Database=OrderDb;User Id=sa;Password=YourPassword123;
      - RabbitMq__Host=rabbitmq
      - RabbitMq__Username=guest
      - RabbitMq__Password=guest
    depends_on:
      sqlserver-orders:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy
    networks:
      - microservices

  # Product Service
  product-service:
    build:
      context: .
      dockerfile: services/ProductService/Dockerfile
    container_name: product-service
    ports:
      - "5003:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__Default=Server=sqlserver-products;Database=ProductDb;User Id=sa;Password=YourPassword123;
    depends_on:
      sqlserver-products:
        condition: service_healthy
    networks:
      - microservices

  # Frontend
  dashboard:
    build:
      context: ./clients/dashboard
      dockerfile: Dockerfile
    container_name: dashboard
    ports:
      - "3000:3000"
    environment:
      - VITE_API_BASE_URL=http://localhost:5000
    depends_on:
      - api-gateway
    networks:
      - microservices

  # Databases (separate per service)
  sqlserver-users:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: sqlserver-users
    environment:
      - SA_PASSWORD=YourPassword123
      - ACCEPT_EULA=Y
    volumes:
      - sqlserver_users_data:/var/opt/mssql
    networks:
      - microservices
    healthcheck:
      test: ["CMD", "/opt/mssql-tools/bin/sqlcmd", "-S", "localhost", "-U", "sa", "-P", "YourPassword123", "-Q", "SELECT 1"]
      interval: 10s
      timeout: 5s
      retries: 5

  sqlserver-orders:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: sqlserver-orders
    environment:
      - SA_PASSWORD=YourPassword123
      - ACCEPT_EULA=Y
    volumes:
      - sqlserver_orders_data:/var/opt/mssql
    networks:
      - microservices
    healthcheck:
      test: ["CMD", "/opt/mssql-tools/bin/sqlcmd", "-S", "localhost", "-U", "sa", "-P", "YourPassword123", "-Q", "SELECT 1"]
      interval: 10s
      timeout: 5s
      retries: 5

  sqlserver-products:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: sqlserver-products
    environment:
      - SA_PASSWORD=YourPassword123
      - ACCEPT_EULA=Y
    volumes:
      - sqlserver_products_data:/var/opt/mssql
    networks:
      - microservices
    healthcheck:
      test: ["CMD", "/opt/mssql-tools/bin/sqlcmd", "-S", "localhost", "-U", "sa", "-P", "YourPassword123", "-Q", "SELECT 1"]
      interval: 10s
      timeout: 5s
      retries: 5

  # Message Broker
  rabbitmq:
    image: rabbitmq:3.13-management-alpine
    container_name: rabbitmq
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      - RABBITMQ_DEFAULT_USER=guest
      - RABBITMQ_DEFAULT_PASS=guest
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    networks:
      - microservices
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

networks:
  microservices:
    driver: bridge

volumes:
  sqlserver_users_data:
  sqlserver_orders_data:
  sqlserver_products_data:
  rabbitmq_data:
```

---

## 3.4 Azure DevOps Pipeline for Microservices

**services/UserService/azure-pipelines.yml:**

```yaml
trigger:
  branches:
    include:
      - main
      - develop
  paths:
    include:
      - 'services/UserService/**'
      - 'shared/**'
      - 'k8s/user-service/**'

pool:
  vmImage: 'ubuntu-latest'

variables:
  dockerRegistryServiceConnection: 'myregistry'
  imageRepository: 'user-service'
  containerRegistry: 'myregistry.azurecr.io'
  dockerfilePath: '$(Build.SourcesDirectory)/services/UserService/Dockerfile'
  tag: '$(Build.BuildId)'
  buildContext: '$(Build.SourcesDirectory)'

stages:
  # ================================================
  # Stage 1: Build & Test Service
  # ================================================
  - stage: Build
    displayName: Build User Service
    jobs:
      - job: BuildTest
        displayName: Build and Test
        steps:
          - task: UseDotNet@2
            inputs:
              packageType: 'sdk'
              version: '8.0.x'

          - task: DotNetCoreCLI@2
            displayName: 'Restore'
            inputs:
              command: 'restore'
              projects: '**/UserService.Api.csproj'

          - task: DotNetCoreCLI@2
            displayName: 'Build'
            inputs:
              command: 'build'
              projects: '**/UserService.Api.csproj'
              arguments: '--configuration Release'

          - task: DotNetCoreCLI@2
            displayName: 'Unit Tests'
            inputs:
              command: 'test'
              projects: '**/*UserService*Tests.csproj'

  # ================================================
  # Stage 2: Build Docker Image
  # ================================================
  - stage: DockerBuild
    displayName: Build Docker Image
    dependsOn: Build
    condition: succeeded()
    jobs:
      - job: BuildImage
        displayName: Build and Push
        steps:
          - task: Docker@2
            displayName: 'Build Image'
            inputs:
              command: 'build'
              Dockerfile: '$(dockerfilePath)'
              tags: |
                $(containerRegistry)/$(imageRepository):$(tag)
                $(containerRegistry)/$(imageRepository):latest

          - task: Docker@2
            displayName: 'Push to ACR'
            inputs:
              command: 'push'
              containerRegistryType: 'Azure Container Registry'
              containerRegistry: '$(dockerRegistryServiceConnection)'
              repository: '$(imageRepository)'
              tags: |
                $(tag)
                latest

  # ================================================
  # Stage 3: Deploy to Dev
  # ================================================
  - stage: DeployDev
    displayName: Deploy to Dev (AKS)
    dependsOn: DockerBuild
    condition: succeeded()
    jobs:
      - deployment: DeployDev
        displayName: Deploy to Development
        environment: 'Development'
        strategy:
          runOnce:
            deploy:
              steps:
                - task: KubernetesManifest@0
                  displayName: 'Update Image in Deployment'
                  inputs:
                    action: 'createSecret'
                    kubernetesServiceConnection: 'AKS-Dev'
                    namespace: 'default'
                    secretType: 'dockerRegistry'
                    secretName: 'acr-secret'
                    dockerRegistryEndpoint: '$(dockerRegistryServiceConnection)'

                - task: KubernetesManifest@0
                  displayName: 'Deploy to Dev'
                  inputs:
                    action: 'deploy'
                    kubernetesServiceConnection: 'AKS-Dev'
                    namespace: 'default'
                    manifests: |
                      k8s/user-service/deployment.yaml
                      k8s/user-service/service.yaml

                - task: Kubernetes@1
                  displayName: 'Verify Rollout'
                  inputs:
                    kubernetesServiceConnection: 'AKS-Dev'
                    command: 'rollout status'
                    arguments: 'deployment/user-service -n default'

  # ================================================
  # Stage 4: Integration Tests
  # ================================================
  - stage: IntegrationTests
    displayName: Integration Tests
    dependsOn: DeployDev
    condition: succeeded()
    jobs:
      - job: RunTests
        displayName: Run Integration Tests
        steps:
          - task: Kubernetes@1
            displayName: 'Port Forward to Service'
            inputs:
              kubernetesServiceConnection: 'AKS-Dev'
              command: 'exec'
              arguments: 'pod/test-pod -c test-runner -- npm run test:integration'

  # ================================================
  # Stage 5: Deploy to Staging
  # ================================================
  - stage: DeployStaging
    displayName: Deploy to Staging
    dependsOn: IntegrationTests
    condition: succeeded()
    jobs:
      - deployment: DeployStaging
        displayName: Deploy to Staging
        environment: 'Staging'
        strategy:
          runOnce:
            deploy:
              steps:
                - task: KubernetesManifest@0
                  displayName: 'Deploy'
                  inputs:
                    action: 'deploy'
                    kubernetesServiceConnection: 'AKS-Staging'
                    namespace: 'default'
                    manifests: |
                      k8s/user-service/deployment.yaml
                      k8s/user-service/service.yaml

  # ================================================
  # Stage 6: Deploy to Production
  # ================================================
  - stage: DeployProd
    displayName: Deploy to Production
    dependsOn: DeployStaging
    condition: succeeded()
    jobs:
      - deployment: DeployProd
        displayName: Manual Approval + Deploy
        environment: 'Production'
        strategy:
          runOnce:
            deploy:
              steps:
                - task: KubernetesManifest@0
                  displayName: 'Deploy with Blue-Green'
                  inputs:
                    action: 'deploy'
                    kubernetesServiceConnection: 'AKS-Production'
                    namespace: 'default'
                    manifests: |
                      k8s/user-service/deployment.yaml
                      k8s/user-service/service.yaml

                - task: Kubernetes@1
                  displayName: 'Health Check'
                  inputs:
                    kubernetesServiceConnection: 'AKS-Production'
                    command: 'get'
                    arguments: 'pods -n default -l app=user-service'
```

---

# PART 4: CONTAINER ORCHESTRATION (KUBERNETES)

## 4.1 Kubernetes Deployment Manifest

**k8s/user-service/deployment.yaml:**

```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: myapp

---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: user-service
  namespace: myapp
  labels:
    app: user-service
spec:
  replicas: 3
  selector:
    matchLabels:
      app: user-service
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1
      maxUnavailable: 0

  template:
    metadata:
      labels:
        app: user-service
        version: v1
    spec:
      # Image pull secret for private registry
      imagePullSecrets:
        - name: acr-secret

      containers:
        - name: user-service
          image: myregistry.azurecr.io/user-service:latest
          imagePullPolicy: Always
          ports:
            - containerPort: 80
              name: http
              protocol: TCP

          # Environment variables
          env:
            - name: ASPNETCORE_ENVIRONMENT
              value: "Production"
            - name: ConnectionStrings__Default
              valueFrom:
                secretKeyRef:
                  name: user-service-secrets
                  key: db-connection-string
            - name: RabbitMq__Host
              valueFrom:
                configMapKeyRef:
                  name: user-service-config
                  key: rabbitmq-host
            - name: RabbitMq__Username
              valueFrom:
                secretKeyRef:
                  name: user-service-secrets
                  key: rabbitmq-username
            - name: RabbitMq__Password
              valueFrom:
                secretKeyRef:
                  name: user-service-secrets
                  key: rabbitmq-password

          # Resource limits
          resources:
            requests:
              cpu: "100m"
              memory: "256Mi"
            limits:
              cpu: "500m"
              memory: "512Mi"

          # Liveness probe (restart if unhealthy)
          livenessProbe:
            httpGet:
              path: /health
              port: 80
            initialDelaySeconds: 30
            periodSeconds: 10
            timeoutSeconds: 5
            failureThreshold: 3

          # Readiness probe (remove from service if not ready)
          readinessProbe:
            httpGet:
              path: /health/ready
              port: 80
            initialDelaySeconds: 10
            periodSeconds: 5
            timeoutSeconds: 3
            failureThreshold: 2

          # Volume mounts
          volumeMounts:
            - name: config
              mountPath: /app/config
              readOnly: true

      # Service account
      serviceAccountName: user-service

      # Volumes
      volumes:
        - name: config
          configMap:
            name: user-service-config

---
apiVersion: v1
kind: Service
metadata:
  name: user-service
  namespace: myapp
  labels:
    app: user-service
spec:
  type: ClusterIP
  selector:
    app: user-service
  ports:
    - port: 80
      targetPort: 80
      protocol: TCP
      name: http

---
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: user-service-hpa
  namespace: myapp
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: user-service
  minReplicas: 2
  maxReplicas: 10
  metrics:
    - type: Resource
      resource:
        name: cpu
        target:
          type: Utilization
          averageUtilization: 70
    - type: Resource
      resource:
        name: memory
        target:
          type: Utilization
          averageUtilization: 80

---
apiVersion: policy/v1
kind: PodDisruptionBudget
metadata:
  name: user-service-pdb
  namespace: myapp
spec:
  minAvailable: 1
  selector:
    matchLabels:
      app: user-service

---
apiVersion: v1
kind: ConfigMap
metadata:
  name: user-service-config
  namespace: myapp
data:
  rabbitmq-host: "rabbitmq.myapp.svc.cluster.local"
  log-level: "Information"
  cache-ttl: "300"

---
apiVersion: v1
kind: Secret
metadata:
  name: user-service-secrets
  namespace: myapp
type: Opaque
stringData:
  db-connection-string: "Server=sqlserver.myapp.svc.cluster.local;Database=UserDb;User Id=sa;Password=YourSecurePassword;"
  rabbitmq-username: "guest"
  rabbitmq-password: "guest"

---
apiVersion: v1
kind: ServiceAccount
metadata:
  name: user-service
  namespace: myapp
```

---

## 4.2 Kubernetes Ingress & Network Policy

**k8s/ingress.yaml:**

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: api-gateway-ingress
  namespace: myapp
  annotations:
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
    nginx.ingress.kubernetes.io/rate-limit: "100"
spec:
  ingressClassName: nginx
  tls:
    - hosts:
        - api.myapp.com
      secretName: api-tls-cert
  rules:
    - host: api.myapp.com
      http:
        paths:
          - path: /users
            pathType: Prefix
            backend:
              service:
                name: user-service
                port:
                  number: 80
          - path: /orders
            pathType: Prefix
            backend:
              service:
                name: order-service
                port:
                  number: 80
          - path: /products
            pathType: Prefix
            backend:
              service:
                name: product-service
                port:
                  number: 80

---
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: user-service-network-policy
  namespace: myapp
spec:
  podSelector:
    matchLabels:
      app: user-service
  policyTypes:
    - Ingress
    - Egress
  ingress:
    - from:
        - namespaceSelector:
            matchLabels:
              name: myapp
        - podSelector:
            matchLabels:
              app: api-gateway
      ports:
        - protocol: TCP
          port: 80
  egress:
    - to:
        - namespaceSelector: {}
      ports:
        - protocol: TCP
          port: 443
    - to:
        - podSelector:
            matchLabels:
              app: sqlserver
      ports:
        - protocol: TCP
          port: 1433
    - to:
        - podSelector:
            matchLabels:
              app: rabbitmq
      ports:
        - protocol: TCP
          port: 5672
```

---

# PART 5: MONITORING & LOGGING

## 5.1 Application Insights Integration

**Program.cs (.NET):**

```csharp
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

var builder = WebApplication.CreateBuilder(args);

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// Add logging
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddApplicationInsights();
    loggingBuilder.AddConsole();
});

var app = builder.Build();

// Middleware for request/response logging
app.UseMiddleware<LoggingMiddleware>();

app.MapControllers();
app.Run();

// Custom logging middleware
public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, TelemetryClient telemetryClient, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _telemetryClient = telemetryClient;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            await _next(context);

            stopwatch.Stop();
            _logger.LogInformation("Request {Method} {Path} completed in {ElapsedMilliseconds}ms with status {StatusCode}",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds,
                context.Response.StatusCode);

            // Track custom event
            var properties = new Dictionary<string, string>
            {
                { "Method", context.Request.Method },
                { "Path", context.Request.Path.ToString() },
                { "StatusCode", context.Response.StatusCode.ToString() }
            };
            var metrics = new Dictionary<string, double>
            {
                { "Duration", stopwatch.ElapsedMilliseconds }
            };
            _telemetryClient.TrackEvent("RequestCompleted", properties, metrics);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Request {Method} {Path} failed after {ElapsedMilliseconds}ms",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds);

            _telemetryClient.TrackException(ex);
            throw;
        }
    }
}
```

**appsettings.json:**

```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-instrumentation-key",
    "ConnectionString": "InstrumentationKey=your-key;IngestionEndpoint=https://eastus-8.in.applicationinsights.azure.com/;LiveEndpoint=https://eastus.livediagnostics.monitor.azure.com/"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

---

## 5.2 ELK Stack (Elasticsearch, Logstash, Kibana) with Docker

**docker-compose.yml (ELK):**

```yaml
version: '3.9'

services:
  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.11.0
    container_name: elasticsearch
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false
      - "ES_JAVA_OPTS=-Xms512m -Xmx512m"
    ports:
      - "9200:9200"
    volumes:
      - elasticsearch_data:/usr/share/elasticsearch/data
    networks:
      - elk

  logstash:
    image: docker.elastic.co/logstash/logstash:8.11.0
    container_name: logstash
    ports:
      - "5000:5000/tcp"
      - "5000:5000/udp"
    volumes:
      - ./logstash.conf:/usr/share/logstash/pipeline/logstash.conf
      - logstash_data:/usr/share/logstash/data
    environment:
      - "LS_JAVA_OPTS=-Xmx256m -Xms256m"
    depends_on:
      - elasticsearch
    networks:
      - elk

  kibana:
    image: docker.elastic.co/kibana/kibana:8.11.0
    container_name: kibana
    ports:
      - "5601:5601"
    depends_on:
      - elasticsearch
    environment:
      - ELASTICSEARCH_HOSTS=http://elasticsearch:9200
    networks:
      - elk

networks:
  elk:
    driver: bridge

volumes:
  elasticsearch_data:
  logstash_data:
```

---

# PART 6: SECURITY & BEST PRACTICES

## 6.1 Container Security Best Practices

**Dockerfile Security:**

```dockerfile
# Use specific base image version (not :latest)
FROM mcr.microsoft.com/dotnet/aspnet:8.0.1 AS base

# Run as non-root user
RUN useradd -m -u 1001 appuser

WORKDIR /app

# Copy only necessary files
COPY --from=build --chown=appuser:appuser /app/publish .

# Switch to non-root user
USER appuser

# Make filesystem read-only where possible
RUN mkdir -p /tmp && chmod 1777 /tmp

# Explicitly declare exposed ports
EXPOSE 80

HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

ENTRYPOINT ["dotnet", "MyApp.dll"]
```

**Image Scanning:**

```bash
# Scan image with Trivy
trivy image myregistry.azurecr.io/myapp:latest

# Build with security scanning in Azure DevOps
# Add to azure-pipelines.yml:
- task: AzureSecurityDevOps.ms-securitydevops-code-scanning-task-codeql.init@0
  displayName: Initialize CodeQL
  inputs:
    languages: 'csharp'

- task: AzureSecurityDevOps.ms-securitydevops-code-scanning-task-codeql.finalize@0
  displayName: Finalize CodeQL
```

---

## 6.2 Secrets Management

**Azure Key Vault Integration:**

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add Azure Key Vault
var keyVaultUrl = new Uri($"https://{builder.Configuration["KeyVault:VaultName"]}.vault.azure.net/");
var credential = new DefaultAzureCredential();
builder.Configuration.AddAzureKeyVault(keyVaultUrl, credential);

// Access secrets
var dbPassword = builder.Configuration["DatabasePassword"];
var apiKey = builder.Configuration["ExternalApiKey"];

var app = builder.Build();
app.Run();
```

**Kubernetes Secrets:**

```bash
# Create secret from literal
kubectl create secret generic app-secrets \
  --from-literal=db-password=YourSecurePassword \
  --from-literal=api-key=YourApiKey \
  -n myapp

# Create secret from file
kubectl create secret generic app-cert \
  --from-file=cert.pem \
  -n myapp

# Reference in deployment
env:
  - name: DB_PASSWORD
    valueFrom:
      secretKeyRef:
        name: app-secrets
        key: db-password
```

---

## 6.3 Deployment Checklist

**Pre-Deployment:**
- [ ] Run security scan on Docker image (Trivy, Snyk)
- [ ] Review code with SAST tools (SonarQube)
- [ ] Run dependency check (OWASP)
- [ ] Load test the application
- [ ] Test disaster recovery procedures
- [ ] Verify all secrets in Key Vault
- [ ] Get approval from security team

**Deployment:**
- [ ] Use blue-green or canary deployment strategy
- [ ] Monitor health checks and metrics
- [ ] Keep rollback plan ready
- [ ] Document deployment in runbook
- [ ] Notify stakeholders

**Post-Deployment:**
- [ ] Verify application is healthy
- [ ] Check logs for errors
- [ ] Monitor performance metrics
- [ ] Run smoke tests
- [ ] Document any issues

---

## End of Deployment Guide

**Complete Coverage:**
✅ Docker fundamentals & Dockerfiles
✅ Monolithic deployment with Azure DevOps
✅ Microservices deployment with Kubernetes
✅ Container orchestration (K8s manifests)
✅ Monitoring & logging (Application Insights, ELK)
✅ Security best practices

---

This guide is production-ready for deploying enterprise applications on Azure using Docker and Kubernetes.
