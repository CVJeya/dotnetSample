# Docker & Azure DevOps Deployment Guide
## Complete Setup Instructions for Three-Tier Microservices

**Intended for:** Developers deploying this application from scratch using Docker and Azure DevOps.

---

## Table of Contents

1. [Overview & Architecture](#overview--architecture)
2. [Docker Setup (Local Testing)](#docker-setup-local-testing)
3. [Azure Account Setup](#azure-account-setup)
4. [Azure DevOps Pipeline Configuration](#azure-devops-pipeline-configuration)
5. [Deployment Options](#deployment-options)
6. [Troubleshooting](#troubleshooting)
7. [Quick Reference](#quick-reference)

---

## Overview & Architecture

### What is Docker?

Docker is **containerization technology**. Think of it like shipping containers:
- **Without containers:** Each application runs differently on each machine (Windows, Mac, Linux)
- **With containers:** Each application runs identically in a self-contained "container" with all dependencies included

### What is Azure?

Azure is **Microsoft's cloud platform**. You can:
- Store code in Azure DevOps repositories
- Run automated pipelines (CI/CD) to build and test code
- Deploy applications to Azure cloud services
- Monitor and manage applications

### Three-Tier Microservices Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                    Docker Network (Bridge)                    │
│                                                               │
│  ┌─────────────────┐  ┌──────────────────┐  ┌────────────┐  │
│  │  backend        │  │  angular-app     │  │ react-app  │  │
│  │  (Port 8080)    │  │  (Port 8081)     │  │(Port 8082) │  │
│  │                 │  │                  │  │            │  │
│  │  ASP.NET Core   │  │  Nginx + SPA     │  │ Nginx+SPA  │  │
│  │  (.NET 10.0)    │  │  (Angular 22)    │  │ (React 19) │  │
│  └─────────────────┘  └──────────────────┘  └────────────┘  │
│         ▲                     │                     │         │
│         │                     └─────────────────────┘         │
│         └──────────────────── /api/                           │
│                                                               │
└──────────────────────────────────────────────────────────────┘
         ↑
    Docker Host (Your machine or Azure)
```

**Service Communication:**
- Browser requests Angular at `localhost:8081`
- Angular's nginx proxies `/api/*` requests to `backend:8080` (using Docker service discovery)
- Backend processes the request and returns data
- Nginx relays response back to browser

**Why this architecture?**
- **Isolation:** Each service runs independently, can be updated separately
- **Scalability:** Can run multiple instances of any service
- **Consistency:** Same containers run locally and in cloud
- **Deployment:** Simple `docker-compose` command deploys entire stack

---

## Docker Setup (Local Testing)

### Prerequisites

**Install these first:**
1. **Docker Desktop** (Windows/Mac) or **Docker Engine** (Linux)
   - Download: https://www.docker.com/products/docker-desktop
   - Includes: Docker + Docker Compose
   - Test installation: `docker --version`

2. **Docker Compose** (included with Docker Desktop)
   - Test: `docker compose version`

3. **.NET 10 SDK** (optional, only if modifying backend)
   - Download: https://dotnet.microsoft.com/download
   - Test: `dotnet --version`

4. **Node.js 24+** (optional, only if modifying frontends)
   - Download: https://nodejs.org/
   - Test: `npm --version`

### Project Files Structure

```
AngularDotnetDemo/
├── docker-compose.yml          ← Orchestrates all services
├── azure-pipelines.yml         ← CI/CD pipeline definition
├── DOCKER_AZURE_DEPLOYMENT_GUIDE.md  ← This file
│
├── backend/AngularDotnetDemo.Api/
│   ├── Dockerfile              ← Builds .NET API container
│   ├── Program.cs              ← API code (updated CORS)
│   ├── appsettings.json        ← Configuration (JWT settings)
│   └── ... other files
│
├── frontend/
│   ├── Dockerfile              ← Builds Angular SPA container
│   ├── nginx.conf              ← Reverse proxy config (API routing)
│   ├── package.json
│   ├── src/                    ← Angular source code
│   └── dist/frontend/          ← Built output (created by docker build)
│
└── ReactDotNetDemo/
    ├── Dockerfile              ← Builds React SPA container
    ├── nginx.conf              ← Reverse proxy config (API routing)
    ├── package.json
    ├── src/                    ← React source code
    └── dist/                   ← Built output (created by docker build)
```

### Step 1: Build All Containers Locally

Run this command from the **project root** directory:

```bash
cd AngularDotnetDemo
docker compose up --build
```

**What happens:**
1. Docker reads `docker-compose.yml`
2. Creates a Docker network called `app-network`
3. Builds 3 images:
   - `dotnet-backend` from `backend/Dockerfile`
   - `angular-frontend` from `frontend/Dockerfile`
   - `react-frontend` from `ReactDotNetDemo/Dockerfile`
4. Starts 3 containers from those images
5. Streams logs to your terminal

**Expected output (takes 2-3 minutes first time):**
```
[+] Building 120.3s (24/24) FINISHED
 => dotnet-backend: build complete
 => angular-frontend: build complete
 => react-frontend: build complete
[+] Running 3/3
 ✔ dotnet-backend Started
 ✔ angular-frontend Started
 ✔ react-frontend Started
```

### Step 2: Test the Application

Open your browser and visit:

**Angular Frontend:**
```
http://localhost:8081
```
- See: Login form
- Try: Register a new user, then add a product

**React Frontend:**
```
http://localhost:8082
```
- Same functionality as Angular frontend
- Different UI (React instead of Angular)

**Direct API:**
```
http://localhost:8080/api/users
```
- Should return list of users (no auth required)

### Step 3: Stop the Containers

**In the terminal where docker compose is running:**
```
Ctrl+C
```

**Or in another terminal:**
```bash
docker compose down
```

This stops and removes all containers.

### Step 4: View Running Containers

```bash
docker ps
```

Shows all running containers with their ports, names, status.

### Useful Docker Commands

```bash
# List all containers (running + stopped)
docker ps -a

# View logs from a specific service
docker compose logs backend
docker compose logs angular-app
docker compose logs react-app

# View logs in real-time
docker compose logs -f backend

# Rebuild and restart without stopping
docker compose up --build

# Remove all containers and networks
docker compose down

# Remove containers + images + volumes
docker compose down -v

# Execute command inside running container
docker compose exec backend dotnet --version

# Build just one image
docker build -t dotnet-backend ./backend/AngularDotnetDemo.Api

# View image details
docker image ls
docker image inspect dotnet-backend
```

---

## Azure Account Setup

### Step 1: Create Azure Account

1. Go to https://portal.azure.com
2. Click "Create a resource"
3. Fill in account details (requires credit card, but you get $200 free credits)

### Step 2: Create Azure Container Registry (ACR)

The **Azure Container Registry** stores your Docker images in the cloud.

**Via Azure Portal:**
1. Search for "Container Registry" in the portal
2. Click "Create"
3. Fill in details:
   - **Resource Group:** Create new (e.g., `my-app-resources`)
   - **Registry name:** Must be unique (e.g., `myappregistry123`)
   - **SKU:** Select "Basic" (cheapest)
   - **Location:** Choose closest to you (e.g., `East US`)
4. Click "Create" (takes ~1 minute)

**Via Azure CLI (faster):**
```bash
# Install Azure CLI: https://learn.microsoft.com/en-us/cli/azure/install-azure-cli

az login  # Opens browser for login

az group create --name myapp-rg --location eastus

az acr create --resource-group myapp-rg \
  --name myappregistry123 \
  --sku Basic
```

**Note the Registry URL:** `myappregistry123.azurecr.io` (you'll need this later)

### Step 3: Create Azure DevOps Project

1. Go to https://dev.azure.com
2. Click "Create project"
3. Fill in:
   - **Project name:** `AngularDotnetDemo`
   - **Visibility:** Public (free) or Private
4. Click "Create"

### Step 4: Connect DevOps to GitHub / Local Repo

**Option A: Use GitHub (Recommended)**

1. In your Azure DevOps project, go to "Repos"
2. Click "Import a repository"
3. Paste GitHub URL of your project
4. Click "Import"

**Option B: Push to Azure Repos (Alternative)**

```bash
# Clone the Azure DevOps repo to your machine
git clone https://dev.azure.com/[YOUR_ORG]/[PROJECT]/_git/[REPO]
cd [REPO]

# Copy your project files here
# (Copy all files from AngularDotnetDemo folder)

git add .
git commit -m "Initial commit: Full stack app with Docker"
git push
```

### Step 5: Create Service Connection (Allows Pipeline to Push to ACR)

This gives the DevOps pipeline permission to push images to your container registry.

**In Azure DevOps:**
1. Project settings (bottom left)
2. Service connections
3. "Create service connection"
4. Select "Docker Registry"
5. Choose "Azure Container Registry"
6. Fill in:
   - **Azure subscription:** (select your subscription)
   - **Container registry:** (select the ACR you created)
   - **Service connection name:** `AzureContainerRegistry`
7. Click "Save"

**Note the connection name:** You'll use this in the pipeline.

---

## Azure DevOps Pipeline Configuration

### What is a Pipeline?

A **pipeline** is an automated workflow that:
1. **Builds** your code (compiles, runs tests)
2. **Packages** it into containers
3. **Pushes** to a registry
4. **Deploys** to the cloud

### How the Pipeline Works

```
┌─────────────────────────────────────────────┐
│  You push code to repo                      │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│  Pipeline triggers automatically            │
│  (azure-pipelines.yml is detected)          │
└──────────────────┬──────────────────────────┘
                   │
         ┌─────────┴─────────┐
         │                   │
    ┌────▼────┐        ┌─────▼────┐
    │  Build  │        │  Push    │
    │ Images  │───────▶│  to ACR  │
    └─────────┘        └─────┬────┘
                              │
                    ┌─────────▼──────────┐
                    │  Deploy (Optional) │
                    └────────────────────┘
```

### Step 1: Update Pipeline Configuration

Edit **`azure-pipelines.yml`** in your project root.

**Find these lines:**
```yaml
variables:
  dockerRegistryServiceConnection: ''
  containerRegistry: ''
```

**Replace with your values:**
```yaml
variables:
  dockerRegistryServiceConnection: 'AzureContainerRegistry'
  containerRegistry: 'myappregistry123.azurecr.io'
```

**Explanation:**
- `dockerRegistryServiceConnection`: The service connection you created (e.g., `AzureContainerRegistry`)
- `containerRegistry`: Your ACR URL (e.g., `myappregistry123.azurecr.io`)

### Step 2: Create Pipeline in Azure DevOps

**In Azure DevOps:**
1. Go to "Pipelines" (left sidebar)
2. Click "Create Pipeline"
3. Select "GitHub" or "Azure Repos" (where your code is)
4. Select your repository
5. Select "Existing Azure Pipelines YAML"
6. Select the file: `azure-pipelines.yml`
7. Click "Continue"
8. Click "Save and run"

**The pipeline will:**
- Trigger on code push to `main`, `master`, or `develop`
- Build 3 Docker images
- Push them to your ACR with tags: `<build_id>` and `latest`

### Step 3: Monitor Pipeline Execution

**In Azure DevOps:**
1. Go to "Pipelines"
2. Click your pipeline name
3. You'll see a list of "Runs" (each push triggers a run)

**For each run, you can see:**
- **Build logs** — what commands ran
- **Stage status** — Build ✓, Deploy (optional)
- **Artifact links** — images pushed to ACR

**Example build output:**
```
docker build -t myappregistry123.azurecr.io/dotnet-backend:123 \
  ./backend/AngularDotnetDemo.Api
docker build -t myappregistry123.azurecr.io/angular-frontend:123 \
  ./frontend
docker build -t myappregistry123.azurecr.io/react-frontend:123 \
  ./ReactDotNetDemo
docker push myappregistry123.azurecr.io/dotnet-backend:123
docker push myappregistry123.azurecr.io/angular-frontend:123
docker push myappregistry123.azurecr.io/react-frontend:123
```

### Step 4: Verify Images in ACR

**In Azure Portal:**
1. Search for "Container Registries"
2. Click your registry (`myappregistry123`)
3. Go to "Repositories"
4. You should see:
   - `dotnet-backend`
   - `angular-frontend`
   - `react-frontend`

Each repository has tags like `123` (build ID) and `latest`.

---

## Deployment Options

### Option A: Deploy to Azure Container Instances (ACI)

**Best for:** Simple, single-instance deployments (dev/demo).

**Prerequisites:**
- ACR with images already pushed ✓

**Steps:**

```bash
# Set variables
ACR_NAME="myappregistry123"
ACR_URL="$ACR_NAME.azurecr.io"
RESOURCE_GROUP="myapp-rg"
IMAGE_TAG="latest"

# Create resource group (if not already created)
az group create --name $RESOURCE_GROUP --location eastus

# Get ACR credentials
az acr credential show --name $ACR_NAME --query "[username, passwords[0].value]"
# Save these: username and password (you'll need them)

# Deploy backend
az container create \
  --resource-group $RESOURCE_GROUP \
  --name dotnet-backend \
  --image $ACR_URL/dotnet-backend:$IMAGE_TAG \
  --cpu 1 --memory 1 \
  --ports 8080 \
  --registry-login-server $ACR_URL \
  --registry-username <YOUR_USERNAME> \
  --registry-password <YOUR_PASSWORD>

# Deploy Angular frontend
az container create \
  --resource-group $RESOURCE_GROUP \
  --name angular-frontend \
  --image $ACR_URL/angular-frontend:$IMAGE_TAG \
  --cpu 1 --memory 1 \
  --ports 80 \
  --registry-login-server $ACR_URL \
  --registry-username <YOUR_USERNAME> \
  --registry-password <YOUR_PASSWORD>

# Deploy React frontend
az container create \
  --resource-group $RESOURCE_GROUP \
  --name react-frontend \
  --image $ACR_URL/react-frontend:$IMAGE_TAG \
  --cpu 1 --memory 1 \
  --ports 80 \
  --registry-login-server $ACR_URL \
  --registry-username <YOUR_USERNAME> \
  --registry-password <YOUR_PASSWORD>
```

**To see running containers:**
```bash
az container list --resource-group $RESOURCE_GROUP -o table
```

**To get the public IP:**
```bash
az container show --resource-group $RESOURCE_GROUP \
  --name dotnet-backend --query ipAddress.ip
```

**Limitations:**
- ❌ Containers must be on different machines (no internal Docker network)
- ❌ Need public IPs for each service
- ❌ Harder for frontends to call backend

### Option B: Deploy to Azure App Service

**Best for:** Production-grade web applications.

**Setup (via Portal):**
1. Create **App Service Plan** (compute resources)
2. Create **Web App** (actual application)
3. Configure container settings
4. Deploy

**Steps:**

```bash
# Create App Service Plan
az appservice plan create \
  --name myapp-plan \
  --resource-group myapp-rg \
  --is-linux \
  --sku B1  # Basic tier (cheapest)

# Create Web App for backend
az webapp create \
  --resource-group myapp-rg \
  --plan myapp-plan \
  --name myapp-api \
  --deployment-container-image-name-user "<USERNAME>" \
  --deployment-container-image-name-password "<PASSWORD>"

# Configure container for backend
az webapp config container set \
  --name myapp-api \
  --resource-group myapp-rg \
  --docker-custom-image-name myappregistry123.azurecr.io/dotnet-backend:latest \
  --docker-registry-server-url https://myappregistry123.azurecr.io \
  --docker-registry-server-user "<USERNAME>" \
  --docker-registry-server-password "<PASSWORD>"
```

**Access your app:**
```
https://myapp-api.azurewebsites.net/api/users
```

### Option C: Deploy to Azure Kubernetes Service (AKS)

**Best for:** Large-scale, high-availability applications.

**Complexity:** High (requires Kubernetes knowledge)

**Why:** Auto-scaling, load balancing, rolling updates, self-healing

**Setup (simplified):**
```bash
# Create AKS cluster
az aks create \
  --resource-group myapp-rg \
  --name myapp-cluster \
  --node-count 3 \
  --attach-acr myappregistry123 \
  --enable-managed-identity

# Get credentials
az aks get-credentials --resource-group myapp-rg --name myapp-cluster

# Deploy using kubectl
kubectl apply -f k8s/deployment.yaml
```

**Note:** Requires creating Kubernetes manifests (beyond this guide).

### Option D: Continuous Deployment via Pipeline

**Automate deployment to ACI/App Service when code is pushed.**

**Update `azure-pipelines.yml`** to include a Deploy stage:

```yaml
- stage: Deploy
  displayName: Deploy to Azure App Service
  dependsOn: Build
  condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))
  
  jobs:
  - deployment: DeployBackend
    displayName: Deploy Backend
    environment: production
    strategy:
      runOnce:
        deploy:
          steps:
          - task: AzureWebAppContainer@1
            inputs:
              azureSubscription: '<SERVICE_CONNECTION_NAME>'
              appName: 'myapp-api'
              imageName: '$(containerRegistry)/dotnet-backend:$(Build.BuildId)'
```

**When you push to `main`:**
1. Pipeline builds images
2. Pipeline pushes to ACR
3. Pipeline automatically deploys to Azure App Service

---

## Environment Variables & Secrets

### Why Environment Variables?

Your code has **hardcoded secrets** in `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "ThisIsAVeryLongSecretKeyForDevelopmentPurposesOnly123!",
    "Issuer": "AngularDotnetDemo",
    "Audience": "AngularDotnetDemoClient"
  }
}
```

**Problems:**
- ❌ Anyone who clones the repo sees the secret
- ❌ Same secret in dev, staging, and production
- ❌ Can't rotate secret without code change

**Solution:** Use environment variables

### Step 1: Update Program.cs to Support Environment Variables

The current code reads from `appsettings.json`. To use environment variables, update `Program.cs`:

```csharp
var jwtKey = builder.Configuration["Jwt:Key"] 
  ?? Environment.GetEnvironmentVariable("JWT_KEY") 
  ?? throw new InvalidOperationException("JWT_KEY not configured");
```

**Now the app checks:**
1. `appsettings.json` (development)
2. Environment variable `JWT_KEY` (production)
3. Throws error if neither exists

### Step 2: Set Environment Variables in Docker

**In `backend/Dockerfile`:**

```dockerfile
# ... (existing content)

ENV ASPNETCORE_ENVIRONMENT=Production
ENV Jwt__Key=$JWT_KEY
ENV Jwt__Issuer=$JWT_ISSUER
ENV Jwt__Audience=$JWT_AUDIENCE

ENTRYPOINT ["dotnet", "AngularDotnetDemo.Api.dll"]
```

### Step 3: Set Environment Variables in Docker Compose

**In `docker-compose.yml`:**

```yaml
services:
  backend:
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - Jwt__Key=ThisIsAVeryLongSecretKeyForDevelopmentPurposesOnly123!
      - Jwt__Issuer=AngularDotnetDemo
      - Jwt__Audience=AngularDotnetDemoClient
```

### Step 4: Set Environment Variables in Azure Pipeline

**Never hardcode secrets in `azure-pipelines.yml`!**

Instead, use Azure Key Vault:

1. **Create Azure Key Vault:**
   ```bash
   az keyvault create --name myapp-vault --resource-group myapp-rg
   ```

2. **Add secrets to vault:**
   ```bash
   az keyvault secret set --vault-name myapp-vault \
     --name JwtKey \
     --value "your-secret-key-here"
   ```

3. **Update pipeline to fetch from vault:**
   ```yaml
   steps:
   - task: AzureKeyVault@1
     inputs:
       azureSubscription: 'YOUR_SUBSCRIPTION'
       KeyVaultName: 'myapp-vault'
       SecretsFilter: 'JwtKey,JwtIssuer,JwtAudience'
       RunAsPreJob: true
   
   - task: Docker@2
     inputs:
       buildArguments: |
         --build-arg JWT_KEY=$(JwtKey)
         --build-arg JWT_ISSUER=$(JwtIssuer)
         --build-arg JWT_AUDIENCE=$(JwtAudience)
   ```

### Step 5: Update Dockerfile to Accept Build Arguments

```dockerfile
ARG JWT_KEY
ARG JWT_ISSUER
ARG JWT_AUDIENCE

ENV Jwt__Key=$JWT_KEY
ENV Jwt__Issuer=$JWT_ISSUER
ENV Jwt__Audience=$JWT_AUDIENCE
```

---

## Troubleshooting

### Issue 1: Docker Build Fails - "Cannot find module"

**Symptom:**
```
npm ERR! code ENOENT
npm ERR! syscall open
npm ERR! path /app/package.json
```

**Cause:** Missing `package-lock.json` or incorrect COPY paths

**Fix:**
```dockerfile
# Make sure both files exist in your project root
COPY package.json package-lock.json ./
RUN npm ci
```

### Issue 2: Backend Container Starts but Crashes

**Symptom:**
```
docker compose logs backend
error: Unable to start application. Failed to load configuration.
```

**Cause:** Missing `appsettings.json` or JWT secrets not configured

**Fix:**
1. Verify `appsettings.json` exists in backend folder
2. Check environment variables are set in `docker-compose.yml`:
   ```yaml
   environment:
     - Jwt__Key=your-secret-here
   ```

### Issue 3: Frontends Can't Call Backend API

**Symptom:**
```
error: Failed to fetch http://backend:8080/api/users
ERR_NAME_NOT_RESOLVED
```

**Cause:** Services are not on the same Docker network

**Fix:** Verify `docker-compose.yml` has:
```yaml
networks:
  app-network:
    driver: bridge

services:
  backend:
    networks:
      - app-network
  angular-app:
    networks:
      - app-network
```

### Issue 4: Pipeline Fails to Push to ACR

**Symptom:**
```
Error: Failed to push image to registry
unauthorized: authentication required
```

**Cause:** Service connection not properly configured

**Fix:**
1. In Azure DevOps, go to Project Settings → Service connections
2. Verify "AzureContainerRegistry" connection exists
3. Click it and verify credentials are correct
4. Update `azure-pipelines.yml`:
   ```yaml
   containerRegistry: $(containerRegistry)  # Must be defined
   dockerRegistryServiceConnection: 'AzureContainerRegistry'
   ```

### Issue 5: ACR Images are Huge (500MB+)

**Symptom:**
```
Successfully pushed image: 562MB
```

**Cause:** Docker layer caching not optimized

**Fix:** Use multi-stage builds (already in Dockerfiles):
```dockerfile
# Build stage
FROM node:24-alpine AS build
RUN npm ci && npm run build

# Runtime stage (only copy output)
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
```

This removes build tools from final image, reducing size by 70%.

### Issue 6: Nginx Returns 404 for SPA Routes

**Symptom:**
```
GET /products returns 404
GET /products/ returns 404
```

**Cause:** Nginx doesn't know to fallback to `index.html`

**Fix:** Verify `nginx.conf` has:
```nginx
location / {
  try_files $uri $uri/ /index.html;
}
```

This tells nginx to serve `index.html` for any route that isn't a file.

### Issue 7: CORS Error in Browser

**Symptom:**
```
Access to XMLHttpRequest from origin 'http://localhost:8081'
has been blocked by CORS policy
```

**Cause:** Backend CORS policy is too restrictive

**Fix:** Update `Program.cs`:
```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

**Note:** This is OK for dev. For production, be more restrictive:
```csharp
policy.WithOrigins("https://myapp.azurewebsites.net")
      .AllowAnyMethod()
      .AllowAnyHeader();
```

---

## Quick Reference

### Local Development

```bash
# Start all services
docker compose up --build

# View logs
docker compose logs -f

# Stop services
docker compose down

# Rebuild one service
docker compose build backend
```

### Azure DevOps

```bash
# View pipeline logs
# Azure DevOps UI → Pipelines → Select your pipeline → Click run number

# Manually trigger pipeline
# Azure DevOps UI → Pipelines → Run pipeline

# View images in ACR
az acr repository list --name myappregistry123
az acr repository show-tags --name myappregistry123 --repository dotnet-backend
```

### Azure Deployment

```bash
# View all containers
az container list --resource-group myapp-rg -o table

# View container logs
az container logs --resource-group myapp-rg --name dotnet-backend

# Delete container
az container delete --resource-group myapp-rg --name dotnet-backend

# View Web App logs
az webapp log tail --name myapp-api --resource-group myapp-rg
```

### Key Files Reference

| File | Purpose | Edit When |
|------|---------|-----------|
| `docker-compose.yml` | Local orchestration | Adding services, changing ports, env vars |
| `azure-pipelines.yml` | CI/CD pipeline | Changing build steps, deployment targets |
| `backend/Dockerfile` | .NET image build | Changing .NET version, adding dependencies |
| `frontend/Dockerfile` | Angular image build | Changing Node version, build commands |
| `frontend/nginx.conf` | Angular routing & proxy | Changing API endpoint, adding new routes |
| `backend/Program.cs` | API code & config | Adding endpoints, changing CORS, env vars |
| `backend/appsettings.json` | JWT secrets (dev only) | Rotating JWT key, changing issuer |

---

## Checklist for First Deployment

### Before You Start
- [ ] Azure account created
- [ ] Azure Container Registry created
- [ ] Azure DevOps project created
- [ ] Code pushed to GitHub/Azure Repos
- [ ] Docker Desktop installed locally

### Local Testing
- [ ] `docker compose up --build` succeeds
- [ ] Can access Angular at `localhost:8081`
- [ ] Can access React at `localhost:8082`
- [ ] Can register user and add product
- [ ] `docker compose down` removes containers cleanly

### Azure Setup
- [ ] Container Registry created
- [ ] Service connection created in DevOps
- [ ] `azure-pipelines.yml` variables updated
- [ ] Pipeline created in Azure DevOps

### First Pipeline Run
- [ ] Pipeline triggers on code push
- [ ] Build stage completes ✓
- [ ] Images appear in ACR ✓
- [ ] Can manually test images locally with updated `docker-compose.yml`

### Deployment
- [ ] Choose deployment target (ACI, App Service, or AKS)
- [ ] Create deployment resources in Azure
- [ ] Test deployed application
- [ ] Set up monitoring & logging

---

## Security Best Practices

### Secrets Management

**❌ DON'T:**
```json
{
  "Jwt": {
    "Key": "my-secret-key"
  }
}
```

**✓ DO:**
```bash
az keyvault secret set --vault-name myapp-vault --name JwtKey --value "my-secret-key"
```

### CORS Policy

**❌ DON'T (in production):**
```csharp
policy.AllowAnyOrigin()
```

**✓ DO (in production):**
```csharp
policy.WithOrigins("https://myapp.example.com",
                  "https://api.example.com")
```

### Image Tagging

**❌ DON'T:**
```bash
docker build -t myapp:latest .
```

**✓ DO:**
```bash
docker build -t myapp:$(date +%Y%m%d-%H%M%S) .
docker build -t myapp:1.0.0 .
```

### Registry Access

**❌ DON'T:**
```bash
az acr credential show  # Shows passwords in plain text
```

**✓ DO:**
```bash
# Use managed identities in AKS/App Service
# Use Azure Key Vault for secrets
# Use Azure Container Registry Tasks (ACR Tasks)
```

---

## Next Steps

1. **Complete the checklist above**
2. **Deploy to Azure**
3. **Set up monitoring:**
   - Application Insights
   - Log Analytics
   - Alerts
4. **Configure CI/CD automation:**
   - Auto-deploy on successful build
   - Run tests in pipeline
   - Add code quality checks
5. **Scale the application:**
   - Multiple replicas
   - Load balancer
   - Auto-scaling based on demand

---

## Getting Help

| Issue | Resource |
|-------|----------|
| Docker problems | https://docs.docker.com |
| Azure DevOps | https://learn.microsoft.com/en-us/azure/devops |
| Azure Container Registry | https://learn.microsoft.com/en-us/azure/container-registry |
| ASP.NET configuration | https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration |
| Nginx configuration | https://nginx.org/en/docs/ |

---

## Summary

You now have:

1. **Three Docker images** (backend API + 2 frontends)
2. **Automated CI/CD pipeline** (build & push on every commit)
3. **Secure secret management** (Azure Key Vault)
4. **Multiple deployment options** (ACI, App Service, AKS)
5. **Production-ready architecture** (containerized microservices)

Deploy with confidence! 🚀
