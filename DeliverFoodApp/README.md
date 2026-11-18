# DeliverFoodApp — Complete Food Delivery Microservices

A production-ready ASP.NET Core application demonstrating **all 12 communication scenarios** from microservices architecture:

- **REST APIs** for queries (search, stock check)
- **MassTransit + RabbitMQ** for async processing (payment, inventory, email)
- **gRPC** for internal high-performance queries
- **Kafka** for analytics streaming (optional)
- **SignalR** for real-time notifications
- **Saga pattern** for distributed transactions with auto-compensation
- **Docker** for local development and containerization
- **Azure DevOps** CI/CD pipeline for cloud deployment

---

## Architecture Overview

```
                    Frontend (React/Angular)
                            ↓
                    ┌──────────────────┐
                    │  nginx (80)      │  Reverse proxy, SPA serving
                    └────────┬─────────┘
                             ↓
                    ┌──────────────────┐
                    │ order-api (8080) │  REST + gRPC + SignalR
                    │ & OrderHub       │
                    └────────┬─────────┘
                             ↓
            ┌────────────────┼────────────────┐
            ↓                ↓                ↓
      ┌──────────┐     ┌──────────┐    ┌──────────┐
      │RabbitMQ  │     │  Kafka   │    │SQL Server│
      │ (AMQP)   │     │(Analytics)   │(Database)│
      └────┬─────┘     └──────────┘    └──────────┘
           ↓
    ┌────────────────────┐
    │ order-consumers    │  Payment, Inventory, Email
    │ (×2 replicas)      │  Partner Integration, Notifications
    └────────────────────┘
```

---

## Scenarios Implemented

| # | Scenario | Pattern | Implementation |
|---|----------|---------|-----------------|
| **1** | Search products | REST query | `MenuController.Search()` |
| **2** | Place order | REST 202 + broker | `OrderController.PlaceOrder()` publishes `OrderCreatedEvent` |
| **3** | Send email | Broker async | `EmailConsumer` — decoupled, fire-and-forget |
| **4** | Check stock | REST query | `InventoryController.CheckStock()` |
| **5** | Payment processing | Broker + retry | `PaymentConsumer` — 3 retries, dead-letter queue |
| **6** | Notify 5 services | Pub/sub fanout | `OrderCreatedEvent` → 5 consumers in parallel |
| **7** | Order history | REST + gRPC | `OrderGrpcService` for internal, `OrderHistoryController` for external |
| **8** | Analytics sync | Kafka streaming | `AnalyticsProducer` → high-throughput event sink |
| **9** | Charge credit card | Broker + retry | `PaymentConsumer` — same as #5, retry with backoff |
| **10** | Real-time notifications | SignalR | `OrderHub` — WebSocket push to connected clients |
| **11** | External partner | REST (inside broker) | `PartnerIntegrationConsumer` — broker trigger, REST call |
| **12** | Distributed transaction | Saga | `OrderSaga` — auto-compensates if inventory fails |

---

## Quick Start (Local Development)

### 1. Prerequisites
- Docker & docker-compose installed
- .NET 8 SDK (if running locally without Docker)
- Git

### 2. Start Infrastructure

```bash
# Navigate to project root
cd DeliverFoodApp

# Start all services: SQL Server, RabbitMQ, Kafka
docker-compose up -d

# Wait for health checks (~30 seconds)
docker-compose ps
```

**What spins up:**
- **SQL Server** on port 1433 (user: `sa`, password: `YourStrong!Passw0rd`)
- **RabbitMQ** on port 5672 (AMQP) and 15672 (UI) (guest/guest)
- **Kafka** on port 9092 (via KRaft mode, no ZooKeeper)

### 3. Run the Application

**Option A: Docker (Recommended)**
```bash
# Build and run both API and Consumers
docker-compose up -d order-api order-consumers

# View logs
docker-compose logs -f order-api order-consumers
```

**Option B: Local (Development)**

Terminal 1 — Run the API:
```bash
cd src/FoodDelivery.Api
dotnet run
```

Terminal 2 — Run Consumers:
```bash
cd src/FoodDelivery.Consumers
dotnet run
```

### 4. Test the Application

#### Health Check
```bash
curl -X GET http://localhost:8080/health
# Response: { "status": "Healthy" }
```

#### SCENARIO 1: Search Products
```bash
curl -X GET "http://localhost:8080/api/menu/search?query=pizza"
# Returns: Margherita Pizza, Pepperoni Pizza
```

#### SCENARIO 4: Check Stock
```bash
curl -X GET "http://localhost:8080/api/inventory/item-1/stock"
# Response: { "Available": true, "QuantityAvailable": 100 }
```

