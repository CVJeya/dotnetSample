# 5. Cloud & DevOps

Learn **one cloud deeply (Azure, since your stack is .NET)** and know the **AWS equivalent** by name. Leads are expected to speak both, but depth in one.

---

## 5.1 Azure (deep)

| Service | Use it for | Lead-level nuance |
|---|---|---|
| App Service | Simple web hosting, slots for blue-green | Slots + staging swap = cheapest blue-green |
| Container Apps | Containers without K8s ops, scale-to-zero, Dapr | Best default for most teams over AKS |
| AKS | Full Kubernetes control, complex workloads | You now own upgrades, node pools, ingress, CSI |
| Azure Functions | Event-driven, serverless, timers, queues | Cold start, consumption vs premium plan |
| Key Vault | Secrets, certs, keys | Managed identity access; no secrets in config |
| API Management | Gateway, rate limiting, auth, versioning | Smart endpoints, dumb pipes — keep logic out |
| Entra ID | Identity for users + workloads | App roles, groups→claims, managed identity |
| Service Bus / Event Grid / Event Hubs | Messaging / events / streaming | Pick per pattern (queue vs pub-sub vs stream) |
| App Insights / Azure Monitor | Logs, traces, metrics, alerts | Workspace-based, sampling tuning |
| SQL Database / Cosmos DB / Blob | Data stores | DTU vs vCore; partition key design |

**Exercise:** deploy one of your services to **Container Apps** with managed identity reading Key Vault, App Insights wired, and a staging slot / revision for blue-green.

## 5.2 AWS (breadth)

| AWS | Azure equivalent | Notes |
|---|---|---|
| EC2 / ECS Fargate / EKS | VMs / Container Apps / AKS | Fargate = serverless containers |
| Lambda | Azure Functions | 15-min limit, event sources |
| S3 / RDS / DynamoDB | Blob / SQL DB / Cosmos DB | DynamoDB single-table design is the skill |
| IAM / STS / Cognito | Entra ID / managed identity | Least privilege, roles over users |
| SQS / SNS / EventBridge / Kinesis | Service Bus / Event Grid / Event Hubs | SQS no replay; FIFO ordering per group |
| CloudWatch / X-Ray | Azure Monitor / App Insights | OTel exporter works with both |
| CloudFormation / CDK | ARM / Bicep | CDK = code (TS/Python) |

**Exercise:** deploy the *same* service to **ECS Fargate** (or Lambda for the API) with IAM least-privilege roles and CloudWatch; compare cost + DX with Azure in a short write-up.

## 5.3 Containers & Kubernetes

- **Docker:** multi-stage builds (SDK → runtime), small base images (chiseled/distroless), non-root user, `.dockerignore`, layer caching, healthcheck.
- **K8s:** Deployment/Service/Ingress, ConfigMaps/Secrets, readiness vs liveness vs startup probes, requests/limits, HPA, PDB, rolling updates, `kubectl`/`helm`. Understand *what happens from `kubectl apply` to a running pod*: scheduling → image pull → containers start → probes pass → endpoints registered.
- **Exercise:** multi-stage Dockerfile for your API (< ~100 MB final image, non-root), deploy to AKS/Container Apps with probes + resource limits + HPA; trigger a rolling update with zero downtime.

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.sln ./
COPY LHDN_STSDS/*.csproj LHDN_STSDS/
RUN dotnet restore
COPY . .
RUN dotnet publish LHDN_STSDS/*.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled AS runtime
WORKDIR /app
USER $APP_UID
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "LHDN_STSDS.dll"]
```

## 5.4 CI/CD

- **Pipeline anatomy:** restore → build → test (unit + integration) → static analysis → package → scan → deploy (per environment with approvals) → smoke test → promote.
- **Build once, deploy many** (identical artifact per environment); config via environment, not rebuild.
- **Azure DevOps Pipelines** (your repo already has `.gitlab-ci.yml`, but ADO/GitHub are the targets) and **GitHub Actions**; environments + approvals; secrets from Key Vault / OIDC federated credentials (no stored secrets).
- **Deploy strategies:**
  - **Rolling** — default, zero-downtime, needs N-1 compatibility.
  - **Blue-green** — swap staging/prod, instant rollback, 2× cost.
  - **Canary** — route a % of traffic, watch SLOs, auto-rollback.
  - **Feature flags** — decouple deploy from release (most agile).
- **Release readiness:** DB migrations backward-compatible (expand/contract), versioning, rollback plan, kill switch.
- **Exercise:** pipeline that builds once, runs tests, runs SAST + container scan, deploys to staging, waits for approval, then **blue-green swaps** to prod. Prove rollback works.

## 5.5 IaC

- **Terraform:** HCL, state (remote backend + locking), modules, plan/apply, drift. Cloud-agnostic, industry default.
- **Bicep / ARM:** native Azure, no state file, great with ADO.
- **Rule:** infra is code in the repo, reviewed in PRs, applied by the pipeline (never `apply` from a laptop to prod).
- **Exercise:** `terraform` OR `bicep` that provisions the app (resource group, container app/app service, Key Vault, App Insights, SQL), parameterised per environment. Destroy and recreate from scratch to prove it works.

## 5.6 DevSecOps

- **SAST** (static): GitHub CodeQL / SonarQube.
- **DAST** (dynamic): OWASP ZAP against staging.
- **Dependency scanning:** `dotnet list package --vulnerable`, Dependabot/Renovate, Snyk.
- **Container scanning:** Trivy/Grype; sign images (cosign).
- **Secrets:** `gitleaks` in pre-commit + CI; Key Vault at runtime.
- **Supply chain:** SBOM (CycloneDX), pin versions, signed artifacts, least-privilege pipeline identity (OIDC federation, no long-lived creds).
- **Exercise:** add CodeQL + Trivy + dependency scan + gitleaks to the pipeline, make them **blocking** on high severity, and document the exception process.

---

## Cost & FinOps (a lead is accountable here)

- Right-size before you scale; use scale-to-zero (Container Apps/Functions); reserved instances/savings plans for steady load; set budget alerts; tag resources (owner, env, cost-centre); review monthly.
- **Exercise:** estimate monthly cost of your capstone on both clouds by hand before deploying, then compare to the actual bill. Write down the delta and why.