#### SCENARIO 2: Place Order (Triggers async processing)
```bash
curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "cust-001",
    "customerEmail": "user@example.com",
    "items": [
      {
        "menuItemId": "item-1",
        "name": "Margherita Pizza",
        "quantity": 2,
        "unitPrice": 12.99
      }
    ]
  }'

# Response: 202 Accepted (order published to broker, returns immediately)
# { "orderId": "guid", "status": "Pending" }
```

**What happens next (async):**
- Payment Consumer charges credit card (~3 seconds)
- Inventory Consumer reserves stock
- Email Consumer sends confirmation
- Partner Integration Consumer notifies delivery partner
- SignalR pushes real-time updates to customer browser
- Order status transitions: Pending → PaymentProcessed → InventoryReserved → Completed

#### SCENARIO 7: Get Order History (REST)
```bash
curl -X GET "http://localhost:8080/api/orders/history/cust-001"
# Returns: Array of 50 most recent orders
```

#### SCENARIO 7: Get Order History (gRPC — fast internal)
```bash
# Using grpcurl
grpcurl -plaintext -d '{"customer_id":"cust-001","limit":10}' \
  localhost:5001 OrderGrpc/GetOrderHistory
```

#### SCENARIO 10: Real-Time Notifications (SignalR)
```javascript
// Client-side (JavaScript)
const connection = new signalR.HubConnectionBuilder()
    .withUrl('http://localhost:8080/hubs/orders?userId=cust-001')
    .withAutomaticReconnect()
    .build();

connection.on('OrderStatusChanged', (update) => {
    console.log(`Order ${update.OrderId} status: ${update.Status}`);
    console.log(`Message: ${update.Message}`);
});

await connection.start();
```

---

## Monitoring & Debugging

### RabbitMQ Management UI
```
http://localhost:15672
User: guest
Password: guest
```

Check:
- Queues: `order-created`, `order-payment`, `order-inventory`
- Messages published vs consumed
- Dead-letter queues (DLQ) for failed messages

### Docker Logs

View API logs:
```bash
docker-compose logs -f order-api
```

View Consumer logs:
```bash
docker-compose logs -f order-consumers
```

### Database Inspection

Connect to SQL Server:
```bash
docker-compose exec sql-server sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd"

# List databases
SELECT name FROM sys.databases;

# View orders
USE FoodDelivery;
SELECT * FROM Orders;
```

---

## Project Structure

```
DeliverFoodApp/
├── src/
│   ├── FoodDelivery.Api/                    # REST API, gRPC, SignalR
│   │   ├── Controllers/
│   │   │   ├── MenuController.cs            # SCENARIO 1: Search
│   │   │   ├── InventoryController.cs       # SCENARIO 4: Check stock
│   │   │   └── OrderController.cs           # SCENARIO 2,7: Orders
│   │   ├── Hubs/
│   │   │   └── OrderHub.cs                  # SCENARIO 10: SignalR
│   │   ├── Services/
│   │   │   └── OrderGrpcService.cs          # SCENARIO 7: gRPC
│   │   ├── Protos/
│   │   │   └── order.proto                  # gRPC contract
│   │   ├── Data/
│   │   │   └── AppDbContext.cs              # EF Core + migrations
│   │   ├── Models/
│   │   │   └── DomainModels.cs              # Order, MenuItem, Inventory
│   │   ├── Program.cs                       # DI configuration
│   │   ├── appsettings.json                 # Config
│   │   └── FoodDelivery.Api.csproj
│   │
│   ├── FoodDelivery.Consumers/              # Background message processing
│   │   ├── Consumers/
│   │   │   ├── PaymentConsumer.cs           # SCENARIO 5,9: Payment
│   │   │   ├── InventoryReserveConsumer.cs  # SCENARIO 6: Notify
│   │   │   ├── EmailConsumer.cs             # SCENARIO 3: Email
│   │   │   ├── PartnerIntegrationConsumer.cs # SCENARIO 11: Partner
│   │   │   └── NotificationBridgeConsumer.cs # SCENARIO 10: SignalR
│   │   ├── Program.cs                       # Consumer setup
│   │   ├── appsettings.json
│   │   └── FoodDelivery.Consumers.csproj
│   │
│   └── FoodDelivery.Shared/                 # Shared events
│       ├── Events/
│       │   └── DomainEvents.cs
│       └── FoodDelivery.Shared.csproj
│
├── docker-compose.yml                       # Local dev: SQL, RabbitMQ, Kafka
├── Dockerfile.api                           # Multi-stage build
├── Dockerfile.consumers
├── DeliverFoodApp.sln                       # Visual Studio solution
├── .gitignore
└── README.md                                # This file
```

---

## Configuration

All configuration lives in `appsettings.json`:

**FoodDelivery.Api/appsettings.json:**
```json
{
  "ConnectionStrings": {
    "Default": "Server=sql-server,1433;Database=FoodDelivery;..."
  },
  "RabbitMQ": {
    "Host": "rabbitmq",
    "Username": "guest",
    "Password": "guest"
  },
  "Kafka": {
    "BootstrapServers": "kafka:9092"
  },
  "PartnerApi": {
    "BaseUrl": "https://api.delivery-partner.com",
    "Key": "demo-key-123"
  }
}
```

For **production**, use Azure Key Vault:
```csharp
// Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri("https://fooddelivery-kv.vault.azure.net/"),
    new DefaultAzureCredential());
```

---

## Deployment

### Docker (Local)
```bash
docker-compose up -d
```

### Kubernetes (AKS)
See `deploy/kubernetes/` folder for manifests. Deploy with:
```bash
kubectl apply -f deploy/kubernetes/
```

### Azure DevOps CI/CD
See `azure-pipelines.yml` for the complete CI/CD pipeline:
- **Build**: Restore → Build → Test → Docker push to ACR
- **Deploy Dev**: Blue-green with Helm
- **Deploy Prod**: Canary 10% → 50% → 100% with approval gates

---

## Common Tasks

### Scale Consumers (Parallel Processing)
```bash
# Locally: run multiple consumer instances
docker-compose up -d --scale order-consumers=5

# Kubernetes: update replica count
kubectl scale deployment order-consumers --replicas=10
```

### Rebuild Containers
```bash
docker-compose build
```

### View Message Queue
```
http://localhost:15672/
Navigate to: Queues → order-created
```

### Check Consumer Health
```bash
docker-compose logs order-consumers | grep "error"
```

### Wipe Database
```bash
docker-compose down -v
docker-compose up sql-server
# Wait for health check, then restart API
```

---

## Testing Scenarios

### Test SCENARIO 5: Payment Retry
The `PaymentConsumer` simulates a 10% failure rate. Monitor RabbitMQ to see retries:
1. Place order
2. Go to RabbitMQ UI → Queues → `order-created`
3. Observe message resend after 5 seconds (retry interval)
4. After 3 retries, message moves to dead-letter queue

### Test SCENARIO 6: Pub/Sub Fanout
Place an order and observe:
1. `PaymentConsumer` processes payment
2. `InventoryReserveConsumer` processes inventory
3. `EmailConsumer` sends email
4. `PartnerIntegrationConsumer` calls partner API
5. All 4 run in parallel, unaware of each other

### Test SCENARIO 12: Saga with Compensation
Enable "inventory failure" by modifying `InventoryReserveConsumer`:
1. Place order → payment succeeds
2. Inventory fails (modify success rate to 0%)
3. Saga auto-publishes `RefundPaymentEvent`
4. `PaymentConsumer` refunds the charge
5. Order ends in `Failed` status

---

## Troubleshooting

| Issue | Cause | Solution |
|-------|-------|----------|
| `Connection refused: localhost:5672` | RabbitMQ not running | `docker-compose up -d rabbitmq` |
| `Connection refused: localhost:1433` | SQL Server not ready | Wait 30 seconds for health check |
| `gRPC service not found` | gRPC not mapped | Verify `MapGrpcService<>()` in Program.cs |
| `OrderHub connection fails` | SignalR path wrong | Use `/hubs/orders` endpoint |
| `Order never completes` | Consumer crashed | Check `docker-compose logs order-consumers` |
| Database migration error | Connection string wrong | Verify `appsettings.json` |

---

## Next Steps

1. ✅ **Local Development**: Start containers, test REST APIs
2. 📊 **Monitor**: Watch RabbitMQ and logs for async flows
3. 🧪 **Test Scenarios**: Trigger each of the 12 scenarios
4. 🚀 **Deploy**: Push to Azure DevOps, deploy to AKS
5. 📈 **Scale**: Add more consumer replicas, monitor performance
6. 🔐 **Secure**: Move secrets to Azure Key Vault
7. 🔍 **Observe**: Add Application Insights for tracing

---

## References

- **Microservices**: See `interview_answers.md` (4500+ lines of comprehensive documentation)
- **MassTransit**: https://masstransit.io/
- **gRPC**: https://grpc.io/
- **RabbitMQ**: https://www.rabbitmq.com/
- **Kafka**: https://kafka.apache.org/
- **SignalR**: https://learn.microsoft.com/en-us/aspnet/core/signalr/

---

## License

MIT — Free for educational and commercial use.

---

## Summary

This is a **working, production-ready** microservices application demonstrating:
- Synchronous communication (REST, gRPC)
- Asynchronous messaging (RabbitMQ, Kafka)
- Real-time push (SignalR)
- Distributed transactions (Saga)
- Containerization (Docker)
- Cloud deployment (Azure, Kubernetes)

**All scenarios can be tested locally with a single `docker-compose up -d` command.**
