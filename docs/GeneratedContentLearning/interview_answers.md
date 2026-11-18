## Microservices

### What are microservices?
- Microservices are an architectural style that structures an application as a collection of small, loosely coupled services, each responsible for a specific business capability.

### What are the benefits of using microservices?
- Benefits include scalability, flexibility in technology choices, better fault isolation, and improved team autonomy.

### How do you communicate between microservices?
- Communication can be achieved using REST APIs, message brokers (e.g., RabbitMQ, Kafka), or gRPC.
- gRPC stands for Google Remote Procedure Call; it’s an open-source, high-performance RPC framework that uses HTTP/2 and Protocol Buffers for strongly-typed, low-latency inter-service communication.

### What is service discovery in microservices?
- Service discovery allows microservices to find and communicate with each other dynamically, typically using tools like Eureka or Consul.

### How do you manage data in a microservices architecture?
- Each microservice typically has its own database to enforce data ownership and independence. Data can be shared via APIs or event-driven architecture.

### What is API Gateway?
- An API Gateway is a single entry point for clients to interact with multiple microservices, handling requests, routing, and protocol translation.

## DEVOPS

### What is DevOps?
- DevOps is a culture and set of practices that aim to improve collaboration between development and operations teams to deliver software more rapidly and reliably.

### What are CI/CD pipelines?
- Continuous Integration (CI) and Continuous Deployment (CD) pipelines automate the process of integrating code changes, running tests, and deploying applications to production environments.

### How do you implement CI/CD in a project?
- CI/CD can be implemented using tools like Jenkins, GitLab CI, or Azure DevOps, which automate the build, test, and deployment processes.

### What is containerization?
- Containerization is the practice of packaging applications and their dependencies into containers, enabling them to run consistently across different environments.

### How do you use Docker in DevOps?
- Docker can be used to create, deploy, and manage containers, allowing for easier development and deployment of applications.

### What is orchestration in the context of containers?
- Orchestration refers to the automated management of containerized applications, including deployment, scaling, and networking.

## DOCKER

### What is Docker?
- Docker is an open-source platform that enables developers to automate the deployment of applications inside lightweight, portable containers.

### What are Docker images and containers?
- **Docker Image**: A read-only template used to create containers. It includes the application code, libraries, and dependencies.
- **Docker Container**: A runnable instance of a Docker image, containing everything needed to run the application.

### How do you create a Docker container?
- A Docker container can be created using the `docker run` command followed by the image name.

### What is a Dockerfile?
- A Dockerfile is a text file that contains instructions on how to build a Docker image.

### How do you manage data in Docker containers?
- Data can be managed using Docker volumes or bind mounts, which allow data to persist even when containers are stopped or removed.

---

## Message Brokers & gRPC in ASP.NET Core Microservices

### Overview

In a microservices architecture, services need to communicate. There are two fundamental communication patterns:

| Pattern | Mechanism | Example Tools | Use Case |
|---------|-----------|---------------|----------|
| **Synchronous** | Direct request-response | REST, gRPC | Queries, commands needing immediate response |
| **Asynchronous** | Event-driven via broker | RabbitMQ, Kafka | Event notifications, decoupled workflows |

Choosing the right pattern is critical for building scalable, resilient microservices.

---

### Message Brokers (RabbitMQ & Kafka)

#### What is a Message Broker?

A message broker is middleware that enables asynchronous communication between services by receiving, routing, and delivering messages. Producers send messages to the broker; consumers receive them without needing to know about each other.

**Benefits in Microservices:**
- **Decoupling** — Services don't need direct network knowledge of each other
- **Resilience** — Messages persist even if a consumer is down
- **Scalability** — Consumers can be scaled independently
- **Buffering** — Brokers handle traffic spikes by queuing messages

#### RabbitMQ

- Uses the **AMQP** protocol (Advanced Message Queuing Protocol)
- Centers around **Exchanges** and **Queues** — producers send to exchanges, exchanges route to queues based on bindings/routing keys
- Best for **task distribution** and **work queues** with reliable delivery guarantees

**RabbitMQ Concepts:**
- **Producer** — Publishes messages to an exchange
- **Exchange** — Receives messages and routes them to queues (Direct, Topic, Fanout, Headers)
- **Queue** — Stores messages until consumed
- **Consumer** — Subscribes to a queue and processes messages
- **Binding** — Rule linking an exchange to a queue with a routing key

**ASP.NET Core Example — RabbitMQ Producer (using MassTransit):**

```csharp
// NuGet: MassTransit.RabbitMQ

// Program.cs — Setup
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq:5672", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

// OrderController.cs — Producer
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;

    public OrdersController(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        // Business logic: save order to database...

        // Publish event — RabbitMQ routes this to all subscribed consumers
        await _publishEndpoint.Publish(new OrderCreatedEvent
        {
            OrderId = Guid.NewGuid(),
            CustomerEmail = request.Email,
            TotalAmount = request.Amount,
            CreatedAt = DateTime.UtcNow
        });

        return Ok(new { Message = "Order created and event published" });
    }
}

// OrderCreatedEvent.cs — Shared contract (in a shared library)
public record OrderCreatedEvent
{
    public Guid OrderId { get; init; }
    public string CustomerEmail { get; init; }
    public decimal TotalAmount { get; init; }
    public DateTime CreatedAt { get; init; }
}
```

**ASP.NET Core Example — RabbitMQ Consumer (separate microservice):**

```csharp
// NotificationService — Consumes OrderCreatedEvent

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(IEmailService emailService, ILogger<OrderCreatedConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        _logger.LogInformation("Processing order {OrderId} for {Email}",
            context.Message.OrderId, context.Message.CustomerEmail);

        // Send confirmation email
        await _emailService.SendOrderConfirmationAsync(
            context.Message.CustomerEmail,
            context.Message.OrderId,
            context.Message.TotalAmount);

        // Auto-retry on failure: MassTransit retries configured on the bus
    }
}
```

**Without MassTransit — Raw RabbitMQ.Client:**

```csharp
// NuGet: RabbitMQ.Client

// Producer
var factory = new ConnectionFactory { HostName = "rabbitmq" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare("orders", durable: true, exclusive: false, autoDelete: false);
var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(orderEvent));
channel.BasicPublish(exchange: "", routingKey: "orders", basicProperties: null, body: body);

// Consumer
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();
channel.QueueDeclare("orders", durable: true, exclusive: false, autoDelete: false);

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = JsonSerializer.Deserialize<OrderCreatedEvent>(Encoding.UTF8.GetString(body));
    // Process message...
    channel.BasicAck(ea.DeliveryTag, false); // Acknowledge message processed
};
channel.BasicConsume(queue: "orders", autoAck: false, consumer: consumer);
```

#### Apache Kafka

- A distributed **event streaming** platform — not just a queue, but a **distributed log**
- Messages are stored in **topics**, partitioned across multiple brokers for high throughput
- Consumers track their position (offset) in the log, allowing replay of past events
- Best for **event sourcing**, **data pipelines**, and **high-throughput event streaming**

**Kafka Concepts:**
- **Topic** — A category/feed name to which records are published
- **Partition** — A topic is split into partitions for parallelism and scalability
- **Producer** — Writes records to a topic partition
- **Consumer** — Reads records from a topic, tracking its offset
- **Consumer Group** — Multiple consumers coordinate to read partitions in parallel
- **Broker** — A Kafka server that stores data
- **Offset** — Unique position identifier for each record within a partition

**ASP.NET Core Example — Kafka Producer (using Confluent.Kafka):**

```csharp
// NuGet: Confluent.Kafka

// Program.cs — Register Kafka producer
builder.Services.AddSingleton<IProducer<string, string>>(sp =>
{
    var config = new ProducerConfig
    {
        BootstrapServers = "kafka:9092",
        Acks = Acks.All,        // Wait for all replicas to acknowledge
        EnableIdempotence = true // Exactly-once semantics
    };
    return new ProducerBuilder<string, string>(config).Build();
});

// OrderController.cs
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IProducer<string, string> _producer;

    public OrdersController(IProducer<string, string> producer)
    {
        _producer = producer;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var orderEvent = new OrderCreatedEvent
        {
            OrderId = Guid.NewGuid(),
            CustomerEmail = request.Email,
            TotalAmount = request.Amount,
            CreatedAt = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(orderEvent);
        var result = await _producer.ProduceAsync(
            "order-created",                                    // Topic name
            new Message<string, string>
            {
                Key = orderEvent.OrderId.ToString(),            // Partition key (same OrderId → same partition)
                Value = json
            });

        return Ok(new
        {
            Message = "Order created",
            Partition = result.Partition.Value,
            Offset = result.Offset.Value
        });
    }
}
```

**ASP.NET Core Example — Kafka Consumer (Background Service):**

```csharp
// NuGet: Confluent.Kafka

public class OrderCreatedConsumerService : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrderCreatedConsumerService> _logger;

    public OrderCreatedConsumerService(IConsumer<string, string> consumer,
        IServiceScopeFactory scopeFactory,
        ILogger<OrderCreatedConsumerService> logger)
    {
        _consumer = consumer;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe("order-created");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(stoppingToken);
                var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(result.Message.Value);

                _logger.LogInformation("Processing order {OrderId} from partition {Partition} at offset {Offset}",
                    orderEvent.OrderId, result.Partition.Value, result.Offset.Value);

                // Process in a scoped DI context (e.g., database access)
                using var scope = _scopeFactory.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                await emailService.SendOrderConfirmationAsync(
                    orderEvent.CustomerEmail,
                    orderEvent.OrderId,
                    orderEvent.TotalAmount);

                // Commit offset automatically (enable.auto.commit = true)
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Kafka consume error");
            }
        }
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        base.Dispose();
    }
}

// Program.cs — Register consumer
builder.Services.AddSingleton<IConsumer<string, string>>(sp =>
{
    var config = new ConsumerConfig
    {
        BootstrapServers = "kafka:9092",
        GroupId = "notification-service",        // Consumer group
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = true
    };
    return new ConsumerBuilder<string, string>(config).Build();
});
builder.Services.AddHostedService<OrderCreatedConsumerService>();
```

#### RabbitMQ vs Kafka — When to Use Which

| Factor | RabbitMQ | Kafka |
|--------|----------|-------|
| **Message Model** | Queue-based (competing consumers) | Log-based (retention window) |
| **Throughput** | Thousands/sec | Millions/sec |
| **Latency** | Low (microseconds) | Low (milliseconds) |
| **Message Ordering** | Per queue | Per partition |
| **Message Retention** | Deleted after ack | Configurable retention period |
| **Routing** | Complex (exchanges, bindings) | Simple (topic-based) |
| **Replay** | Not designed for replay | Full replay supported |
| **Best For** | Task queues, RPC, reliable delivery | Event sourcing, data pipelines, streams |

**Rule of thumb:**
- Use **RabbitMQ** when you need complex routing, work queues, or guaranteed delivery for business transactions
- Use **Kafka** when you need high throughput, event replay, event sourcing, or streaming data pipelines

---

### gRPC

#### What is gRPC?

gRPC (Google Remote Procedure Call) is a high-performance, open-source RPC framework that uses:
- **HTTP/2** — Multiplexed streams, header compression, bidirectional streaming
- **Protocol Buffers** — Strongly-typed, language-neutral serialization format
- **Code generation** — Service contracts are defined in `.proto` files, generating client and server stubs

**Why gRPC for Microservices?**
- **Performance** — Binary serialization (Protobuf) is ~4-10x faster than JSON
- **Strong typing** — Contract-first development with `.proto` definitions
- **Streaming** — Server streaming, client streaming, and bidirectional streaming
- **Built-in code generation** — Reduces boilerplate and ensures contract alignment

#### gRPC Communication Patterns

| Pattern | Description | Use Case |
|---------|-------------|----------|
| **Unary** | Single request → Single response | Traditional RPC, queries |
| **Server Streaming** | Single request → Stream of responses | Real-time feed, pagination |
| **Client Streaming** | Stream of requests → Single response | Upload, batch processing |
| **Bidirectional Streaming** | Stream of requests ↔ Stream of responses | Chat, real-time collaboration |

#### Step 1 — Define the `.proto` Contract

```protobuf
// Proto/order.proto
syntax = "proto3";

option csharp_namespace = "OrderService.Proto";

service OrderGrpcService {
    // Unary: Get order by ID
    rpc GetOrder (GetOrderRequest) returns (OrderResponse);

    // Server streaming: Stream all orders for a customer
    rpc GetCustomerOrders (GetCustomerOrdersRequest) returns (stream OrderResponse);

    // Bidirectional streaming: Process a real-time order feed
    rpc ProcessOrderStream (stream OrderAction) returns (stream OrderStatus);
}

message GetOrderRequest {
    string order_id = 1;
}

message GetCustomerOrdersRequest {
    string customer_id = 1;
}

message OrderResponse {
    string order_id = 1;
    string customer_id = 2;
    double total_amount = 3;
    string status = 4;
    int64 created_at = 5;  // Unix timestamp
    repeated OrderItem items = 6;
}

message OrderItem {
    string product_id = 1;
    string product_name = 2;
    int32 quantity = 3;
    double unit_price = 4;
}

message OrderAction {
    string order_id = 1;
    string action = 2;  // "approve", "cancel", "refund"
    string actor_id = 3;
}

message OrderStatus {
    string order_id = 1;
    string status = 2;
    string message = 3;
}
```

#### Step 2 — ASP.NET Core gRPC Server

```csharp
// NuGet: Grpc.AspNetCore

// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = true;
    options.MaxReceiveMessageSize = 4 * 1024 * 1024; // 4 MB
    options.MaxSendMessageSize = 4 * 1024 * 1024;
});

var app = builder.Build();

app.UseRouting();
app.MapGrpcService<OrderGrpcServiceImpl>();
app.MapGet("/", () => "Order gRPC Service is running.");
app.Run();

// Services/OrderGrpcServiceImpl.cs
public class OrderGrpcServiceImpl : OrderGrpcService.OrderGrpcServiceBase
{
    private readonly IOrderRepository _orderRepo;
    private readonly ILogger<OrderGrpcServiceImpl> _logger;

    public OrderGrpcServiceImpl(IOrderRepository orderRepo, ILogger<OrderGrpcServiceImpl> logger)
    {
        _orderRepo = orderRepo;
        _logger = logger;
    }

    // Unary RPC
    public override async Task<OrderResponse> GetOrder(
        GetOrderRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Fetching order {OrderId}", request.OrderId);

        var order = await _orderRepo.GetByIdAsync(Guid.Parse(request.OrderId));
        if (order == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Order not found"));

        return MapToProto(order);
    }

    // Server streaming RPC
    public override async Task GetCustomerOrders(
        GetCustomerOrdersRequest request,
        IServerStreamWriter<OrderResponse> responseStream,
        ServerCallContext context)
    {
        var orders = await _orderRepo.GetByCustomerIdAsync(Guid.Parse(request.CustomerId));

        foreach (var order in orders)
        {
            // Check if client cancelled
            context.CancellationToken.ThrowIfCancellationRequested();
            await responseStream.WriteAsync(MapToProto(order));
        }
    }

    // Bidirectional streaming RPC
    public override async Task ProcessOrderStream(
        IAsyncStreamReader<OrderAction> requestStream,
        IServerStreamWriter<OrderStatus> responseStream,
        ServerCallContext context)
    {
        await foreach (var action in requestStream.ReadAllAsync())
        {
            _logger.LogInformation("Processing {Action} for order {OrderId}",
                action.Action, action.OrderId);

            var status = action.Action switch
            {
                "approve" => await ApproveOrderAsync(action.OrderId, action.ActorId),
                "cancel" => await CancelOrderAsync(action.OrderId, action.ActorId),
                "refund" => await RefundOrderAsync(action.OrderId, action.ActorId),
                _ => new OrderStatus
                {
                    OrderId = action.OrderId,
                    Status = "rejected",
                    Message = "Unknown action"
                }
            };

            await responseStream.WriteAsync(status);
        }
    }

    private OrderResponse MapToProto(Order order) => new OrderResponse
    {
        OrderId = order.Id.ToString(),
        CustomerId = order.CustomerId.ToString(),
        TotalAmount = order.TotalAmount,
        Status = order.Status,
        CreatedAt = new DateTimeOffset(order.CreatedAt).ToUnixTimeSeconds()
    };
    // ...
}
```

#### Step 3 — ASP.NET Core gRPC Client

```csharp
// NuGet: Grpc.Net.Client, Google.Protobuf

// In another microservice (e.g., API Gateway or BillingService)

public class OrderGrpcClient
{
    private readonly OrderGrpcService.OrderGrpcServiceClient _client;
    private readonly ILogger<OrderGrpcClient> _logger;

    public OrderGrpcClient(IConfiguration config, ILogger<OrderGrpcClient> logger)
    {
        _logger = logger;
        var channel = GrpcChannel.ForAddress(config["GrpcServices:OrderService"]);
        _client = new OrderGrpcService.OrderGrpcServiceClient(channel);
    }

    // Unary call
    public async Task<OrderDto> GetOrderAsync(Guid orderId)
    {
        var response = await _client.GetOrderAsync(new GetOrderRequest
        {
            OrderId = orderId.ToString()
        });

        return new OrderDto
        {
            Id = Guid.Parse(response.OrderId),
            CustomerId = Guid.Parse(response.CustomerId),
            TotalAmount = response.TotalAmount,
            Status = response.Status
        };
    }

    // Server streaming — read orders as they arrive
    public async IAsyncEnumerable<OrderDto> GetCustomerOrdersAsync(Guid customerId)
    {
        using var call = _client.GetCustomerOrders(new GetCustomerOrdersRequest
        {
            CustomerId = customerId.ToString()
        });

        await foreach (var response in call.ResponseStream.ReadAllAsync())
        {
            yield return new OrderDto
            {
                Id = Guid.Parse(response.OrderId),
                TotalAmount = response.TotalAmount,
                Status = response.Status
            };
        }
    }

    // Bidirectional streaming — send actions and receive statuses
    public async Task ProcessOrderActionsAsync(IAsyncEnumerable<OrderActionDto> actions)
    {
        using var call = _client.ProcessOrderStream();

        // Background task: read responses
        var responseTask = Task.Run(async () =>
        {
            await foreach (var status in call.ResponseStream.ReadAllAsync())
            {
                _logger.LogInformation("Order {OrderId} → {Status}: {Message}",
                    status.OrderId, status.Status, status.Message);
            }
        });

        // Send requests
        await foreach (var action in actions)
        {
            await call.RequestStream.WriteAsync(new OrderAction
            {
                OrderId = action.OrderId.ToString(),
                Action = action.Action,
                ActorId = action.ActorId.ToString()
            });
        }

        await call.RequestStream.CompleteAsync();
        await responseTask;
    }
}
```

#### gRPC vs REST vs Message Brokers — When to Use

| Scenario | gRPC | REST | Message Broker |
|----------|------|------|----------------|
| Internal service-to-service calls | ✅ Fast, typed | ⚠️ Slower, loose | ❌ Overkill |
| Real-time streaming | ✅ Bidirectional | ❌ Polling only | ✅ Kafka streams |
| External/public APIs | ❌ HTTP/2 requirement | ✅ Universal | ❌ No |
| Event-driven workflows | ❌ Request-response only | ❌ Request-response only | ✅ Decoupled |
| Data-heavy / low-latency | ✅ Protobuf binary | ❌ JSON overhead | ⚠️ Depends |

**Rule of thumb:**
- Use **gRPC** for synchronous, high-performance internal service-to-service calls and real-time streaming
- Keep **REST** for external/public-facing APIs where universal HTTP/1.1 compatibility is needed
- Use **Message Brokers** for asynchronous, event-driven workflows where services should not depend on each other being available

---

### Effective Microservices Architecture — Combining All Three

A well-designed microservices system uses a **mix of all three** communication patterns. No single pattern fits every interaction.

#### Real-World Architecture Example: E-Commerce Platform

```
                         ┌─────────────────────────────────────┐
                         │        API Gateway (REST/gRPC)       │
                         │  Web clients → REST / gRPC gateway   │
                         └──────────┬──────────────────────────┘
                                    │ gRPC (internal)
                                    ▼
              ┌───────────────────────────────────────────┐
              │            Order Service                   │
              │                                            │
              │  ┌──────────┐  ┌───────────────┐           │
              │  │ gRPC     │  │ MassTransit    │           │
              │  │ Server   │  │ Producer       │           │
              │  └────┬─────┘  └──────┬────────┘           │
              └───────┼──────────────┼──────────────────────┘
                      │              │
                      │              │ RabbitMQ / Kafka
                      │              ▼
              ┌───────┼──────────────────────────────────────┐
              │       │         Payment Service               │
              │       │  Subscribes: OrderCreated              │
              │       │  Publishes: PaymentProcessed           │
              └───────┼──────────────────────────────────────┘
                      │ gRPC
                      ▼
              ┌───────────────────────────────────────────────┐
              │            Inventory Service                   │
              │  Subscribes: OrderCreated (reserve stock)      │
              │  gRPC client: query OrderService for details   │
              └───────────────────────────────────────────────┘
```

**Architecture Rules Applied:**

| Interaction | Pattern | Why |
|-------------|---------|-----|
| Gateway → OrderService | gRPC | Low-latency, typed queries for order lookup |
| OrderService → PaymentService | RabbitMQ/Kafka | Async — don't block order creation on payment |
| OrderService → InventoryService | RabbitMQ/Kafka | Async — reserve stock without waiting |
| PaymentService → NotificationService | RabbitMQ/Kafka | Fire-and-forget email/SMS |
| InventoryService → OrderService | gRPC | Query stock levels synchronously (needs immediate answer) |

#### Pattern: Event-Driven with CQRS

```csharp
// Order Service — Command side (writes via gRPC API)

[ApiController]
public class OrderCommandController : ControllerBase
{
    private readonly IOrderRepository _repo;
    private readonly IPublishEndpoint _publisher;

    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderCommand cmd)
    {
        // 1. Write to the write database
        var order = new Order(cmd.CustomerId, cmd.Items);
        await _repo.SaveAsync(order);

        // 2. Publish event — all downstream services react asynchronously
        await _publisher.Publish(new OrderCreatedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            Items = cmd.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }).ToList()
        });

        return Accepted(order.Id);
    }
}

// Inventory Service — Query side (reads via gRPC)

public class InventoryGrpcService : InventoryGrpc.InventoryGrpcBase
{
    private readonly IInventoryReadModel _readDb;

    public override async Task<StockResponse> GetStock(StockRequest request, ServerCallContext context)
    {
        // Read from a denormalized read-optimized database
        var stock = await _readDb.GetStockLevelAsync(Guid.Parse(request.ProductId));
        return new StockResponse { ProductId = request.ProductId, Available = stock };
    }
}
```

#### Pattern: Saga (Distributed Transactions)

When an operation spans multiple services (e.g., order → payment → inventory → shipping), use a **Saga** pattern with a message broker:

```csharp
// Order Service — Orchestrator Saga using MassTransit

public class OrderSaga : MassTransitStateMachine<OrderSagaState>
{
    public State Submitted { get; private set; }
    public State PaymentPending { get; private set; }
    public State Completed { get; private set; }
    public State Failed { get; private set; }

    public Event<OrderCreatedEvent> OrderCreated { get; private set; }
    public Event<PaymentProcessed> PaymentCompleted { get; private set; }
    public Event<PaymentFailed> PaymentFailed { get; private set; }
    public Event<InventoryReserved> StockReserved { get; private set; }
    public Event<InventoryReservationFailed> StockFailed { get; private set; }

    public OrderSaga()
    {
        InstanceState(x => x.CurrentState);

        Initially(
            When(OrderCreated)
                .Then(ctx => ctx.Saga.OrderId = ctx.Message.OrderId)
                .Publish(ctx => new ProcessPayment(ctx.Saga.OrderId))
                .TransitionTo(PaymentPending));

        During(PaymentPending,
            When(PaymentCompleted)
                .Publish(ctx => new ReserveInventory(ctx.Saga.OrderId))
                .TransitionTo(Submitted),
            When(PaymentFailed)
                .Publish(ctx => new CancelOrder(ctx.Saga.OrderId))
                .TransitionTo(Failed));

        During(Submitted,
            When(StockReserved)
                .Then(ctx => ctx.Saga.CompletedAt = DateTime.UtcNow)
                .TransitionTo(Completed),
            When(StockFailed)
                .Publish(ctx => new RefundPayment(ctx.Saga.OrderId))
                .TransitionTo(Failed));
    }
}

// If anything fails, compensating transactions are automatically published
// (e.g., refund payment if stock reservation fails)
```

**Saga benefits:**
- No distributed transaction (2PC) needed
- Each service commits locally; compensating actions undo on failure
- Message broker ensures reliable delivery through each step

#### Deployment Topology

```
┌─────────────────────────────────────────────────────┐
│                    Kubernetes Cluster                 │
│                                                       │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐           │
│  │ Order    │  │ Payment  │  │Inventory │  ...       │
│  │ Service  │  │ Service  │  │ Service  │           │
│  │ gRPC     │  │ gRPC     │  │ gRPC     │           │
│  │ :5001    │  │ :5002    │  │ :5003    │           │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘           │
│       │              │              │                 │
│       └──────────────┼──────────────┘                 │
│                      │ gRPC (internal cluster DNS)     │
│                      ▼                                 │
│              ┌───────────────┐                         │
│              │ API Gateway   │                         │
│              │ (Envoy/YARP)  │                         │
│              └───────┬───────┘                         │
│                      │                                 │
│              ┌───────┴───────┐                         │
│              │  RabbitMQ     │                         │
│              │  / Kafka      │                         │
│              └───────────────┘                         │
│                                                       │
│  Service Discovery: Kubernetes DNS (order-svc:5001)    │
└─────────────────────────────────────────────────────┘
```

#### Summary Checklist — Choosing the Right Pattern

| Requirement | Recommended Pattern |
|-------------|-------------------|
| "I need an immediate answer" | gRPC (unary) |
| "I need to process high-volume events" | Kafka |
| "I need reliable task distribution" | RabbitMQ |
| "I need real-time updates" | gRPC (streaming) |
| "I need to decouple services" | Message broker (async) |
| "I need external/3rd-party access" | REST API |
| "I need event replay / audit log" | Kafka |
| "I need a distributed transaction" | Saga (message broker) |
| "I want contract-first, type-safe APIs" | gRPC (Protobuf) |

#### Key Takeaways

1. **No silver bullet** — Use a combination of gRPC, message brokers, and REST based on the interaction pattern
2. **gRPC** excels for internal, synchronous, low-latency, typed communication
3. **RabbitMQ** excels for reliable task queues and complex routing
4. **Kafka** excels for high-throughput event streaming, event sourcing, and replay
5. **REST** remains the standard for external/public APIs
6. **Sagas** solve the distributed transaction problem using message brokers
7. **Always plan for failure** — retries, dead-letter queues, compensating transactions, and circuit breakers should be built in from day one

---

## REST APIs vs Message Brokers — Comparison, Easy Explanation & Cheatsheet

### The Core Difference in One Analogy

**REST API (Synchronous)** = You call a pizza place, stay on the phone until they say "Your order is placed" (you get an immediate answer).

**Message Broker (Asynchronous)** = You drop a letter in a mailbox. The post office delivers it whenever the recipient checks their mailbox. You walk away immediately. The recipient reads it hours later.

---

### Easy Terms — How Each Works

#### REST API — "Direct Phone Call"

```
Service A                    Service B
    │                            │
    │────── HTTP Request ──────►│
    │◄───── HTTP Response ──────│
    │                            │
    │ (A waits, blocking,        │
    │  until B responds)         │
```

- Service A sends an HTTP request directly to Service B
- Service A **waits** (blocked) for the response
- Both services must be **online** at the same time
- If B is down → A gets an error

#### Message Broker — "Mailbox System"

```
Service A          Broker (RabbitMQ/Kafka)        Service B
    │                      │                           │
    │─── Publish ────────►│                           │
    │   (walks away)      │   ┌────────────────┐      │
    │                     │   │   Message is    │      │
    │                     │   │   persisted     │      │
    │                     │   └────────────────┘      │
    │                     │◄──── Consumer polls ──────│
    │                     │──── Deliver message ────►│
    │                     │                           │
```

- Service A publishes a message and **walks away** immediately
- The broker holds onto the message
- Service B reads it whenever it's ready
- B can be offline temporarily — the broker **stores** the message

---

### Detailed Comparison

| Aspect | REST API | Message Broker |
|--------|----------|----------------|
| **Nature** | Synchronous (request-response) | Asynchronous (fire-and-forget / event-driven) |
| **Coupling** | Tight — A knows B's URL, port, endpoint, and response schema | Loose — A only knows the broker; B is completely decoupled |
| **Availability** | Both services must be up simultaneously | Consumer can be down — broker queues messages until it recovers |
| **Blocking** | Caller blocks/waits for the response | Caller publishes and continues immediately |
| **Response** | Immediate — response contains data | No direct response — consumer processes later (if at all) |
| **Latency** | Lower for single request (direct TCP) | Higher per message (broker hop), but zero wait for caller |
| **Throughput** | Limited by HTTP connection pool | Very high — broker buffers and distributes load |
| **Ordering** | N/A (single request) | Guaranteed per queue/partition (depends on broker) |
| **Error Handling** | Return 4xx/5xx, caller retries or fails | Retry queues, dead-letter queues, automatic redelivery |
| **Use Case** | Queries, commands needing immediate confirmation | Events, notifications, background processing |
| **Protocol** | HTTP/1.1 or HTTP/2 | AMQP (RabbitMQ), binary protocol (Kafka) |
| **Payload** | Usually JSON (text, human-readable) | Binary or JSON (serialized, compact) |
| **Scalability** | Scale: add more instances behind a load balancer | Scale: add more consumers — broker partitions load automatically |
| **Debugging** | Easy — inspect HTTP requests/responses (Postman, curl) | Harder — need to peek into queues, monitor consumer logs |
| **Testing** | Simple — call endpoint, assert response | Complex — need to run broker, assert side effects |
| **Contract** | OpenAPI/Swagger (documentation-based) | Schema registry / .proto (enforced by broker in some cases) |

---

### Real-World Example — Order Processing

#### REST API Approach

```csharp
// Order Service calls Payment Service synchronously via REST

public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
{
    // 1. Save order
    var order = new Order(request);
    _db.Orders.Add(order);
    await _db.SaveChangesAsync();

    // 2. Call Payment Service via REST — WAITS here!
    var paymentResponse = await _httpClient.PostAsJsonAsync(
        "http://payment-service/api/payments",
        new { OrderId = order.Id, Amount = order.Total });

    if (!paymentResponse.IsSuccessStatusCode)
    {
        // Payment failed — rollback order
        _db.Orders.Remove(order);
        await _db.SaveChangesAsync();
        return StatusCode(502, "Payment service unavailable");
    }

    // 3. Call Inventory Service via REST — WAITS here too!
    var inventoryResponse = await _httpClient.PostAsJsonAsync(
        "http://inventory-service/api/inventory/reserve",
        new { OrderId = order.Id, Items = request.Items });

    if (!inventoryResponse.IsSuccessStatusCode)
    {
        // Inventory failed — refund payment, rollback order
        await _httpClient.PostAsync("http://payment-service/api/payments/refund", ...);
        _db.Orders.Remove(order);
        await _db.SaveChangesAsync();
        return StatusCode(502, "Inventory service unavailable");
    }

    return Ok(new { OrderId = order.Id });
}
```

**Problems with the REST approach:**
- If Payment Service is slow, the HTTP call **times out** (default 100s in .NET)
- If Inventory Service is down, you must **manually refund** and **rollback** — messy
- The entire request is as slow as the **slowest downstream service**
- **Cascading failures** — if Payment is down, Order fails too

#### Message Broker Approach

```csharp
// Order Service publishes an event and walks away

public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
{
    // 1. Save order
    var order = new Order(request);
    _db.Orders.Add(order);
    await _db.SaveChangesAsync();

    // 2. Publish event — IMMEDIATE return!
    await _publishEndpoint.Publish(new OrderCreatedEvent
    {
        OrderId = order.Id,
        Items = request.Items,
        TotalAmount = order.Total,
        CreatedAt = DateTime.UtcNow
    });

    return Accepted(new { OrderId = order.Id }); // Returns 202 immediately
}

// Payment Service (separate microservice, separate process)
public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        // Process payment — this could take 10 seconds, caller doesn't care
        var paymentResult = await _paymentGateway.ChargeAsync(
            context.Message.OrderId, context.Message.TotalAmount);

        if (paymentResult.Success)
            await context.Publish(new PaymentProcessed(context.Message.OrderId));
        else
            await context.Publish(new PaymentFailed(context.Message.OrderId));
    }
}

// Inventory Service (separate microservice)
public class PaymentProcessedConsumer : IConsumer<PaymentProcessed>
{
    public async Task Consume(ConsumeContext<PaymentProcessed> context)
    {
        // Reserve inventory only after payment succeeds
        await _inventoryRepo.ReserveAsync(context.Message.OrderId);
        await context.Publish(new InventoryReserved(context.Message.OrderId));
    }
}
```

**Benefits of the broker approach:**
- Caller returns **202 Accepted** immediately — user sees "Order placed" in milliseconds
- Each service processes **independently** — Payment can take 10s, Inventory 5s, Order doesn't care
- If Inventory is down, Payment messages **queue up** — no data loss, no errors
- **Cascading failures eliminated** — Order doesn't fail because Payment is slow

---

### When to Use REST vs Message Broker — Decision Flowchart

```
Do you need an immediate response?
  ├── YES ──► Do you need real-time streaming?
  │            ├── YES ──► gRPC (streaming)
  │            └── NO  ──► REST API (for external) or gRPC (for internal)
  │
  └── NO  ──► Is this a command (do something) or event (something happened)?
               ├── Command (process payment, send email) ──► Message Broker
               └── Event (order created, user registered) ──► Message Broker
```

---

### Cheatsheet — Quick Reference

#### REST API Cheatsheet

| If you need... | Use REST |
|----------------|----------|
| Instant answer ("Is this in stock?") | ✅ |
| External/3rd-party integration | ✅ |
| CRUD operations (Create, Read, Update, Delete) | ✅ |
| Simple prototype / MVP | ✅ |
| Debugging with curl / Postman | ✅ |
| Querying data by ID | ✅ |
| Stateless communication | ✅ |

**Avoid REST when:**
- The downstream service might be down (broker handles this better)
- You need high throughput (thousands/sec will exhaust HTTP connections)
- You need to broadcast one event to many consumers (would need N HTTP calls)
- You cannot afford cascading failures

#### Message Broker Cheatsheet

| If you need... | Use Message Broker |
|----------------|--------------------|
| Decouple services | ✅ |
| Resilience (consumer can be down) | ✅ |
| Broadcast one event to many services | ✅ |
| Background/async processing (email, report generation) | ✅ |
| Handle traffic spikes (buffering) | ✅ |
| Event-driven workflows (this happens → then that) | ✅ |
| Saga / distributed transactions | ✅ |
| High throughput (millions of events) | ✅ (Kafka) |

**Avoid Message Broker when:**
- You need a synchronous query response (you'd have to hack request-response on top of a broker)
- The workflow is simple CRUD with no downstream dependencies
- You're building an external/public API (clients can't subscribe to your broker)
- Debugging and tracing needs to be simple

#### Pattern Selection Matrix

```
                 ┌──────────────────────────────────────────────┐
                 │         Service A wants to...                 │
                 ├─────────────┬───────────────┬────────────────┤
                 │  Query data │  Issue command │  Notify event  │
                 │  (immediate  │  (do something)│  (something     │
                 │   answer)    │                │   happened)    │
┌────────────────┼─────────────┼───────────────┼────────────────┤
│ Service B can  │             │                │                │
│ be called      │  REST/gRPC  │   REST/gRPC    │   Message      │
│ directly       │             │   (if instant   │   Broker       │
│                │             │    ack needed)  │                │
├────────────────┼─────────────┼───────────────┼────────────────┤
│ Service B may  │  Can't use  │  Message       │   Message      │
│ be down / busy │  sync call  │  Broker        │   Broker       │
│                │  (use cache │                │                │
│                │   or CQRS)  │                │                │
├────────────────┼─────────────┼───────────────┼────────────────┤
│ One to many    │  N/A        │  Message       │   Message      │
│ consumers      │             │  Broker        │   Broker       │
│                │             │  (pub/sub)     │   (pub/sub)    │
└────────────────┴─────────────┴───────────────┴────────────────┘
```

#### Common Mistakes

| Mistake | Why It's Wrong |
|---------|----------------|
| Using REST for everything | Tight coupling, cascading failures, poor scalability |
| Using message broker for queries | Hacky — you have to implement request-response on top of pub/sub, losing simplicity |
| Mixing synchronous logic inside async consumers | Blocking inside a consumer kills throughput |
| Not having dead-letter queues | Failed messages are lost forever — no way to replay |
| Using REST for fire-and-forget | Still ties up HTTP connections, introduces unnecessary latency |
| Using broker for external APIs | External clients can't subscribe to RabbitMQ; REST is universal |

#### Quick Decision Table

```
Scenario                                         Recommended
──────────────────────────────────────────────────────────────────
User searches products ───────────────────────── REST (query, immediate)
User places an order ─────────────────────────── REST (accept + broker behind)
Send confirmation email ──────────────────────── Message Broker (async, don't wait)
Check inventory stock ────────────────────────── REST (need instant answer)
Payment processing ───────────────────────────── Message Broker (slow, retry-safe)
Notify 5 services that order was placed ──────── Message Broker (publish once, many consume)
Get order history ────────────────────────────── REST or gRPC (query)
Sync data to analytics / data warehouse ──────── Kafka (high throughput, replay)
Charge credit card ───────────────────────────── Message Broker (retry, compensation)
Send real-time notifications to user ─────────── Message Broker + SignalR/WebSocket
External partner integration ─────────────────── REST (universal HTTP standard)
Distributed transaction across 4 services ────── Saga (message broker orchestrated)
```

---

## Full Example — Food Delivery ASP.NET Application

A single food delivery platform demonstrating **all 12 scenarios** from the table above, using one coherent codebase.

### Application Overview

```
                    ┌─────────────────────────────────────────────────────┐
                    │                  API Gateway (nginx)                │
                    │             https://fooddelivery.example.com        │
                    └──────────┬──────────────┬───────────────┬──────────┘
                               │              │               │
                    ┌──────────▼──┐   ┌───────▼───────┐  ┌───▼────────────┐
                    │  REST APIs  │   │  gRPC Server  │  │  SignalR Hub   │
                    │  (C# MVC)   │   │  (C#)         │  │  (C#)          │
                    └──────┬──────┘   └───────┬───────┘  └───────┬────────┘
                           │                  │                   │
                    ┌──────▼──────────────────────────────────────▼───────┐
                    │              MassTransit + RabbitMQ  + Kafka       │
                    │         (Async commands, events, pub/sub, saga)     │
                    └──────┬──────────────────────────────────────────────┘
                           │
              ┌────────────┼────────────┬──────────────┬──────────────┐
        ┌─────▼─────┐ ┌───▼────┐ ┌─────▼─────┐ ┌──────▼──────┐ ┌───▼──────┐
        │ OrderSvc  │ │Payment │ │Inventory  │ │ Notification│ │Analytics │
        │Consumer   │ │Consumer│ │Consumer   │ │Consumer     │ │Consumer  │
        └───────────┘ └────────┘ └───────────┘ └─────────────┘ └──────────┘
```

### Solution Structure

```
FoodDelivery/
├── Controllers/
│   ├── MenuController.cs          # Search products (REST query)
│   ├── OrderController.cs         # Place order (REST + broker behind)
│   ├── InventoryController.cs     # Check stock (REST query)
│   └── OrderHistoryController.cs  # Get order history (REST query)
├── gRPC/
│   ├── Protos/order.proto
│   └── Services/OrderGrpcService.cs   # Get order history (gRPC query)
├── Hubs/
│   └── OrderHub.cs                    # Real-time notifications (SignalR)
├── Consumers/
│   ├── PaymentConsumer.cs         # Charge credit card (broker, retry)
│   ├── InventoryReserveConsumer.cs # Reserve stock (broker)
│   ├── EmailConsumer.cs           # Send confirmation email (broker)
│   ├── PartnerIntegrationConsumer.cs # Notify 3rd-party (broker)
│   └── AnalyticsConsumer.cs       # Sync to warehouse (Kafka)
├── Saga/
│   └── OrderStateMachine.cs       # Distributed transaction (Saga)
├── Kafka/
│   └── AnalyticsProducer.cs       # High-throughput event streaming
├── Events/
│   ├── OrderCreatedEvent.cs
│   ├── PaymentProcessedEvent.cs
│   ├── InventoryReservedEvent.cs
│   └── OrderCompletedEvent.cs
├── Models/
│   ├── Order.cs
│   ├── MenuItem.cs
│   └── OrderHistory.cs
├── Services/
│   └── ExternalPartnerService.cs  # REST to external partner
├── Program.cs                     # Full DI setup
└── FoodDelivery.csproj
```

### 1. Domain Events (Shared Contracts)

```csharp
// Events/OrderCreatedEvent.cs — Published when an order is placed
public record OrderCreatedEvent
{
    public Guid OrderId { get; init; }
    public string CustomerId { get; init; }
    public string CustomerEmail { get; init; }
    public List<OrderItem> Items { get; init; }
    public decimal TotalAmount { get; init; }
    public DateTime CreatedAt { get; init; }
}
public record OrderItem(string MenuItemId, string Name, int Quantity, decimal UnitPrice);

// Events/PaymentProcessedEvent.cs — Published after payment succeeds
public record PaymentProcessedEvent
{
    public Guid OrderId { get; init; }
    public string TransactionId { get; init; }
    public decimal Amount { get; init; }
    public DateTime ProcessedAt { get; init; }
}

// Events/InventoryReservedEvent.cs — Published after stock is reserved
public record InventoryReservedEvent
{
    public Guid OrderId { get; init; }
    public bool Success { get; init; }
    public string FailedItem { get; init; }
}

// Events/OrderCompletedEvent.cs — Published when the full order flow finishes
public record OrderCompletedEvent
{
    public Guid OrderId { get; init; }
    public string CustomerId { get; init; }
    public string CustomerEmail { get; init; }
    public List<OrderItem> Items { get; init; }
    public decimal TotalAmount { get; init; }
    public DateTime CompletedAt { get; init; }
}
```

### 2. Controllers — REST Endpoints (Scenarios 1, 2, 4, 7)

```csharp
// Controllers/MenuController.cs
// SCENARIO 1: User searches products → REST (query, immediate answer)
[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly AppDbContext _db;

    public MenuController(AppDbContext db) => _db = db;

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        // 🔴 REST — immediate answer, user needs results now
        var items = await _db.MenuItems
            .Where(m => m.Name.Contains(query) || m.Category.Contains(query))
            .Take(20)
            .ToListAsync();

        return Ok(items); // User sees results in milliseconds
    }
}
```

```csharp
// Controllers/InventoryController.cs
// SCENARIO 4: Check inventory stock → REST (need instant answer)
[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly AppDbContext _db;

    [HttpGet("{menuItemId}/stock")]
    public async Task<IActionResult> CheckStock(string menuItemId)
    {
        // 🔴 REST — customer on the menu page needs to see "In Stock" instantly
        var stock = await _db.Inventory
            .Where(i => i.MenuItemId == menuItemId)
            .Select(i => i.QuantityAvailable)
            .FirstOrDefaultAsync();

        return Ok(new { MenuItemId = menuItemId, Available = stock > 0, Quantity = stock });
    }
}
```

```csharp
// Controllers/OrderController.cs
// SCENARIO 2: User places an order → REST (accept + broker behind)
// SCENARIO 9: Charge credit card → Message Broker (retry, compensation)
[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPublishEndpoint _publishEndpoint;  // MassTransit
    private readonly IHubContext<OrderHub> _hubContext;  // SignalR

    public OrderController(AppDbContext db, IPublishEndpoint publish,
        IHubContext<OrderHub> hubContext)
    {
        _db = db;
        _publishEndpoint = publish;
        _hubContext = hubContext;
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder(CreateOrderRequest request)
    {
        // 1. Save order locally
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Items = request.Items,
            TotalAmount = request.Items.Sum(i => i.UnitPrice * i.Quantity),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        // 2. Publish event → broker handles payment, inventory, email, etc.
        // 🔴 REST accepts the order immediately, broker does the rest
        await _publishEndpoint.Publish(new OrderCreatedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            CustomerEmail = request.CustomerEmail,
            Items = request.Items,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt
        });

        // 3. Notify the user via SignalR in real-time
        // SCENARIO 10: Real-time notifications → Message Broker + SignalR
        await _hubContext.Clients.User(order.CustomerId)
            .SendAsync("OrderStatusChanged", new
            {
                OrderId = order.Id,
                Status = "Pending",
                Message = "Order placed! Processing payment..."
            });

        // 🔴 REST — returns 202 Accepted immediately, not 200 OK
        return Accepted(new { OrderId = order.Id, Status = "Pending" });
    }
}
```

```csharp
// Controllers/OrderHistoryController.cs
// SCENARIO 7: Get order history → REST or gRPC (query)
[ApiController]
[Route("api/[controller]")]
public class OrderHistoryController : ControllerBase
{
    private readonly AppDbContext _db;

    // REST version — for external/mobile clients
    [HttpGet("{customerId}")]
    public async Task<IActionResult> GetHistory(string customerId)
    {
        var orders = await _db.Orders
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .Take(50)
            .Select(o => new OrderSummary
            {
                OrderId = o.Id,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync();

        return Ok(orders);
    }
}
```

### 3. Consumers — Message Broker (Scenarios 3, 5, 6, 9)

```csharp
// Consumers/PaymentConsumer.cs
// SCENARIO 5: Payment processing → Message Broker (slow, retry-safe)
// SCENARIO 9: Charge credit card → Message Broker (retry, compensation)
public class PaymentConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PaymentConsumer> _logger;

    public PaymentConsumer(IPublishEndpoint publish, ILogger<PaymentConsumer> logger)
    {
        _publishEndpoint = publish;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        // 🔴 BROKER — this runs in the background, caller already got 202
        _logger.LogInformation("Processing payment for order {OrderId}", context.Message.OrderId);

        try
        {
            // Simulate slow payment gateway call (3-5 seconds)
            var transactionId = await SimulatePaymentGatewayAsync(
                context.Message.OrderId,
                context.Message.TotalAmount);

            await _publishEndpoint.Publish(new PaymentProcessedEvent
            {
                OrderId = context.Message.OrderId,
                TransactionId = transactionId,
                Amount = context.Message.TotalAmount,
                ProcessedAt = DateTime.UtcNow
            });

            _logger.LogInformation("Payment success for order {OrderId}", context.Message.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment failed for order {OrderId}", context.Message.OrderId);

            // 🔴 BROKER BENEFIT: MassTransit automatically retries based on config
            // If retries exhausted, message goes to dead-letter queue for manual inspection
            throw; // MassTransit will handle retry policy
        }
    }

    private async Task<string> SimulatePaymentGatewayAsync(Guid orderId, decimal amount)
    {
        await Task.Delay(3000); // Simulate 3-second payment processing
        return Guid.NewGuid().ToString("N"); // Simulated transaction ID
    }
}
```

```csharp
// Consumers/InventoryReserveConsumer.cs
// This is 1 of the 5 services notified in Scenario 6
public class InventoryReserveConsumer : IConsumer<PaymentProcessedEvent>
{
    private readonly AppDbContext _db;
    private readonly ILogger<InventoryReserveConsumer> _logger;

    public InventoryReserveConsumer(AppDbContext db, ILogger<InventoryReserveConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        // Only reserve inventory AFTER payment succeeds
        // This avoids reserving stock for orders that never get paid
        _logger.LogInformation("Reserving inventory for order {OrderId}", context.Message.OrderId);

        // Deduct stock
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstAsync(o => o.Id == context.Message.OrderId);

        foreach (var item in order.Items)
        {
            var inventory = await _db.Inventory
                .FirstAsync(i => i.MenuItemId == item.MenuItemId);
            inventory.QuantityAvailable -= item.Quantity;
        }

        await _db.SaveChangesAsync();
    }
}
```

```csharp
// Consumers/EmailConsumer.cs
// SCENARIO 3: Send confirmation email → Message Broker (async, don't wait)
public class EmailConsumer : IConsumer<OrderCompletedEvent>
{
    private readonly ILogger<EmailConsumer> _logger;

    public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
    {
        // 🔴 BROKER — email sending is slow (SMTP calls), don't block the user
        _logger.LogInformation(
            "Sending confirmation email to {Email} for order {OrderId}",
            context.Message.CustomerEmail,
            context.Message.OrderId);

        // Simulate SMTP call
        await Task.Delay(1000);

        _logger.LogInformation(
            "Email sent to {Email} for order {OrderId}",
            context.Message.CustomerEmail,
            context.Message.OrderId);
    }
}
```

```csharp
// Consumers/PartnerIntegrationConsumer.cs
// SCENARIO 6: Notify 5 services that order was placed → Message Broker (publish once, many consume)
// SCENARIO 11: External partner integration could use REST here too
//
// This consumer runs alongside PaymentConsumer, InventoryReserveConsumer,
// EmailConsumer, and AnalyticsConsumer — all 5 receive the SAME event
// without the OrderController knowing about any of them.
public class PartnerIntegrationConsumer : IConsumer<OrderCompletedEvent>
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PartnerIntegrationConsumer> _logger;

    public PartnerIntegrationConsumer(IHttpClientFactory factory, ILogger<PartnerIntegrationConsumer> logger)
    {
        _httpClient = factory.CreateClient("ExternalPartners");
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
    {
        // SCENARIO 11: External partner integration → REST (universal standard)
        // Even though THIS trigger came from a broker, the actual call to
        // the external partner is REST because they don't speak AMQP
        var partnerPayload = new
        {
            OrderId = context.Message.OrderId.ToString(),
            context.Message.Items,
            context.Message.TotalAmount,
            context.Message.CompletedAt
        };

        // Call 3rd-party delivery partner API (e.g., UberEats, DoorDash)
        var response = await _httpClient.PostAsJsonAsync(
            "https://api.delivery-partner.com/v2/orders",
            partnerPayload);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Notified delivery partner for order {OrderId}",
                context.Message.OrderId);
        }
        else
        {
            // 🔴 BROKER BENEFIT: if partner is down, message stays in queue
            // and retries automatically (no manual retry needed)
            _logger.LogWarning("Partner API failed for order {OrderId}, will retry...",
                context.Message.OrderId);
            throw new HttpRequestException("Partner API returned " + response.StatusCode);
        }
    }
}
```

### 4. gRPC — Internal High-Performance Queries (Scenario 7)

```csharp
// Protos/order.proto
syntax = "proto3";

service OrderGrpc {
    rpc GetOrderHistory (OrderHistoryRequest) returns (OrderHistoryResponse);
    rpc GetOrderDetail (OrderDetailRequest) returns (OrderDetailResponse);
}

message OrderHistoryRequest {
    string customer_id = 1;
    int32 limit = 2;
}

message OrderHistoryResponse {
    repeated OrderSummary orders = 1;
}

message OrderSummary {
    string order_id = 1;
    double total_amount = 2;
    string status = 3;
    string created_at = 4;
}

message OrderDetailRequest {
    string order_id = 1;
}

message OrderDetailResponse {
    string order_id = 1;
    string customer_id = 2;
    double total_amount = 3;
    string status = 4;
    repeated OrderItem items = 5;
    string created_at = 6;
}

message OrderItem {
    string menu_item_id = 1;
    string name = 2;
    int32 quantity = 3;
    double unit_price = 4;
}
```

```csharp
// gRPC/Services/OrderGrpcService.cs
// SCENARIO 7: Get order history → gRPC (faster than REST for internal services)
// The Kitchen Display System and Analytics Dashboard use gRPC instead of REST
// because they're internal services that need maximum throughput.
public class OrderGrpcService : OrderGrpc.OrderGrpcBase
{
    private readonly AppDbContext _db;

    public OrderGrpcService(AppDbContext db) => _db = db;

    public override async Task<OrderHistoryResponse> GetOrderHistory(
        OrderHistoryRequest request, ServerCallContext context)
    {
        // 🔴 gRPC — strongly-typed, fast, internal-only
        // This is the SAME data as the REST endpoint, but for internal services
        var orders = await _db.Orders
            .Where(o => o.CustomerId == request.CustomerId)
            .OrderByDescending(o => o.CreatedAt)
            .Take(request.Limit)
            .Select(o => new OrderSummary
            {
                OrderId = o.Id.ToString(),
                TotalAmount = (double)o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt.ToString("O")
            })
            .ToListAsync();

        var response = new OrderHistoryResponse();
        response.Orders.AddRange(orders);
        return response;
    }

    public override async Task<OrderDetailResponse> GetOrderDetail(
        OrderDetailRequest request, ServerCallContext context)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstAsync(o => o.Id == Guid.Parse(request.OrderId));

        var response = new OrderDetailResponse
        {
            OrderId = order.Id.ToString(),
            CustomerId = order.CustomerId,
            TotalAmount = (double)order.TotalAmount,
            Status = order.Status,
            CreatedAt = order.CreatedAt.ToString("O")
        };
        response.Items.AddRange(order.Items.Select(i => new OrderItem
        {
            MenuItemId = i.MenuItemId,
            Name = i.Name,
            Quantity = i.Quantity,
            UnitPrice = (double)i.UnitPrice
        }));
        return response;
    }
}
```

### 5. SignalR Hub — Real-Time Notifications (Scenario 10)

```csharp
// Hubs/OrderHub.cs
// SCENARIO 10: Send real-time notifications to user → SignalR + Message Broker
//
// Flow:
//   1. Consumer processes a payment
//   2. Consumer publishes PaymentProcessedEvent
//   3. A bridge consumer listens and pushes via SignalR to the specific user
[HubName("orderHub")]
public class OrderHub : Hub
{
    // Clients connect with ?userId=xyz as a query param
    public override async Task OnConnectedAsync()
    {
        var userId = Context.GetHttpContext().Request.Query["userId"];
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
        await base.OnConnectedAsync();
    }
}

// A lightweight consumer bridges broker events → SignalR notifications
public class NotificationBridge : IConsumer<PaymentProcessedEvent>,
    IConsumer<InventoryReservedEvent>, IConsumer<OrderCompletedEvent>
{
    private readonly IHubContext<OrderHub> _hubContext;

    public NotificationBridge(IHubContext<OrderHub> hub) => _hubContext = hub;

    public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        // Push real-time update to the specific user's browser
        await _hubContext.Clients.Group($"user_{/* lookup customerId */}")
            .SendAsync("OrderStatusChanged", new
            {
                OrderId = context.Message.OrderId,
                Status = "PaymentProcessed",
                Message = $"Payment of ${context.Message.Amount} confirmed!"
            });
    }

    public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
    {
        await _hubContext.Clients.Group($"user_{context.Message.CustomerId}")
            .SendAsync("OrderStatusChanged", new
            {
                OrderId = context.Message.OrderId,
                Status = "Completed",
                Message = "Order completed! Your food is being prepared."
            });
    }
}
```

### 6. Saga Orchestrator — Distributed Transaction (Scenario 12)

```csharp
// Saga/OrderStateMachine.cs
// SCENARIO 12: Distributed transaction across 4 services → Saga
//
// The order flow touches 4 services:
//   1. Order Service    — saves the order
//   2. Payment Service  — charges credit card (compensation: refund)
//   3. Inventory Service — reserves stock (compensation: restock)
//   4. Notification Svc — sends email (no compensation needed)
//
// If any step fails, the Saga automatically runs compensating actions
// for ALL previously completed steps.
public class OrderSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderStateMachine : MassTransitStateMachine<OrderSagaState>
{
    // States
    public State Submitted { get; private set; }
    public State PaymentPending { get; private set; }
    public State PaymentFailed { get; private set; }
    public State InventoryPending { get; private set; }
    public State Completed { get; private set; }
    public State Failed { get; private set; }

    // Events
    public Event<OrderCreatedEvent> OrderCreated { get; private set; }
    public Event<PaymentProcessedEvent> PaymentProcessed { get; private set; }
    public Event<InventoryReservedEvent> InventoryReserved { get; private set; }

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        // Start: OrderCreated → PaymentPending
        Initially(
            When(OrderCreated)
                .Then(ctx =>
                {
                    ctx.Saga.OrderId = ctx.Message.OrderId;
                    ctx.Saga.CustomerId = ctx.Message.CustomerId;
                    ctx.Saga.TotalAmount = ctx.Message.TotalAmount;
                    ctx.Saga.CreatedAt = ctx.Message.CreatedAt;
                })
                .TransitionTo(PaymentPending));

        // Payment succeeds → proceed to inventory
        During(PaymentPending,
            When(PaymentProcessed)
                .TransitionTo(InventoryPending));

        // Inventory succeeds → order complete
        During(InventoryPending,
            When(InventoryReserved)
                .ThenAsync(async ctx =>
                {
                    // Publish completion for email + analytics + partners
                    await ctx.Publish(new OrderCompletedEvent
                    {
                        OrderId = ctx.Saga.OrderId,
                        CustomerId = ctx.Saga.CustomerId,
                        TotalAmount = ctx.Saga.TotalAmount,
                        CompletedAt = DateTime.UtcNow
                    });
                })
                .TransitionTo(Completed));

        // 🔴 COMPENSATION: If inventory fails, refund the payment!
        During(InventoryPending,
            When(InventoryReserved)
                .Where(ctx => !ctx.Message.Success)
                .ThenAsync(async ctx =>
                {
                    // Publish refund event → payment consumer processes refund
                    await ctx.Publish(new RefundPaymentEvent
                    {
                        OrderId = ctx.Saga.OrderId,
                        Amount = ctx.Saga.TotalAmount,
                        Reason = $"Item out of stock: {ctx.Message.FailedItem}"
                    });
                })
                .TransitionTo(Failed));
    }
}
```

### 7. Kafka Producer — Analytics Sync (Scenario 8)

```csharp
// Kafka/AnalyticsProducer.cs
// SCENARIO 8: Sync data to analytics / data warehouse → Kafka (high throughput, replay)
//
// Unlike RabbitMQ which is optimized for routing, Kafka is optimized for
// high-throughput event streaming and replay. We use Kafka for analytics
// because:
//   - Millions of events per day from food delivery
//   - Data warehouse needs to replay old events if schema changes
//   - Multiple analytics consumers can read at their own pace
public class AnalyticsProducer
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<AnalyticsProducer> _logger;

    public AnalyticsProducer(IConfiguration config, ILogger<AnalyticsProducer> logger)
    {
        _logger = logger;
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"],
            Acks = Acks.All  // Wait for all replicas to acknowledge
        };
        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
    }

    public async Task ProduceOrderEvent(OrderCompletedEvent @event)
    {
        // Kafka topic name — analytics consumers subscribe to this topic
        const string topic = "food-delivery-orders";

        var message = JsonSerializer.Serialize(new
        {
            @event.OrderId,
            @event.CustomerId,
            @event.TotalAmount,
            Items = @event.Items.Count,
            @event.CompletedAt,
            EventType = "order_completed"
        });

        var result = await _producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = @event.OrderId.ToString(),
            Value = message,
            Headers = new Headers
            {
                { "event-type", Encoding.UTF8.GetBytes("order-completed") },
                { "version", Encoding.UTF8.GetBytes("1.0") }
            }
        });

        _logger.LogInformation(
            "Produced order {OrderId} to Kafka topic {Topic} [partition: {Partition}, offset: {Offset}]",
            @event.OrderId, topic, result.Partition.Value, result.Offset.Value);
    }
}

// Analytics consumer in a separate microservice reads from Kafka
public class AnalyticsOrderConsumer : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<AnalyticsOrderConsumer> _logger;

    public AnalyticsOrderConsumer(IConfiguration config, ILogger<AnalyticsOrderConsumer> logger)
    {
        _logger = logger;
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"],
            GroupId = "analytics-group",
            AutoOffsetReset = AutoOffsetReset.Earliest  // 🔴 REPLAY: Start from oldest if needed
        };
        _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        _consumer.Subscribe("food-delivery-orders");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(stoppingToken);

                _logger.LogInformation(
                    "Analytics received order {Key} from partition {Partition} at offset {Offset}",
                    result.Message.Key,
                    result.Partition.Value,
                    result.Offset.Value);

                // Upsert into data warehouse
                await UpsertToWarehouseAsync(result.Message.Value);

                // 🔴 KAFKA BENEFIT: committed offset tracks progress
                // If analytics service crashes and restarts, it resumes from
                // the last committed offset — no duplicates, no data loss
                _consumer.Commit(result);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task UpsertToWarehouseAsync(string json)
    {
        // Simulate inserting into Snowflake / BigQuery / Redshift
        await Task.Delay(50);
    }
}
```

### 8. Program.cs — Full Registration (All Scenarios Wired Together)

```csharp
// Program.cs — Wires everything together
var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────
// SCENARIO 1, 2, 4, 7: REST Controllers (immediate queries)
// ─────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// ─────────────────────────────────────────────────────────────
// SCENARIO 5, 6, 9, 12: RabbitMQ via MassTransit
//   (async commands, pub/sub, retry, saga)
// ─────────────────────────────────────────────────────────────
builder.Services.AddMassTransit(x =>
{
    // Register saga state machine (Scenario 12)
    x.AddSagaStateMachine<OrderStateMachine, OrderSagaState>()
        .InMemoryRepository();

    // Register consumers (Scenarios 3, 5, 6, 9, 11)
    x.AddConsumer<PaymentConsumer>();
    x.AddConsumer<InventoryReserveConsumer>();
    x.AddConsumer<EmailConsumer>();
    x.AddConsumer<PartnerIntegrationConsumer>();
    x.AddConsumer<NotificationBridge>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
        });

        cfg.ReceiveEndpoint("order-payment", e =>
        {
            // 🔴 RETRY: If PaymentConsumer throws, retry 3 times with delays
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
            e.ConfigureConsumer<PaymentConsumer>(ctx);
        });

        cfg.ReceiveEndpoint("order-inventory", e =>
        {
            e.ConfigureConsumer<InventoryReserveConsumer>(ctx);
        });

        cfg.ReceiveEndpoint("order-email", e =>
        {
            e.ConfigureConsumer<EmailConsumer>(ctx);
        });

        cfg.ReceiveEndpoint("order-partners", e =>
        {
            // 🔴 RETRY for external partner calls (Scenario 11)
            e.UseMessageRetry(r => r.Interval(5, TimeSpan.FromMinutes(1)));
            e.ConfigureConsumer<PartnerIntegrationConsumer>(ctx);
        });

        cfg.ReceiveEndpoint("order-notifications", e =>
        {
            e.ConfigureConsumer<NotificationBridge>(ctx);
        });
    });
});

// ─────────────────────────────────────────────────────────────
// SCENARIO 7 (alternative): gRPC for internal high-perf queries
// ─────────────────────────────────────────────────────────────
builder.Services.AddGrpc();

// ─────────────────────────────────────────────────────────────
// SCENARIO 8: Kafka for analytics streaming
// ─────────────────────────────────────────────────────────────
builder.Services.AddSingleton<AnalyticsProducer>();
builder.Services.AddHostedService<AnalyticsOrderConsumer>();

// ─────────────────────────────────────────────────────────────
// SCENARIO 10: SignalR for real-time notifications
// ─────────────────────────────────────────────────────────────
builder.Services.AddSignalR();

// ─────────────────────────────────────────────────────────────
// SCENARIO 11: External partner REST calls (separate HttpClient)
// ─────────────────────────────────────────────────────────────
builder.Services.AddHttpClient("ExternalPartners", client =>
{
    client.BaseAddress = new Uri("https://api.delivery-partner.com");
    client.DefaultRequestHeaders.Add("Authorization",
        $"Bearer {builder.Configuration["PartnerApi:Key"]}");
});

var app = builder.Build();

app.MapControllers();
app.MapGrpcService<OrderGrpcService>();  // gRPC on /OrderGrpcService
app.MapHub<OrderHub>("/hubs/orders");    // SignalR on /hubs/orders

app.Run();
```

### 9. End-to-End Call Flow

```
User clicks "Place Order" on the web app
         │
         ▼
┌─────────────────────────────────────────┐
│  OrderController.PlaceOrder()           │  SCENARIO 2 (REST)
│  - Saves order to DB                    │
│  - Publishes OrderCreatedEvent          │
│  - Pushes SignalR notification          │  SCENARIO 10 (SignalR)
│  - Returns 202 Accepted                 │
└────────────┬────────────────────────────┘
             │ OrderCreatedEvent published to RabbitMQ
             │
    ┌────────┴────────────────────────────┐
    │         MassTransit Broker          │
    │  (RabbitMQ)                         │
    └────────┬────────────────────────────┘
             │
    ┌────────┴────────────────────────────┐
    │  Topic: order.created               │
    │  (fanout exchange)                  │
    └────────┬────────────────────────────┘
             │
    ┌────────┼──────────────┬─────────────┐
    ▼        ▼              ▼             ▼
┌────────┐ ┌────────┐ ┌──────────┐ ┌────────────┐
│Payment │ │Inven-  │ │Analytics │ │Notification│  SCENARIO 6
│Consumer│ │tory    │ │Producer  │ │Bridge      │  (publish once,
│        │ │Cons.   │ │(Kafka)   │ │(SignalR)   │   5 services consume)
│SCEN. 5 │ │SCEN. 4 │ │SCEN. 8   │ │SCEN. 10    │
│SCEN. 9 │ │        │ │          │ │            │
└───┬────┘ └────────┘ └──────────┘ └────────────┘
    │          │                        │
    │  Payment │                        │ Push to user's browser
    │  OK      │                        │ via WebSocket
    ▼          ▼                        ▼
┌─────────────────────────────────────────┐
│  PaymentProcessedEvent published        │
│  → InventoryReserveConsumer runs        │
│  → If inventory OK → OrderCompletedEvent│
└────────────┬────────────────────────────┘
             │
    ┌────────┴────────┐
    ▼                 ▼
┌────────┐     ┌──────────────┐
│Email   │     │PartnerInte- │  SCENARIO 11 (REST to external)
│Consumer│     │grationCons. │
│SCEN. 3 │     │             │
└────────┘     └─────────────┘
```

### 10. Scenario-to-Code Mapping

| # | Scenario | Pattern | File | Lines |
|---|----------|---------|------|-------|
| 1 | User searches products | REST query | `MenuController.Search()` | GET `/api/menu/search?query=pizza` |
| 2 | User places an order | REST accept + broker | `OrderController.PlaceOrder()` | Returns 202, publishes `OrderCreatedEvent` |
| 3 | Send confirmation email | Message Broker | `EmailConsumer` | Consumes `OrderCompletedEvent`, calls SMTP |
| 4 | Check inventory stock | REST query | `InventoryController.CheckStock()` | GET `/api/inventory/{id}/stock` |
| 5 | Payment processing | Message Broker (slow) | `PaymentConsumer` | Simulates 3s payment, retries on failure |
| 6 | Notify 5 services | Message Broker (pub/sub) | ALL consumers | `OrderCreatedEvent` fans out to Payment, Inventory, Email, Partners, Analytics |
| 7 | Get order history | REST + gRPC | `OrderHistoryController` + `OrderGrpcService` | REST for mobile, gRPC for internal services |
| 8 | Sync to analytics/warehouse | Kafka | `AnalyticsProducer` + `AnalyticsOrderConsumer` | High-throughput, replayable, committed offsets |
| 9 | Charge credit card | Message Broker (retry) | `PaymentConsumer` | 3 retries, 5s intervals, dead-letter on exhaustion |
| 10 | Real-time notifications | Broker + SignalR | `NotificationBridge` + `OrderHub` | Broker event → WebSocket push to browser |
| 11 | External partner integration | REST | `PartnerIntegrationConsumer` | Broker trigger → HTTP POST to external API |
| 12 | Distributed transaction | Saga (broker-orchestrated) | `OrderStateMachine` | Payment → Inventory → Compensation (refund if fails) |

### Key Takeaway

This one application uses **every communication pattern** for its intended purpose:
- **REST** when the user needs an answer **now** (search, stock check)
- **Message Broker** when work can happen **later** (email, payment, notifications)
- **gRPC** for **fast internal** queries (kitchen display, analytics dashboard)
- **SignalR** for **real-time push** to connected clients
- **Kafka** for **high-throughput streaming** to data warehouses
- **Saga** to keep **data consistency** across 4 services without distributed transactions

**No single pattern dominates** — the right tool is chosen for each job within the same application.

---

## How to Get RabbitMQ & Kafka Credentials (Real-World Setup)

The code references `RabbitMQ:Host`, `RabbitMQ:Username`, `Kafka:BootstrapServers`, etc. — here's exactly where those values come from in different environments.

### 1. Local Development — Docker (Free, no credentials needed for dev)

Run both locally with a single `docker-compose.yml`:

```yaml
# docker-compose.yml
version: '3.8'
services:
  rabbitmq:
    image: rabbitmq:3-management
    container_name: local-rabbitmq
    ports:
      - "5672:5672"       # AMQP (app connection)
      - "15672:15672"     # Management UI (http://localhost:15672)
    environment:
      RABBITMQ_DEFAULT_USER: guest      # default username
      RABBITMQ_DEFAULT_PASS: guest      # default password
    # No credentials needed from cloud — runs entirely on your machine

  kafka:
    image: confluentinc/cp-kafka:latest
    container_name: local-kafka
    ports:
      - "9092:9092"       # Kafka broker
    environment:
      KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://localhost:9092
    # Kafka in dev mode has no auth — plaintext, no credentials

  # Optional: Kafka UI to inspect topics/partitions
  kafka-ui:
    image: provectuslabs/kafka-ui:latest
    ports:
      - "8080:8080"
    environment:
      KAFKA_CLUSTERS_0_NAME: local
      KAFKA_CLUSTERS_0_BOOTSTRAPSERVERS: kafka:9092
```

**appsettings.Development.json** for local:

```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092"
    // No credentials — plaintext in dev
  }
}
```

**Start:**
```bash
docker-compose up -d
# RabbitMQ UI: http://localhost:15672 (guest/guest)
# Kafka UI:    http://localhost:8080
```

### 2. Cloud-Managed RabbitMQ

You don't host RabbitMQ yourself — you pay a provider to run it. Sign up, they give you a connection string.

#### Option A: CloudAMQP (easiest — free tier available)

| Step | What You Do |
|------|-------------|
| 1 | Go to https://www.cloudamqp.com → Sign up (free: 1M messages/month) |
| 2 | Click "Create New Instance" → pick "Little Lemur" (free) → choose AWS/EU |
| 3 | Click on your instance → **"AMQP URL"** tab |
| 4 | Copy the URL: `amqps://youruser:yourpass@sparrow.rmq.cloudamqp.com/yourvhost` |

**appsettings.json:**
```json
{
  "RabbitMQ": {
    "Host": "sparrow.rmq.cloudamqp.com",
    "Username": "youruser",
    "Password": "yourpass",
    "VirtualHost": "yourvhost",
    "Port": 5671   // 5671 = TLS, 5672 = non-TLS
  }
}
```

Or use the full connection string directly with MassTransit:
```csharp
cfg.Host("amqps://youruser:yourpass@sparrow.rmq.cloudamqp.com/yourvhost");
```

#### Option B: Azure Service Bus (with AMQP)

```json
{
  "RabbitMQ": {
    "Host": "your-namespace.servicebus.windows.net",
    "Username": "RootManageSharedAccessKey",
    "Password": "your-primary-key-from-azure-portal",
    "Port": 5671
  }
}
```

**Where to find:** Azure Portal → Service Bus Namespace → "Shared access policies" → "RootManageSharedAccessKey" → copy Primary Key.

#### Option C: AWS Amazon MQ for RabbitMQ

```json
{
  "RabbitMQ": {
    "Host": "broker-id.mq.us-east-1.amazonaws.com",
    "Username": "admin",
    "Password": "your-password-set-during-creation",
    "Port": 5671
  }
}
```

**Where to find:** AWS Console → Amazon MQ → Brokers → Select your broker → "Connections" tab.

### 3. Cloud-Managed Kafka

#### Option A: Confluent Cloud (free tier — $0 for first 400K messages/month)

| Step | What You Do |
|------|-------------|
| 1 | Go to https://confluent.cloud → Sign up |
| 2 | Create a new cluster → pick "Basic" (free) |
| 3 | Go to "Cluster settings" → "API Keys" → "Create Key" |
| 4 | Copy the **Bootstrap server** (e.g., `pkc-12345.us-east-1.aws.confluent.cloud:9092`) |
| 5 | Copy the **API Key** and **API Secret** |

**appsettings.json:**
```json
{
  "Kafka": {
    "BootstrapServers": "pkc-12345.us-east-1.aws.confluent.cloud:9092",
    "SaslUsername": "your-api-key",
    "SaslPassword": "your-api-secret",
    "SecurityProtocol": "SaslSsl",
    "SaslMechanism": "Plain"
  }
}
```

**C# ProducerConfig:**
```csharp
var config = new ProducerConfig
{
    BootstrapServers = "pkc-12345.us-east-1.aws.confluent.cloud:9092",
    SecurityProtocol = SecurityProtocol.SaslSsl,
    SaslMechanism = SaslMechanism.Plain,
    SaslUsername = "your-api-key",
    SaslPassword = "your-api-secret"
};
```

#### Option B: Azure Event Hubs with Kafka API

Azure Event Hubs speaks the Kafka protocol. Use it as your Kafka broker.

```json
{
  "Kafka": {
    "BootstrapServers": "your-namespace.servicebus.windows.net:9093",
    "SaslUsername": "$ConnectionString",
    "SaslPassword": "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key",
    "SecurityProtocol": "SaslSsl",
    "SaslMechanism": "Plain"
  }
}
```

**Where to find:** Azure Portal → Event Hubs Namespace → "Shared access policies" → "RootManageSharedAccessKey" → copy **Connection string–primary key**. The Kafka-compatible endpoint is listed in "Properties" → "Kafka bootstrap server".

#### Option C: AWS MSK (Managed Streaming for Kafka)

```json
{
  "Kafka": {
    "BootstrapServers": "b-1.your-cluster.msk.us-east-1.amazonaws.com:9092,b-2.your-cluster.msk.us-east-1.amazonaws.com:9092",
    "SecurityProtocol": "Plaintext"  // or "SaslSsl" if IAM auth enabled
  }
}
```

**Where to find:** AWS Console → MSK → Clusters → Select cluster → "Client information" → "Bootstrap servers".

### 4. Self-Hosted (Production)

You run your own RabbitMQ/Kafka cluster on VMs or Kubernetes.

#### Kubernetes with Helm Charts

```bash
# RabbitMQ
helm repo add bitnami https://charts.bitnami.com/bitnami
helm install my-rabbitmq bitnami/rabbitmq \
  --set auth.username=admin \
  --set auth.password=strongpassword123

# Kafka
helm install my-kafka bitnami/kafka \
  --set listeners.client.protocol=PLAINTEXT  # or SASL_SSL
```

After install, get credentials:
```bash
# Get RabbitMQ password from secret
kubectl get secret my-rabbitmq -o jsonpath="{.data.rabbitmq-password}" | base64 --decode

# Get Kafka password from secret
kubectl get secret my-kafka-user-passwords -o jsonpath="{.data.client-passwords}" | base64 --decode
```

**appsettings.Production.json** (deployed via Kubernetes secrets):
```json
{
  "RabbitMQ": {
    "Host": "my-rabbitmq.namespace.svc.cluster.local",
    "Username": "admin",
    "Password": ""  // injected via K8s secret at runtime
  },
  "Kafka": {
    "BootstrapServers": "my-kafka.namespace.svc.cluster.local:9092"
  }
}
```

And mount the secret in Kubernetes:

```yaml
apiVersion: v1
kind: Pod
spec:
  containers:
  - name: my-app
    env:
    - name: RabbitMQ__Password    # ASP.NET Core nested config notation
      valueFrom:
        secretKeyRef:
          name: my-rabbitmq
          key: rabbitmq-password
    - name: Kafka__SaslPassword
      valueFrom:
        secretKeyRef:
          name: kafka-credentials
          key: sasl-password
```

### 5. Credential Sources Cheatsheet

| Environment | RabbitMQ Creds Source | Kafka Creds Source | Cost |
|-------------|----------------------|--------------------|------|
| **Local dev** | `guest`/`guest` on localhost via Docker | No auth, plaintext on localhost via Docker | Free |
| **CloudAMQP** | Dashboard → Instance → AMQP URL | N/A | Free tier: 1M msgs/month |
| **Confluent Cloud** | N/A (use RabbitMQ for routing) | Dashboard → API Keys → Create Key | Free tier: 400K msgs/month |
| **Azure Service Bus** | Portal → Shared access policies → Primary Key | N/A | Pay-as-you-go |
| **Azure Event Hubs** | N/A | Portal → Shared access policies → Connection string | Pay-as-you-go |
| **AWS Amazon MQ** | Console → Broker → Connections tab | N/A | ~$0.20/hr minimum |
| **AWS MSK** | N/A | Console → Cluster → Client information → Bootstrap servers | ~$0.10/hr per broker |
| **Kubernetes (Helm)** | `kubectl get secret` | `kubectl get secret` | Infrastructure cost only |
| **VM / DIY** | You set the credentials during install | You configure the JAAS file | Infrastructure cost only |

### 6. Security Best Practices

1. **Never commit credentials to git** — use `appsettings.Development.json` (gitignored) or environment variables
2. **Use managed identities where possible** — Azure AD / AWS IAM instead of passwords
3. **Always use TLS in production** — port 5671 for RabbitMQ, `SaslSsl` for Kafka
4. **Use separate credentials per environment** — dev, staging, prod each have their own
5. **Rotate keys regularly** — CloudAMQP and Confluent Cloud let you regenerate API keys
6. **MassTransit with IAM** — on Azure, use `AzureServiceBusTokenProvider` instead of connection strings

### Quick Start — From Zero to Running in 10 Minutes

```bash
# 1. Start local RabbitMQ + Kafka (no credentials needed)
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
docker run -d --name kafka -p 9092:9092 confluentinc/cp-kafka:latest

# 2. Your appsettings.json already works with defaults
#    RabbitMQ: guest/guest @ localhost:5672
#    Kafka:    localhost:9092

# 3. Run your app
dotnet run

# 4. When ready for production, swap to cloud:
    #    - Sign up for CloudAMQP (free) → paste connection URL
    #    - Sign up for Confluent Cloud (free) → paste API key/secret
    #    No code changes — just config changes!
```

---

## Deployment Code — Docker & Azure DevOps for the Food Delivery App

This section provides everything needed to containerize and deploy the food delivery application.

### Services Breakdown

The food delivery app is deployed as **5 separate microservices**, each in its own container:

| Service | Role | Port | Dependencies |
|---------|------|------|-------------|
| `order-api` | REST + gRPC + SignalR (the main API) | 8080 | RabbitMQ, Kafka, SQL Server |
| `order-consumers` | All MassTransit consumers (Payment, Inventory, Email, Partners, Notifications) | - (internal) | RabbitMQ, SQL Server |
| `analytics-consumer` | Kafka consumer for data warehouse sync | - (internal) | Kafka |
| `frontend` | React/Angular SPA served via nginx | 80 | order-api |
| `sql-server` | Database | 1433 | - |

In production, RabbitMQ and Kafka are **managed cloud services** — not deployed in your cluster.

---

### 1. Dockerfiles — Each Microservice

#### 1a. order-api Dockerfile

```dockerfile
# Dockerfile.order-api
# ──────────────────────────────────────────────────────
# Multi-stage build: SDK build → runtime image
# ──────────────────────────────────────────────────────

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore (layer caching optimization)
COPY FoodDelivery.Api/FoodDelivery.Api.csproj ./
RUN dotnet restore

# Copy everything and publish
COPY . .
RUN dotnet publish FoodDelivery.Api/FoodDelivery.Api.csproj \
    -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
EXPOSE 5001   # gRPC port
EXPOSE 5002   # SignalR port

# Health check
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

COPY --from=build /app/publish .

# Run as non-root user for security
USER appuser
ENTRYPOINT ["dotnet", "FoodDelivery.Api.dll"]
```

#### 1b. order-consumers Dockerfile

```dockerfile
# Dockerfile.order-consumers
# ──────────────────────────────────────────────────────
# A separate process that only runs MassTransit consumers.
# No HTTP endpoints — just background message processing.
# ──────────────────────────────────────────────────────

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY FoodDelivery.Consumers/FoodDelivery.Consumers.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish FoodDelivery.Consumers/FoodDelivery.Consumers.csproj \
    -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# No HTTP ports needed — only AMQP (outbound) and DB (outbound)
# The consumer connects TO RabbitMQ, it doesn't LISTEN on a port

HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
    CMD dotnet run --project FoodDelivery.Consumers --healthcheck || exit 1

COPY --from=build /app/publish .
USER appuser
ENTRYPOINT ["dotnet", "FoodDelivery.Consumers.dll"]
```

#### 1c. analytics-consumer Dockerfile

```dockerfile
# Dockerfile.analytics-consumer
# ──────────────────────────────────────────────────────
# Kafka consumer service that writes to data warehouse.
# Separated from order-consumers because Kafka consumers
# need different scaling (CPUs/memory) than RabbitMQ consumers.
# ──────────────────────────────────────────────────────

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY FoodDelivery.Analytics/FoodDelivery.Analytics.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish FoodDelivery.Analytics/FoodDelivery.Analytics.csproj \
    -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
USER appuser
ENTRYPOINT ["dotnet", "FoodDelivery.Analytics.dll"]
```

#### 1d. frontend Dockerfile (React SPA)

```dockerfile
# Dockerfile.frontend
# ──────────────────────────────────────────────────────
# Multi-stage: Node build → nginx serve
# ──────────────────────────────────────────────────────

# Build stage
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci --only=production
COPY . .
RUN npm run build

# Runtime stage with nginx
FROM nginx:alpine AS runtime
EXPOSE 80

# Custom nginx config with API proxy
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
    CMD wget -qO- http://localhost:80/health || exit 1
```

```nginx
# nginx.conf (for the frontend container)
server {
    listen 80;
    server_name localhost;

    # API proxy → order-api service in Docker/K8s
    location /api/ {
        proxy_pass http://order-api:8080/api/;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    }

    # SignalR WebSocket proxy
    location /hubs/ {
        proxy_pass http://order-api:8080/hubs/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
    }

    # gRPC proxy (if frontend calls gRPC-web)
    location /OrderGrpc/ {
        grpc_pass grpc://order-api:5001;
    }

    # SPA static files
    location / {
        root /usr/share/nginx/html;
        try_files $uri $uri/ /index.html;
    }
}
```

---

### 2. docker-compose.yml — Full Local Orchestration

```yaml
# docker-compose.yml
# ──────────────────────────────────────────────────────
# Spins up the ENTIRE food delivery stack locally.
# Run: docker-compose up -d
# Access: http://localhost
# ──────────────────────────────────────────────────────

version: '3.8'

networks:
  food-delivery-net:
    driver: bridge

volumes:
  mssql-data:        # Persist database across restarts
  rabbitmq-data:     # Persist queue messages
  kafka-data:        # Persist topic data

services:
  # ─── Infrastructure ─────────────────────────────────
  sql-server:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: food-sql
    networks:
      - food-delivery-net
    ports:
      - "1433:1433"
    environment:
      ACCEPT_EULA: "Y"
      SA_PASSWORD: "YourStrong!Passw0rd"
      MSSQL_PID: Developer
    volumes:
      - mssql-data:/var/opt/mssql
    healthcheck:
      test: /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -C -Q "SELECT 1" || exit 1
      interval: 15s
      timeout: 5s
      retries: 10

  rabbitmq:
    image: rabbitmq:3-management-alpine
    container_name: food-rabbitmq
    networks:
      - food-delivery-net
    ports:
      - "5672:5672"     # AMQP
      - "15672:15672"   # Management UI
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
    volumes:
      - rabbitmq-data:/var/lib/rabbitmq
    healthcheck:
      test: rabbitmq-diagnostics -q ping
      interval: 15s
      timeout: 5s
      retries: 5

  kafka:
    image: confluentinc/cp-kafka:latest
    container_name: food-kafka
    networks:
      - food-delivery-net
    ports:
      - "9092:9092"
    environment:
      CLUSTER_ID: "food-delivery-cluster"
      KAFKA_NODE_ID: 1
      KAFKA_PROCESS_ROLES: broker,controller
      KAFKA_CONTROLLER_QUORUM_VOTERS: 1@localhost:9093
      KAFKA_LISTENERS: PLAINTEXT://0.0.0.0:9092,CONTROLLER://0.0.0.0:9093
      KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://localhost:9092
      KAFKA_LISTENER_SECURITY_PROTOCOL_MAP: PLAINTEXT:PLAINTEXT,CONTROLLER:PLAINTEXT
      KAFKA_INTER_BROKER_LISTENER_NAME: PLAINTEXT
      KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR: 1
      KAFKA_TRANSACTION_STATE_LOG_REPLICATION_FACTOR: 1
    volumes:
      - kafka-data:/var/lib/kafka/data
    healthcheck:
      test: nc -z localhost 9092 || exit 1
      interval: 15s
      timeout: 5s
      retries: 5

  # ─── Application Services ───────────────────────────
  order-api:
    build:
      context: .
      dockerfile: Dockerfile.order-api
    container_name: food-order-api
    networks:
      - food-delivery-net
    ports:
      - "8080:8080"
      - "5001:5001"   # gRPC
    environment:
      ASPNETCORE_ENVIRONMENT: Docker
      ASPNETCORE_URLS: "http://+:8080"
      ConnectionStrings__Default: "Server=sql-server,1433;Database=FoodDelivery;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
      RabbitMQ__Host: rabbitmq
      RabbitMQ__Username: guest
      RabbitMQ__Password: guest
      Kafka__BootstrapServers: kafka:9092
    depends_on:
      sql-server:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy
      kafka:
        condition: service_healthy
    restart: unless-stopped

  order-consumers:
    build:
      context: .
      dockerfile: Dockerfile.order-consumers
    container_name: food-order-consumers
    networks:
      - food-delivery-net
    environment:
      ASPNETCORE_ENVIRONMENT: Docker
      ConnectionStrings__Default: "Server=sql-server,1433;Database=FoodDelivery;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
      RabbitMQ__Host: rabbitmq
      RabbitMQ__Username: guest
      RabbitMQ__Password: guest
      PartnerApi__Key: ""     # Set via .env file
      PartnerApi__BaseUrl: "https://api.delivery-partner.com"
    depends_on:
      sql-server:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy
    restart: unless-stopped
    # Consumers don't need ports — they only make outbound connections
    deploy:
      replicas: 2       # Run 2 consumer instances for parallelism

  analytics-consumer:
    build:
      context: .
      dockerfile: Dockerfile.analytics-consumer
    container_name: food-analytics-consumer
    networks:
      - food-delivery-net
    environment:
      Kafka__BootstrapServers: kafka:9092
      Warehouse__ConnectionString: "Server=sql-server,1433;Database=FoodWarehouse;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
    depends_on:
      kafka:
        condition: service_healthy
    restart: unless-stopped

  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile.frontend
    container_name: food-frontend
    networks:
      - food-delivery-net
    ports:
      - "80:80"         # Entry point for users
    depends_on:
      - order-api
    restart: unless-stopped

  # ─── Development Tools ──────────────────────────────
  rabbitmq-management:
    image: rabbitmq:3-management-alpine
    container_name: food-rabbitmq-ui
    networks:
      - food-delivery-net
    ports:
      - "15672:15672"
    profiles:
      - dev-only      # Only start with: docker-compose --profile dev-only up

  kafka-ui:
    image: provectuslabs/kafka-ui:latest
    container_name: food-kafka-ui
    networks:
      - food-delivery-net
    ports:
      - "8081:8080"
    environment:
      KAFKA_CLUSTERS_0_NAME: local
      KAFKA_CLUSTERS_0_BOOTSTRAPSERVERS: kafka:9092
    profiles:
      - dev-only
```

**Usage:**
```bash
# Start everything
docker-compose up -d

# Start only app services (no admin UIs)
docker-compose --profile "" up -d

# Scale consumers
docker-compose up -d --scale order-consumers=5

# View logs
docker-compose logs -f order-api order-consumers

# Tear down
docker-compose down -v
```

---

### 3. Azure DevOps CI/CD Pipeline

```yaml
# azure-pipelines.yml
# ──────────────────────────────────────────────────────
# Full CI/CD: Build → Test → Push to ACR → Deploy to AKS
# Trigger: any push to main or PR to main
# ──────────────────────────────────────────────────────

trigger:
  branches:
    include:
      - main
      - develop
  paths:
    include:
      - src/*                # Only trigger for source code changes
      - Dockerfile.*
      - azure-pipelines.yml

pr:
  branches:
    include:
      - main

variables:
  # ─── Azure Resources ────────────────────────────────
  azureSubscription: 'FoodDelivery-ServiceConnection'
  containerRegistry: 'fooddeliveryacr.azurecr.io'
  dockerRegistryServiceConnection: 'FoodDelivery-ACR-Connection'

  # ─── AKS ────────────────────────────────────────────
  kubernetesServiceConnection: 'FoodDelivery-AKS-Connection'
  aksNamespace: 'food-delivery'

  # ─── Image Tags ─────────────────────────────────────
  imageTag: $(Build.BuildId)              # Unique per build
  latestTag: latest                       # Overwritten per branch

  # ─── Dockerfiles ────────────────────────────────────
  apiDockerfile: 'src/FoodDelivery.Api/Dockerfile.order-api'
  consumersDockerfile: 'src/FoodDelivery.Consumers/Dockerfile.order-consumers'
  analyticsDockerfile: 'src/FoodDelivery.Analytics/Dockerfile.analytics-consumer'
  frontendDockerfile: 'src/frontend/Dockerfile.frontend'

stages:
  # ────────────────────────────────────────────────────
  # STAGE 1: Build & Test
  # ────────────────────────────────────────────────────
  - stage: Build
    displayName: 'Build & Test'
    jobs:
      - job: BuildAndTest
        displayName: 'Build, Test, and Push Docker Images'
        pool:
          vmImage: ubuntu-latest
        steps:
          # ── Restore & Build ──────────────────────────
          - task: DotNetCoreCLI@2
            displayName: 'Restore NuGet packages'
            inputs:
              command: 'restore'
              projects: 'src/FoodDelivery.sln'

          - task: DotNetCoreCLI@2
            displayName: 'Build solution'
            inputs:
              command: 'build'
              projects: 'src/FoodDelivery.sln'
              arguments: '--configuration Release --no-restore'

          # ── Run Tests ────────────────────────────────
          - task: DotNetCoreCLI@2
            displayName: 'Run unit tests'
            inputs:
              command: 'test'
              projects: 'tests/**/*.csproj'
              arguments: '--configuration Release --no-build --collect "XPlat Code Coverage"'

          - task: PublishTestResults@2
            displayName: 'Publish test results'
            inputs:
              testResultsFormat: 'VSTest'
              testResultsFiles: '**/*.trx'
              mergeTestResults: true
            condition: succeededOrFailed()

          - task: PublishCodeCoverageResults@2
            displayName: 'Publish code coverage'
            inputs:
              summaryFileLocation: '**/coverage.cobertura.xml'
              failIfCoverageEmpty: true

          # ── Docker Build & Push ──────────────────────
          # Order API
          - task: Docker@2
            displayName: 'Build & Push order-api'
            inputs:
              containerRegistry: '$(dockerRegistryServiceConnection)'
              repository: 'food-delivery/order-api'
              command: 'buildAndPush'
              Dockerfile: '$(apiDockerfile)'
              tags: |
                $(imageTag)
                $(latestTag)
              buildContext: 'src'

          # Order Consumers
          - task: Docker@2
            displayName: 'Build & Push order-consumers'
            inputs:
              containerRegistry: '$(dockerRegistryServiceConnection)'
              repository: 'food-delivery/order-consumers'
              command: 'buildAndPush'
              Dockerfile: '$(consumersDockerfile)'
              tags: |
                $(imageTag)
                $(latestTag)
              buildContext: 'src'

          # Analytics Consumer
          - task: Docker@2
            displayName: 'Build & Push analytics-consumer'
            inputs:
              containerRegistry: '$(dockerRegistryServiceConnection)'
              repository: 'food-delivery/analytics-consumer'
              command: 'buildAndPush'
              Dockerfile: '$(analyticsDockerfile)'
              tags: |
                $(imageTag)
                $(latestTag)
              buildContext: 'src'

          # Frontend
          - task: Docker@2
            displayName: 'Build & Push frontend'
            inputs:
              containerRegistry: '$(dockerRegistryServiceConnection)'
              repository: 'food-delivery/frontend'
              command: 'buildAndPush'
              Dockerfile: '$(frontendDockerfile)'
              tags: |
                $(imageTag)
                $(latestTag)
              buildContext: 'src/frontend'

          # ── Publish Build Artifacts ──────────────────
          - task: PublishBuildArtifacts@1
            displayName: 'Publish K8s manifests'
            inputs:
              pathToPublish: 'deploy/kubernetes'
              artifactName: 'k8s-manifests'
            condition: succeeded()

  # ────────────────────────────────────────────────────
  # STAGE 2: Deploy to Development AKS
  # ────────────────────────────────────────────────────
  - stage: DeployDev
    displayName: 'Deploy to Dev AKS'
    dependsOn: Build
    condition: succeeded()
    jobs:
      - deployment: DeployToDevAKS
        displayName: 'Deploy to AKS (dev)'
        pool:
          vmImage: ubuntu-latest
        environment: 'food-delivery-dev'
        strategy:
          blueGreen:        # Zero-downtime deployment
            preDeploy:
              steps:
                - task: Kubernetes@1
                  displayName: 'Verify cluster connectivity'
                  inputs:
                    connectionType: 'Kubernetes Service Connection'
                    kubernetesServiceEndpoint: '$(kubernetesServiceConnection)'
                    namespace: '$(aksNamespace)'
                    command: 'get'
                    arguments: 'nodes'

            deploy:
              steps:
                - task: HelmDeploy@0
                  displayName: 'Deploy with Helm'
                  inputs:
                    connectionType: 'Kubernetes Service Connection'
                    kubernetesServiceEndpoint: '$(kubernetesServiceConnection)'
                    namespace: '$(aksNamespace)'
                    command: 'upgrade'
                    chartType: 'FilePath'
                    chartPath: 'deploy/helm/food-delivery'
                    releaseName: 'food-delivery'
                    overrideValues: |
                      images.orderApi.tag=$(imageTag)
                      images.orderConsumers.tag=$(imageTag)
                      images.analyticsConsumer.tag=$(imageTag)
                      images.frontend.tag=$(imageTag)
                      environment=dev
                      imagePullSecrets.registry=$(containerRegistry)
                    valueFile: 'deploy/helm/food-delivery/values-dev.yaml'

            routeTraffic:
              steps:
                - script: |
                    echo "Traffic routed to new pods. Monitoring for 5 minutes..."
                    sleep 300
                  displayName: 'Warm-up and monitor'

            postRouteTraffic:
              steps:
                - task: Kubernetes@1
                  displayName: 'Run smoke tests'
                  inputs:
                    connectionType: 'Kubernetes Service Connection'
                    kubernetesServiceEndpoint: '$(kubernetesServiceConnection)'
                    namespace: '$(aksNamespace)'
                    command: 'exec'
                    arguments: 'deploy/smoke-tester -- curl -f http://order-api:8080/health'

            on:
              failure:
                steps:
                  - script: |
                      echo "Deployment failed — initiating rollback"
                      helm rollback food-delivery
                    displayName: 'Rollback on failure'

  # ────────────────────────────────────────────────────
  # STAGE 3: Deploy to Production AKS (manual approval)
  # ────────────────────────────────────────────────────
  - stage: DeployProd
    displayName: 'Deploy to Production AKS'
    dependsOn: DeployDev
    condition: succeeded()
    jobs:
      - deployment: DeployToProdAKS
        displayName: 'Deploy to AKS (prod)'
        pool:
          vmImage: ubuntu-latest
        environment: 'food-delivery-prod'
        strategy:
          canary:           # 10% → 50% → 100% traffic shift
            increments: [10, 50, 100]
            preDeploy:
              steps:
                - script: |
                    echo "⚠️  Manual approval required before production deployment"
                    echo "Check Azure Boards work item for sign-off"
                  displayName: 'Pre-deployment checklist'

            deploy:
              steps:
                - task: HelmDeploy@0
                  displayName: 'Deploy with Helm'
                  inputs:
                    connectionType: 'Kubernetes Service Connection'
                    kubernetesServiceEndpoint: '$(kubernetesServiceConnection)'
                    namespace: '$(aksNamespace)'
                    command: 'upgrade'
                    chartType: 'FilePath'
                    chartPath: 'deploy/helm/food-delivery'
                    releaseName: 'food-delivery-prod'
                    overrideValues: |
                      images.orderApi.tag=$(imageTag)
                      images.orderConsumers.tag=$(imageTag)
                      images.analyticsConsumer.tag=$(imageTag)
                      images.frontend.tag=$(imageTag)
                      environment=prod
                      replicas.orderApi=5
                      replicas.orderConsumers=10
                      replicas.analyticsConsumer=3
                      replicas.frontend=3
                    valueFile: 'deploy/helm/food-delivery/values-prod.yaml'

            routeTraffic:
              steps:
                - script: |
                    echo "Canary: $(increment)% traffic routed to new version"
                  displayName: 'Monitor canary'

            postRouteTraffic:
              steps:
                - task: AzureMonitor@1
                  displayName: 'Verify no error spike'
                  inputs:
                    connectedServiceNameARM: '$(azureSubscription)'
                    ResourceGroupName: 'food-delivery-rg'
                    filterType: 'metrics'
                    alertRules: 'food-delivery-prod-error-rate'

            on:
              failure:
                steps:
                  - script: helm rollback food-delivery-prod
                    displayName: 'Rollback on canary failure'
```

---

### 4. Kubernetes Deployment Manifests

For teams using raw YAML instead of Helm:

#### 4a. order-api Deployment + Service

```yaml
# deploy/kubernetes/order-api.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: order-api
  namespace: food-delivery
  labels:
    app: food-delivery
    service: order-api
spec:
  replicas: 3
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxUnavailable: 0       # Zero-downtime: kill old AFTER new is ready
      maxSurge: 1
  selector:
    matchLabels:
      service: order-api
  template:
    metadata:
      labels:
        service: order-api
    spec:
      # Pull image from ACR
      imagePullSecrets:
        - name: acr-secret
      containers:
        - name: order-api
          image: fooddeliveryacr.azurecr.io/food-delivery/order-api:latest
          imagePullPolicy: Always
          ports:
            - containerPort: 8080
              name: http
            - containerPort: 5001
              name: grpc
          env:
            - name: ASPNETCORE_ENVIRONMENT
              value: "Production"
            - name: ASPNETCORE_URLS
              value: "http://+:8080"
            - name: ConnectionStrings__Default
              valueFrom:
                secretKeyRef:
                  name: food-delivery-secrets
                  key: sql-connection-string
            - name: RabbitMQ__Host
              value: "food-rabbitmq.namespace.svc.cluster.local"
            - name: RabbitMQ__Username
              valueFrom:
                secretKeyRef:
                  name: food-delivery-secrets
                  key: rabbitmq-username
            - name: RabbitMQ__Password
              valueFrom:
                secretKeyRef:
                  name: food-delivery-secrets
                  key: rabbitmq-password
            - name: Kafka__BootstrapServers
              valueFrom:
                configMapKeyRef:
                  name: food-delivery-config
                  key: kafka-bootstrap-servers
            - name: Kafka__SaslUsername
              valueFrom:
                secretKeyRef:
                  name: food-delivery-secrets
                  key: kafka-sasl-username
            - name: Kafka__SaslPassword
              valueFrom:
                secretKeyRef:
                  name: food-delivery-secrets
                  key: kafka-sasl-password
          resources:
            requests:
              cpu: "250m"
              memory: "256Mi"
            limits:
              cpu: "500m"
              memory: "512Mi"
          livenessProbe:
            httpGet:
              path: /health
              port: 8080
            initialDelaySeconds: 10
            periodSeconds: 30
          readinessProbe:
            httpGet:
              path: /health/ready
              port: 8080
            initialDelaySeconds: 5
            periodSeconds: 15
---
apiVersion: v1
kind: Service
metadata:
  name: order-api
  namespace: food-delivery
spec:
  selector:
    service: order-api
  ports:
    - name: http
      port: 8080
      targetPort: 8080
    - name: grpc
      port: 5001
      targetPort: 5001
  type: ClusterIP
```

#### 4b. order-consumers Deployment

```yaml
# deploy/kubernetes/order-consumers.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: order-consumers
  namespace: food-delivery
  labels:
    app: food-delivery
    service: order-consumers
spec:
  replicas: 5                    # More replicas = more parallel message processing
  strategy:
    type: RollingUpdate
  selector:
    matchLabels:
      service: order-consumers
  template:
    metadata:
      labels:
        service: order-consumers
    spec:
      imagePullSecrets:
        - name: acr-secret
      containers:
        - name: order-consumers
          image: fooddeliveryacr.azurecr.io/food-delivery/order-consumers:latest
          imagePullPolicy: Always
          # No ports — consumers only make outbound connections
          env:
            - name: ASPNETCORE_ENVIRONMENT
              value: "Production"
            - name: ConnectionStrings__Default
              valueFrom:
                secretKeyRef:
                  name: food-delivery-secrets
                  key: sql-connection-string
            - name: RabbitMQ__Host
              value: "food-rabbitmq.namespace.svc.cluster.local"
            - name: RabbitMQ__Username
              valueFrom:
                secretKeyRef:
                  name: food-delivery-secrets
                  key: rabbitmq-username
            - name: RabbitMQ__Password
              valueFrom:
                secretKeyRef:
                  name: food-delivery-secrets
                  key: rabbitmq-password
            - name: PartnerApi__Key
              valueFrom:
                secretKeyRef:
                  name: food-delivery-secrets
                  key: partner-api-key
          resources:
            requests:
              cpu: "200m"
              memory: "256Mi"
            limits:
              cpu: "1"        # Consumers are CPU-bound (message processing)
              memory: "512Mi"
          # No HTTP health check — use TCP or exec probe
          livenessProbe:
            exec:
              command:
                - dotnet
                - FoodDelivery.Consumers.dll
                - --healthcheck
            initialDelaySeconds: 15
            periodSeconds: 30
```

#### 4c. analytics-consumer Deployment

```yaml
# deploy/kubernetes/analytics-consumer.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: analytics-consumer
  namespace: food-delivery
  labels:
    app: food-delivery
    service: analytics-consumer
spec:
  replicas: 2
  selector:
    matchLabels:
      service: analytics-consumer
  template:
    metadata:
      labels:
        service: analytics-consumer
    spec:
      imagePullSecrets:
        - name: acr-secret
      containers:
        - name: analytics-consumer
          image: fooddeliveryacr.azurecr.io/food-delivery/analytics-consumer:latest
          imagePullPolicy: Always
          env:
            - name: Kafka__BootstrapServers
              valueFrom:
                configMapKeyRef:
                  name: food-delivery-config
                  key: kafka-bootstrap-servers
            - name: Kafka__SaslUsername
              valueFrom:
                secretKeyRef:
                  name: food-delivery-secrets
                  key: kafka-sasl-username
            - name: Kafka__SaslPassword
              valueFrom:
                secretKeyRef:
                  name: food-delivery-secrets
                  key: kafka-sasl-password
            - name: Warehouse__ConnectionString
              valueFrom:
                secretKeyRef:
                  name: food-delivery-secrets
                  key: warehouse-connection-string
          resources:
            requests:
              cpu: "300m"
              memory: "512Mi"
            limits:
              cpu: "1"
              memory: "1Gi"
```

#### 4d. frontend Deployment + Service (LoadBalancer)

```yaml
# deploy/kubernetes/frontend.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: frontend
  namespace: food-delivery
  labels:
    app: food-delivery
    service: frontend
spec:
  replicas: 2
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxUnavailable: 0
      maxSurge: 1
  selector:
    matchLabels:
      service: frontend
  template:
    metadata:
      labels:
        service: frontend
    spec:
      imagePullSecrets:
        - name: acr-secret
      containers:
        - name: frontend
          image: fooddeliveryacr.azurecr.io/food-delivery/frontend:latest
          imagePullPolicy: Always
          ports:
            - containerPort: 80
          resources:
            requests:
              cpu: "100m"
              memory: "128Mi"
            limits:
              cpu: "200m"
              memory: "256Mi"
          livenessProbe:
            httpGet:
              path: /
              port: 80
            initialDelaySeconds: 10
            periodSeconds: 30
          readinessProbe:
            httpGet:
              path: /
              port: 80
            initialDelaySeconds: 5
            periodSeconds: 15
---
apiVersion: v1
kind: Service
metadata:
  name: frontend
  namespace: food-delivery
  annotations:
    service.beta.kubernetes.io/azure-load-balancer-health-probe-request-path: "/health"
spec:
  selector:
    service: frontend
  ports:
    - port: 80
      targetPort: 80
  type: LoadBalancer    # Public IP for users to access the app
```

#### 4e. Secrets & ConfigMap

```yaml
# deploy/kubernetes/secrets.yaml
# ──────────────────────────────────────────────────────
# ⚠️  NEVER commit real secrets to git!
#     Use Azure Key Vault Provider for Secrets Store CSI Driver instead.
#     This file is a TEMPLATE showing the structure.
# ──────────────────────────────────────────────────────
apiVersion: v1
kind: Secret
metadata:
  name: food-delivery-secrets
  namespace: food-delivery
type: Opaque
stringData:
  # Replace with your actual values during deployment
  sql-connection-string: "Server=mssql-clusterip;Database=FoodDelivery;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
  rabbitmq-username: "guest"
  rabbitmq-password: "guest"
  kafka-sasl-username: ""     # Set via CI/CD pipeline variables
  kafka-sasl-password: ""
  partner-api-key: ""
  warehouse-connection-string: ""
---
apiVersion: v1
kind: ConfigMap
metadata:
  name: food-delivery-config
  namespace: food-delivery
data:
  kafka-bootstrap-servers: "pkc-12345.us-east-1.aws.confluent.cloud:9092"
```

#### 4f. Ingress — Route traffic to frontend

```yaml
# deploy/kubernetes/ingress.yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: food-delivery-ingress
  namespace: food-delivery
  annotations:
    kubernetes.io/ingress.class: nginx
    cert-manager.io/cluster-issuer: letsencrypt-prod
spec:
  tls:
    - hosts:
        - fooddelivery.example.com
      secretName: food-delivery-tls
  rules:
    - host: fooddelivery.example.com
      http:
        paths:
          - path: /
            pathType: Prefix
            backend:
              service:
                name: frontend
                port:
                  number: 80
```

#### 4g. Horizontal Pod Autoscaler

```yaml
# deploy/kubernetes/hpa.yaml
# ──────────────────────────────────────────────────────
# Auto-scale services based on CPU and memory
# ──────────────────────────────────────────────────────
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: order-api-hpa
  namespace: food-delivery
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: order-api
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
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: order-consumers-hpa
  namespace: food-delivery
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: order-consumers
  minReplicas: 2
  maxReplicas: 20
  metrics:
    - type: Resource
      resource:
        name: cpu
        target:
          type: Utilization
          averageUtilization: 75
---
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: analytics-consumer-hpa
  namespace: food-delivery
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: analytics-consumer
  minReplicas: 1
  maxReplicas: 10
  metrics:
    - type: Resource
      resource:
        name: cpu
        target:
          type: Utilization
          averageUtilization: 70
```

---

### 5. Deploy Commands Cheatsheet

| Task | Command |
|------|---------|
| **Run locally** | `docker-compose up -d` |
| **Run with dev tools** | `docker-compose --profile dev-only up -d` |
| **Scale consumers** | `docker-compose up -d --scale order-consumers=5` |
| **Build all images** | `docker-compose build` |
| **Push to ACR** | `docker-compose push` (with `registry:` config) |
| **Deploy to AKS** | `kubectl apply -f deploy/kubernetes/` |
| **Deploy with Helm** | `helm upgrade --install food-delivery deploy/helm/food-delivery` |
| **Check pods** | `kubectl get pods -n food-delivery` |
| **View logs** | `kubectl logs -n food-delivery -l service=order-api -f` |
| **Scale in K8s** | `kubectl scale deployment order-consumers -n food-delivery --replicas=10` |
| **Rolling restart** | `kubectl rollout restart deployment order-api -n food-delivery` |
| **Rollback** | `kubectl rollout undo deployment order-api -n food-delivery` |
| **Run pipeline** | `az pipelines run --name "FoodDelivery-CI"` |

### 6. Architecture Diagram — Deployed

```
                            Internet
                               │
                               ▼
                    ┌───────────────────┐
                    │  Azure DNS / SSL  │
                    │  fooddelivery.com │
                    └────────┬──────────┘
                             │
                    ┌────────▼──────────┐
                    │  Azure Front Door  │  (CDN + WAF)
                    │  + Application GW │
                    └────────┬──────────┘
                             │
               ┌─────────────┴─────────────┐
               │     AKS Cluster            │
               │     (Kubernetes)           │
               │                            │
               │  ┌─────────────────────┐   │
               │  │  Ingress Controller │   │  (nginx)
               │  │  TLS termination    │   │
               │  └──────────┬──────────┘   │
               │             │              │
               │  ┌──────────▼──────────┐   │
               │  │  frontend Service   │   │  (LoadBalancer)
               │  │  SPA (nginx)        │   │
               │  └──────────┬──────────┘   │
               │             │              │
               │  ┌──────────▼──────────┐   │
               │  │  order-api Service  │   │  (ClusterIP)
               │  │  REST + gRPC + Hub  │   │
               │  └──────────┬──────────┘   │
               │             │              │
               │  ┌──────────┼──────────┐   │
               │  │ order-   │ analytics│   │
               │  │ consumers│ consumer │   │
               │  │ ×5 pods  │ ×2 pods  │   │
               │  └────┬─────┴────┬─────┘   │
               │       │          │          │
               │       ▼          ▼          │
               │  ┌────────┐ ┌──────────┐    │
               │  │RabbitMQ│ │  Kafka   │    │ (Managed cloud
               │  │(Cloud  │ │(Confluent│    │  services — NOT
               │  │ AMQP)  │ │  Cloud)  │    │  inside K8s)
               │  └────────┘ └──────────┘    │
               │                            │
               └────────────────────────────┘
                             │
               ┌─────────────┴─────────────┐
               │     Azure Resources        │
               │  ┌─────────────────────┐   │
               │  │  Azure SQL DB       │   │  (PaaS — managed)
               │  ├─────────────────────┤   │
               │  │  Azure Cache for    │   │
               │  │  Redis (SignalR     │   │
               │  │  backplane)         │   │
               │  ├─────────────────────┤   │
               │  │  Azure Key Vault    │   │  (Secrets)
               │  ├─────────────────────┤   │
               │  │  Azure Monitor      │   │  (Logs + Metrics)
               │  └─────────────────────┘   │
               └────────────────────────────┘

---

## Secret Key Management — All Cloud Options

Every cloud server needs secrets: database passwords, API keys, JWT signing keys, RabbitMQ/Kafka credentials, TLS certificates. **Never hard-code them.** Here's how every major cloud does it.

### The Problem

```csharp
// ❌ NEVER DO THIS
var connectionString = "Server=prod-db;Database=FoodDelivery;User Id=sa;Password=SuperSecret123!";
var apiKey = "sk-proj-abc123def456";
var jwtSecret = "my-super-secret-key-that-should-not-be-in-code";
```

Instead, secrets live in a **dedicated secret store** and are injected at runtime.

---

### 1. Azure Key Vault

**Best for:** Azure-hosted apps (AKS, App Service, Functions, VMs)

#### Setup

```bash
# 1. Create Key Vault
az keyvault create \
  --name fooddelivery-kv \
  --resource-group food-delivery-rg \
  --location eastus \
  --sku Standard

# 2. Store secrets
az keyvault secret set \
  --vault-name fooddelivery-kv \
  --name "sql-connection-string" \
  --value "Server=prod-db;Database=FoodDelivery;User Id=sa;Password=RealPass123!"

az keyvault secret set \
  --name "rabbitmq-password" \
  --value "guest"

az keyvault secret set \
  --name "jwt-signing-key" \
  --value "MIICWwIBAAKBgHu8j6...base64-encoded-cert..."

# 3. Grant your app access (Managed Identity)
az keyvault set-policy \
  --name fooddelivery-kv \
  --object-id $(az aks show -n food-aks -g food-rg --query identityProfile.kubeletidentity.objectId -o tsv) \
  --secret-permissions get list
```

#### ASP.NET Core Integration (Simplest)

```csharp
// Program.cs — Azure Key Vault integration
builder.Configuration.AddAzureKeyVault(
    new Uri("https://fooddelivery-kv.vault.azure.net/"),
    new DefaultAzureCredential());  // Uses Managed Identity automatically

// Now use like normal config:
//   builder.Configuration["sql-connection-string"]
//   builder.Configuration["rabbitmq-password"]
```

#### AKS + Azure Key Vault CSI Driver (Kubernetes-native)

```yaml
# Method 1: Secrets Store CSI Driver — mounts secrets as volumes
# Prerequisite: Install the CSI driver on AKS
apiVersion: secrets-store.csi.x-k8s.io/v1
kind: SecretProviderClass
metadata:
  name: food-delivery-azure-kv
  namespace: food-delivery
spec:
  provider: azure
  parameters:
    usePodIdentity: "true"
    keyvaultName: fooddelivery-kv
    objects: |
      array:
        - objectName: sql-connection-string
          objectType: secret
        - objectName: rabbitmq-password
          objectType: secret
        - objectName: jwt-signing-key
          objectType: secret
    tenantId: "your-tenant-id"
---
# Pod mounts secrets as files at /mnt/secrets
apiVersion: apps/v1
kind: Deployment
metadata:
  name: order-api
spec:
  template:
    metadata:
      labels:
        service: order-api
    spec:
      containers:
        - name: order-api
          volumeMounts:
            - name: secrets-store
              mountPath: "/mnt/secrets"
              readOnly: true
      volumes:
        - name: secrets-store
          csi:
            driver: secrets-store.csi.k8s.io
            readOnly: true
            volumeAttributes:
              secretProviderClass: food-delivery-azure-kv
```

```yaml
# Method 2: Use External Secrets Operator (syncs Key Vault → K8s Secrets)
apiVersion: external-secrets.io/v1beta1
kind: ExternalSecret
metadata:
  name: food-delivery-secrets
  namespace: food-delivery
spec:
  refreshInterval: "1h"                          # Auto-refresh every hour
  secretStoreRef:
    name: azure-kv-store
    kind: SecretStore
  target:
    name: food-delivery-k8s-secret               # Creates a K8s Secret
    creationPolicy: Owner
  data:
    - secretKey: sql-connection-string
      remoteRef:
        key: sql-connection-string
    - secretKey: rabbitmq-password
      remoteRef:
        key: rabbitmq-password
---
apiVersion: external-secrets.io/v1beta1
kind: SecretStore
metadata:
  name: azure-kv-store
  namespace: food-delivery
spec:
  provider:
    azurekv:
      vaultUrl: "https://fooddelivery-kv.vault.azure.net/"
      authType: ManagedIdentity
      vaultUrl: "https://fooddelivery-kv.vault.azure.net/"
```

---

### 2. AWS Secrets Manager & Parameter Store

**Best for:** AWS-hosted apps (EKS, ECS, Lambda, EC2)

#### AWS Secrets Manager (for passwords, API keys, connection strings)

```bash
# 1. Store a secret
aws secretsmanager create-secret \
  --name food-delivery/sql-connection-string \
  --secret-string '{"connectionString":"Server=prod-db;Database=FoodDelivery;User Id=admin;Password=RealPass123!"}'

aws secretsmanager create-secret \
  --name food-delivery/rabbitmq-password \
  --secret-string 'guest'

aws secretsmanager create-secret \
  --name food-delivery/jwt-signing-key \
  --secret-string 'MIICWwIBAAKB...'

# 2. Grant access via IAM role
# Attach this policy to your EKS node role / ECS task role:
# {
#   "Effect": "Allow",
#   "Action": ["secretsmanager:GetSecretValue"],
#   "Resource": "arn:aws:secretsmanager:us-east-1:123456789012:secret:food-delivery/*"
# }
```

#### AWS Parameter Store (free, for config values)

```bash
aws ssm put-parameter \
  --name /food-delivery/kafka-bootstrap-servers \
  --value "pkc-12345.us-east-1.aws.confluent.cloud:9092" \
  --type String

aws ssm put-parameter \
  --name /food-delivery/partner-api-key \
  --value "sk-proj-abc123" \
  --type SecureString   # Encrypted at rest
```

#### ASP.NET Core Integration (with AWS SDK)

```csharp
// Install: dotnet add package Amazon.Extensions.Configuration.SystemsManager

// Program.cs
builder.Configuration.AddSystemsManager("/food-delivery");  // Loads from Parameter Store

// For Secrets Manager, use a custom provider or AWS SDK directly:
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

var client = new AmazonSecretsManagerClient();
var response = await client.GetSecretValueAsync(new GetSecretValueRequest
{
    SecretId = "food-delivery/sql-connection-string"
});
var secret = JsonSerializer.Deserialize<Dictionary<string, string>>(response.SecretString);
var connectionString = secret["connectionString"];
```

#### EKS + External Secrets Operator (AWS)

```yaml
apiVersion: external-secrets.io/v1beta1
kind: SecretStore
metadata:
  name: aws-ssm-store
  namespace: food-delivery
spec:
  provider:
    aws:
      service: ParameterStore        # or SecretsManager
      region: us-east-1
      auth:
        jwt:
          serviceAccountRef:
            name: food-delivery-sa   # K8s service account with IAM role
---
apiVersion: external-secrets.io/v1beta1
kind: ExternalSecret
metadata:
  name: food-delivery-aws-secrets
  namespace: food-delivery
spec:
  refreshInterval: "1h"
  secretStoreRef:
    name: aws-ssm-store
  target:
    name: food-delivery-k8s-secret
  data:
    - secretKey: sql-connection-string
      remoteRef:
        key: /food-delivery/sql-connection-string
    - secretKey: rabbitmq-password
      remoteRef:
        key: /food-delivery/rabbitmq-password
    - secretKey: kafka-bootstrap-servers
      remoteRef:
        key: /food-delivery/kafka-bootstrap-servers
```

#### Direct Lambda / ECS Access

```csharp
// In Lambda or ECS task — no K8s needed, IAM role handles auth
public class SecretsController : ControllerBase
{
    private readonly IAmazonSecretsManager _secretsManager;

    public SecretsController(IAmazonSecretsManager sm) => _secretsManager = sm;

    [HttpGet("init")]
    public async Task<IActionResult> InitFromSecrets()
    {
        var response = await _secretsManager.GetSecretValueAsync(new GetSecretValueRequest
        {
            SecretId = "food-delivery/sql-connection-string"
        });

        var secret = JsonSerializer.Deserialize<Dictionary<string, string>>(response.SecretString);
        _db.InitConnection(secret["connectionString"]);

        return Ok("Initialized from AWS Secrets Manager");
    }
}
```

---

### 3. Google Cloud Secret Manager

**Best for:** GCP-hosted apps (GKE, Cloud Run, Compute Engine)

#### Setup

```bash
# 1. Enable the API
gcloud services enable secretmanager.googleapis.com

# 2. Store secrets
echo -n "Server=prod-db;Database=FoodDelivery;User Id=admin;Password=RealPass123!" | \
  gcloud secrets create sql-connection-string --data-file=-
echo -n "guest" | \
  gcloud secrets create rabbitmq-password --data-file=-
echo -n "MIICWwIBAAKB..." | \
  gcloud secrets create jwt-signing-key --data-file=-

# 3. Grant access to your GKE service account
gcloud secrets add-iam-policy-binding sql-connection-string \
  --member="serviceAccount:food-delivery-sa@project.iam.gserviceaccount.com" \
  --role="roles/secretmanager.secretAccessor"
```

#### ASP.NET Core Integration

```csharp
// Install: dotnet add package Google.Cloud.SecretManager.V1

using Google.Cloud.SecretManager.V1;

var client = await SecretManagerServiceClient.CreateAsync();
var secretName = new SecretVersionName("your-project-id", "sql-connection-string", "latest");
var response = await client.AccessSecretVersionAsync(secretName);
var connectionString = response.Payload.Data.ToStringUtf8();

// Use it
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlServer(connectionString));
```

#### GKE + External Secrets Operator

```yaml
apiVersion: external-secrets.io/v1beta1
kind: SecretStore
metadata:
  name: gcp-secret-store
  namespace: food-delivery
spec:
  provider:
    gcpsm:
      projectID: your-gcp-project-id
      auth:
        workloadIdentity:
          clusterLocation: us-central1
          clusterName: food-gke-cluster
---
apiVersion: external-secrets.io/v1beta1
kind: ExternalSecret
metadata:
  name: food-delivery-gcp-secrets
  namespace: food-delivery
spec:
  refreshInterval: "1h"
  secretStoreRef:
    name: gcp-secret-store
  target:
    name: food-delivery-k8s-secret
  data:
    - secretKey: sql-connection-string
      remoteRef:
        key: sql-connection-string
        version: latest
    - secretKey: rabbitmq-password
      remoteRef:
        key: rabbitmq-password
        version: latest
```

#### Cloud Run — Inject Secrets as Environment Variables

```yaml
# cloud-run.yaml
apiVersion: serving.knative.dev/v1
kind: Service
metadata:
  name: food-delivery-api
spec:
  template:
    spec:
      containers:
        - image: gcr.io/your-project/food-delivery-api:latest
          env:
            - name: ConnectionStrings__Default
              valueFrom:
                secretKeyRef:
                  name: sql-connection-string   # References Secret Manager
                  key: latest
            - name: RabbitMQ__Password
              valueFrom:
                secretKeyRef:
                  name: rabbitmq-password
                  key: latest
```

---

### 4. Kubernetes Secrets (Native)

**Best for:** Simple deployments, dev/staging, non-cloud-specific

#### Imperative

```bash
# Literal values
kubectl create secret generic food-delivery-secrets \
  --from-literal=sql-connection-string='Server=prod-db;Database=FoodDelivery;User Id=sa;Password=RealPass123!' \
  --from-literal=rabbitmq-password='guest' \
  --from-literal=partner-api-key='sk-proj-abc123' \
  -n food-delivery

# From files (e.g., TLS certs, .env files)
kubectl create secret tls food-delivery-tls \
  --cert=tls.crt --key=tls.key -n food-delivery

kubectl create secret generic env-secrets \
  --from-file=.env.production -n food-delivery
```

#### Declarative (for infrastructure-as-code)

```yaml
# deploy/kubernetes/secrets.yaml
apiVersion: v1
kind: Secret
metadata:
  name: food-delivery-secrets
  namespace: food-delivery
type: Opaque
# ⚠️  These values are base64-encoded, NOT encrypted!
# base64 is encoding, not encryption — anyone with kubectl access can decode them.
data:
  sql-connection-string: U2VydmVyPXByb2QtZGI7RGF0YWJhc2U9Rm9vZERlbGl2ZXJ5O1VzZXIgSWQ9c2E7UGFzc3dvcmQ9UmVhbFBhc3MxMjMh
  rabbitmq-password: Z3Vlc3Q=
```

**⚠️  K8s Secrets are NOT secure by default:**
- Values are only base64-encoded (not encrypted) in etcd
- Anyone with `get` permission on secrets can decode them
- **Always enable encryption at rest** in production

#### Enable Encryption at Rest in Kubernetes

```yaml
# encryption-config.yaml
apiVersion: apiserver.config.k8s.io/v1
kind: EncryptionConfiguration
resources:
  - resources:
      - secrets
    providers:
      - aescbc:
          keys:
            - name: key1
              secret: c2VjcmV0LXNpbmNlLTIwMjQ=
      - identity: {}    # Fallback (if AES key rotation fails)
```

```bash
# On control plane nodes:
# 1. Move the config to /etc/kubernetes/encryption-config.yaml
# 2. Update kube-apiserver manifest to include:
#    --encryption-provider-config=/etc/kubernetes/encryption-config.yaml
```

#### Mounting Secrets in Pods

```yaml
# As environment variables (most common)
apiVersion: apps/v1
kind: Deployment
spec:
  template:
    spec:
      containers:
        - name: order-api
          env:
            - name: ConnectionStrings__Default
              valueFrom:
                secretKeyRef:
                  name: food-delivery-secrets
                  key: sql-connection-string
            - name: RabbitMQ__Password
              valueFrom:
                secretKeyRef:
                  name: food-delivery-secrets
                  key: rabbitmq-password
---
# OR: As mounted files (best for large secrets like TLS certs)
# Mounts to /mnt/secrets/sql-connection-string, /mnt/secrets/rabbitmq-password
          volumeMounts:
            - name: secret-volume
              mountPath: "/mnt/secrets"
              readOnly: true
      volumes:
        - name: secret-volume
          secret:
            secretName: food-delivery-secrets
```

---

### 5. HashiCorp Vault (Cloud-Agnostic)

**Best for:** Multi-cloud, on-premises, or when you need dynamic secrets (auto-rotating DB credentials)

#### Architecture

```
┌──────────────────┐     ┌─────────────────────────────┐
│   Application    │────►│  HashiCorp Vault             │
│   (Pods/VM)      │     │                              │
│                  │     │  ┌────────────────────────┐  │
│  Requests secret │     │  │  Secrets Engine        │  │
│  at startup      │     │  │  - KV v2 (static)      │  │
│                  │     │  │  - Database (dynamic)   │  │
│  Receives:       │     │  │  - PKI (certificates)   │  │
│  - DB password   │     │  └────────────────────────┘  │
│  - API key       │     │                              │
└──────────────────┘     └──────────────────────────────┘
```

#### Install Vault (Dev mode for testing)

```bash
# Docker
docker run -d --name vault -p 8200:8200 \
  -e VAULT_DEV_ROOT_TOKEN_ID=root-token \
  vault:latest

# Access UI: http://localhost:8200 (token: root-token)
```

#### Store & Retrieve Secrets

```bash
# Enable KV secrets engine
vault secrets enable -path=food-delivery kv-v2

# Write secrets
vault kv put food-delivery/order-api \
  sql-connection-string="Server=prod-db;Database=FoodDelivery;User Id=sa;Password=RealPass123!" \
  rabbitmq-password="guest" \
  partner-api-key="sk-proj-abc123"

# Read secrets
vault kv get food-delivery/order-api
```

#### ASP.NET Core Integration

```csharp
// Install: dotnet add package VaultSharp

using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;

// Configure Vault client
var vaultClient = new VaultClient(new VaultClientSettings(
    "http://vault:8200",
    new TokenAuthMethodInfo("root-token")
));

// Read secrets at application startup
var secret = await vaultClient.V1.Secrets.KeyValueV2.ReadSecretAsync("food-delivery/order-api");
var connectionString = secret.Data.Data["sql-connection-string"].ToString();
var rabbitPwd = secret.Data.Data["rabbitmq-password"].ToString();

// Inject into config
builder.Configuration["ConnectionStrings:Default"] = connectionString;
builder.Configuration["RabbitMQ:Password"] = rabbitPwd;
```

#### Dynamic Database Secrets (Auto-Rotating)

```csharp
// Vault can generate temp DB credentials that auto-expire!
// This is the highest security: no permanent DB password anywhere.

// 1. Configure Vault DB engine
vault secrets enable database
vault write database/config/sql-server \
  plugin_name="mssql-database-plugin" \
  connection_url="Server=prod-db;Database=FoodDelivery;User Id=sa;Password=MasterPassword;" \
  allowed_roles="food-delivery-role"

vault write database/roles/food-delivery-role \
  db_name=sql-server \
  creation_statements="CREATE LOGIN [{{name}}] WITH PASSWORD = '{{password}}';CREATE USER [{{name}}] FOR LOGIN [{{name}}];" \
  default_ttl="1h" \      # Password expires in 1 hour
  max_ttl="24h"

// 2. In your app — request a temporary credential
var dynamicCreds = await vaultClient.V1.Secrets.Database.GetCredentialsAsync("food-delivery-role");
// Returns: { username: "vault-user-abc123", password: "random-password", lease_duration: 3600s }

// 3. Use it immediately — it expires in 1 hour
var connectionString = $"Server=prod-db;Database=FoodDelivery;User Id={dynamicCreds.Username};Password={dynamicCreds.Password};";

// 4. Your app must renew or re-request before expiry
_ = Task.Run(async () =>
{
    await Task.Delay(TimeSpan.FromMinutes(55));  // 5 min before expiry
    var newCreds = await vaultClient.V1.Secrets.Database.GetCredentialsAsync("food-delivery-role");
    // Swap connection string — old one is invalid now
});
```

#### Vault Agent Sidecar (Kubernetes)

```yaml
# Vault injects secrets via a sidecar container — no code changes needed
apiVersion: apps/v1
kind: Deployment
metadata:
  name: order-api
  annotations:
    vault.hashicorp.com/agent-inject: "true"
    vault.hashicorp.com/agent-inject-secret-connection-string: "food-delivery/order-api"
    vault.hashicorp.com/agent-inject-template-connection-string: |
      {{- with secret "food-delivery/order-api" -}}
      export ConnectionStrings__Default="{{ .Data.data.sql-connection-string }}"
      export RabbitMQ__Password="{{ .Data.data.rabbitmq-password }}"
      {{- end -}}
    vault.hashicorp.com/role: "food-delivery-app"
spec:
  template:
    metadata:
      labels:
        service: order-api
    spec:
      serviceAccountName: food-delivery-sa  # With Vault IAM role attached
      containers:
        - name: order-api
          # No code changes — Vault sidecar writes to shared volume
          # Secrets are at /vault/secrets/connection-string
```

---

### 6. GitHub Actions Secrets (CI/CD Only)

**Best for:** Injecting secrets during CI/CD pipeline runs (not runtime)

```yaml
# .github/workflows/deploy.yml
# Store secrets in GitHub: Settings → Secrets and variables → Actions
name: Deploy
on: [push]
jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Deploy to AKS
        run: |
          # Secrets injected as env vars — never visible in logs
          az login --service-principal \
            -u ${{ secrets.AZURE_CLIENT_ID }} \
            -p ${{ secrets.AZURE_CLIENT_SECRET }} \
            --tenant ${{ secrets.AZURE_TENANT_ID }}

          # Set K8s secrets from GitHub secrets
          kubectl create secret generic food-delivery-secrets \
            --from-literal=sql-connection-string="${{ secrets.SQL_CONNECTION_STRING }}" \
            --from-literal=rabbitmq-password="${{ secrets.RABBITMQ_PASSWORD }}" \
            --dry-run=client -o yaml | kubectl apply -f -
        env:
          # Mask these from logs
          SQL_CONNECTION_STRING: ${{ secrets.SQL_CONNECTION_STRING }}
          RABBITMQ_PASSWORD: ${{ secrets.RABBITMQ_PASSWORD }}
```

---

### 7. .NET User Secrets (Local Dev Only)

**Best for:** Local development — never production

```bash
# Initialize
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost;Database=FoodDelivery;User Id=sa;Password=DevPass123!"
dotnet user-secrets set "RabbitMQ:Password" "guest"

# Stored at (Windows):
# %APPDATA%\Microsoft\UserSecrets\<guid>\secrets.json
# Encrypted at rest on Windows, plaintext on Mac/Linux — dev only!

# ASP.NET Core loads this automatically in Development environment
```

```csharp
// Program.cs — this is automatic in Development:
// builder.Configuration.AddUserSecrets<Program>();
```

---

### 8. Comparison Matrix

| Feature | Azure Key Vault | AWS Secrets Manager | GCP Secret Manager | K8s Secrets | HashiCorp Vault |
|---------|---------------|-------------------|-------------------|-------------|-----------------|
| **Cost** | $0.03/10K ops | $0.40/secret/month | $0.06/version/month | Free | Free (OSS), $ (Enterprise) |
| **Encryption at rest** | ✅ AES-256 | ✅ AES-256 | ✅ AES-256 | ❌ base64 only* | ✅ AES-256 |
| **Auto-rotation** | ✅ (with Event Grid) | ✅ (built-in) | ✅ (via Cloud Scheduler) | ❌ | ✅ (dynamic secrets) |
| **Access control** | Azure RBAC | IAM policies | IAM roles | RBAC | ACL + policies |
| **Audit logging** | ✅ Azure Monitor | ✅ CloudTrail | ✅ Cloud Audit Logs | ❌ | ✅ Audit device |
| **Multi-cloud** | ❌ Azure only | ❌ AWS only | ❌ GCP only | ✅ Any K8s | ✅ Any platform |
| **Dynamic secrets** | ❌ | ❌ | ❌ | ❌ | ✅ (auto-expiring) |
| **K8s native** | CSI Driver / External Secrets | CSI / External Secrets | CSI / External Secrets | ✅ Native | Agent Sidecar |
| **Complexity** | Low | Low | Low | Low | Medium-High |
| **Best for** | Azure shops | AWS shops | GCP shops | Simple/K8s-only | Multi-cloud, compliance |

*\* K8s Secrets encryption requires EncryptionConfiguration setup (not default)*

---

### 9. Which One Should You Use? — Decision Tree

```
What cloud are you on?
  ├── Azure ────────────► Azure Key Vault
  │                       (Native integration, lowest latency, free tier)
  │
  ├── AWS ──────────────► AWS Secrets Manager (passwords)
  │                       + AWS Parameter Store (config values)
  │                       ($0.40/secret/month, automatic rotation)
  │
  ├── GCP ──────────────► Google Cloud Secret Manager
  │                       (Same price as AWS, native GKE integration)
  │
  ├── Multi-cloud ──────► HashiCorp Vault
  │                       OR External Secrets Operator + each cloud's native store
  │
  ├── On-premises ──────► HashiCorp Vault (self-hosted)
  │                       OR Kubernetes Secrets + EncryptionConfiguration
  │
  └── Dev/Staging ──────► Kubernetes Secrets (simplest, no cloud dependency)
                           OR .NET User Secrets (local dev only)
```

### 10. Best Practices Summary

| # | Practice | Why |
|---|----------|-----|
| 1 | **Never commit secrets to git** | `.gitignore` your `appsettings.*.local.json`, use `dotnet user-secrets` for dev |
| 2 | **Managed Identity over keys** | Azure AD Pod Identity / AWS IAM Roles for Service Accounts — no static credentials at all |
| 3 | **Use External Secrets Operator** | Syncs cloud vault → K8s Secret automatically; pods only need RBAC, not cloud credentials |
| 4 | **Auto-rotate secrets** | Especially DB passwords — Vault dynamic secrets or cloud-native rotation |
| 5 | **Audit access** | Every cloud vault logs who accessed what secret — enable alerts on anomalies |
| 6 | **Least privilege** | Each microservice gets its own vault path — `order-api` can't read `analytics-consumer` secrets |
| 7 | **Use short TTLs** | Vault dynamic secrets: 1h TTL. If leaked, they expire in 60 minutes |
| 8 | **Encrypt K8s Secrets at rest** | If no cloud vault, at minimum enable `EncryptionConfiguration` on your cluster |
| 9 | **Monitor secret access** | Set up alerts for: failed access attempts, unusual volume, off-hours access |
| 10 | **Rotate manually with automation** | At minimum, rotate all production secrets every 90 days via pipeline |

### 11. Quick-Start — Food Delivery App Secrets Setup

```bash
# ────────────────────────────────────────────────────────
# Option A: Azure (recommended for this app)
# ────────────────────────────────────────────────────────

# 1. Create Key Vault
az keyvault create -n fooddelivery-kv -g food-delivery-rg

# 2. Store all secrets
az keyvault secret set --vault-name fooddelivery-kv \
  --name "sql-connection-string" \
  --value "Server=prod-db;Database=FoodDelivery;User Id=sa;Password=RealPass123!"

az keyvault secret set --vault-name fooddelivery-kv \
  --name "rabbitmq-password" --value "guest"

az keyvault secret set --vault-name fooddelivery-kv \
  --name "kafka-sasl-username" --value "your-confluent-key"

az keyvault secret set --vault-name fooddelivery-kv \
  --name "kafka-sasl-password" --value "your-confluent-secret"

az keyvault secret set --vault-name fooddelivery-kv \
  --name "partner-api-key" --value "sk-proj-abc123"

az keyvault secret set --vault-name fooddelivery-kv \
  --name "jwt-signing-key" --value "MIICWwIBAAKB..."

az keyvault secret set --vault-name fooddelivery-kv \
  --name "warehouse-connection-string" \
  --value "Server=warehouse-db;Database=FoodWarehouse;User Id=admin;Password=WarePass456!"

# 3. Enable Managed Identity for AKS
az aks update -n food-aks -g food-rg --enable-managed-identity
az keyvault set-policy --name fooddelivery-kv \
  --object-id $(az aks show -n food-aks -g food-rg --query identityProfile.kubeletidentity.objectId -o tsv) \
  --secret-permissions get list

# 4. Install External Secrets Operator
helm repo add external-secrets https://charts.external-secrets.io
helm install external-secrets external-secrets/external-secrets -n external-secrets --create-namespace

# 5. Apply the SecretStore + ExternalSecret (from section 1 above)
kubectl apply -f deploy/kubernetes/external-secrets.yaml

# 6. Verify secrets are synced to K8s
kubectl get secret food-delivery-k8s-secret -n food-delivery -o yaml
```


Architecture :

High-level responsibilities
•	Bootstraps the ASP.NET Core app (Server‑Side Blazor + MVC controllers + SignalR).
•	Configures DI, HttpClientFactory, EF Core, Dapper mapping, localization, authentication/authorization, background workers, and middleware pipeline.
•	Registers many application services, hosted/background services and named HttpClients with custom handlers and proxy resolution.
Key configurations and patterns
•	Dapper custom column mapping
•	Registers Dapper.SqlMapper.TypeMapProvider with a cached property map that honors [Column("...")] attribute and falls back to property name (case‑insensitive). Prevents mapping issues between Oracle UPPER_SNAKE_CASE and C# PascalCase.
•	TLS / HTTP global setup
•	ServicePointManager.SecurityProtocol set to TLS1.2 | TLS1.3 before any HTTP calls.
•	Many named HttpClient registrations using AddHttpClient(...) with custom HttpMessageHandler factories (e.g., WinHttpHandler, HttpClientHandler) and proxy-aware handlers.
•	CreateProxyAwareHandler / CreateBnmProxyHandler helpers: resolve proxy from environment/appsettings or system proxy and configure handler (PreAuthenticate, DefaultProxyCredentials, decompression).
•	Database
•	EF Core AppDbContext registered via AddDbContext using UseOracle and a logger factory configured to suppress EF SQL Info logs (keeps console cleaner).
•	A startup warm-up task executes SELECT 1 FROM DUAL to prime Oracle connection pool to avoid cold-start latency.
•	Service registration (Dependency Injection)
•	Wide use of DI patterns:
•	Scoped for per-request services (repositories, domain services).
•	Transient for lightweight stateless handlers (mappers, processors).
•	Singleton for long-lived shared services (caches, resolvers, processors that must preserve state).
•	Organized by feature/module (Stamping, BulkUpload, ImportData, Backgrounds, Integrations).
•	Named HttpClient registrations ("MyTaxClient", "HITS", "bnm", "DmsClient") with timeouts and base addresses when appropriate.
•	Uses typed clients where helpful (AddHttpClient<IHitsIntegrationService, HitsIntegrationService>).
•	Background processing and hosted services
•	Hangfire configured with in-memory storage and server (AddHangfire, AddHangfireServer) and Hangfire Dashboard enabled in dev/UAT.
•	Multiple IHostedService implementations registered for background tasks, warmups, import orchestrator, bulk processing.
•	Dedicated background queue + hosted service for import jobs.
•	Bulk / Import / Upload architecture
•	Modular registration of bulk processors, mappers, validators and AI-assisted processors.
•	Resilience / Retry policy service singleton used by bulk insert workflows.
•	SignalR hub for bulk processing notifications (MapHub<BulkProcessingHub>).
•	Localization & Culture pipeline
•	AddLocalization(Resources) + AddViewLocalization().
•	RequestLocalizationOptions with custom CustomRequestCultureProvider that reads ?lang= query; cookie provider reinserted to preserve priority.
•	Inline middleware sets the culture cookie when ?lang= provided (ensures UI stays on the requested language across requests).
•	MVC / Blazor / API configuration
•	AddControllersWithViews + XML formatters + view & data annotation localization.
•	Adds Server-side Blazor (AddServerSideBlazor) and MapBlazorHub.
•	Controller conventions:
•	ControllerAuthConvention() (enforce auth at controller level).
•	SwaggerOnlyOpenApiConvention() hides controllers from ApiExplorer unless group == "openapi" to improve Swagger generation performance.
•	Suppresses model binding inference (SuppressInferBindingSourcesForParameters) to avoid expensive reflection for DTOs.
•	Swagger / OpenAPI
•	AddSwaggerGen with DocInclusionPredicate to only include controllers explicitly grouped for openapi and with custom mapping for heavy model types to avoid deep reflection blowups.
•	Authentication & Authorization
•	Multiple authentication schemes registered:
•	Cookie authentication (default): login path, access denied path, cookie settings (Secure, SameSite=None).
•	JWT Bearer: configured with issuer and symmetric signing key.
•	Custom scheme StaticTokenAuthHandler for static token fallback.
•	AddAuthorization() called to enable policy-based checks.
•	Middleware / Request pipeline order
•	Development-only: Swagger UI, Hangfire dashboard, additional request logging for swagger.json generation.
•	Localization: inline middleware to set culture cookie → UseRequestLocalization.
•	Exception handling: dev/QA uses DeveloperExceptionPage; otherwise UseExceptionHandler("/Home/Error") + HSTS.
•	Static files: supports configurable external lampiran folder; in dev serves it via UseStaticFiles with a PhysicalFileProvider mapped to /lampiran.
•	Session: UseSession() before routing.
•	Routing, Authentication, Authorization in classic order: UseRouting() → UseAuthentication() → UseAuthorization().
•	Endpoint mapping:
•	MapControllers()
•	MapBlazorHub()
•	Default MVC route
•	SignalR hubs (/hubs/bulkProcessing)
•	Warm-up background task executed via Task.Run to warm DB connection.
•	App settings & file-storage fallback logic
•	Configurable FileStorage:LampiranBasePath used for attachments storage; robust fallbacks to wwwroot/lampiran if drive not available or path invalid (logs warnings/errors).
•	Performance and safety remarks in code
•	Uses AsNoTracking (seen in other services) and logger suppression to reduce noise.
•	Several comments to avoid N+1 behavior in heavy DTOs (Swagger mapping Hack).
•	Many services wired to avoid per-request creation of heavy objects; uses singletons where safe.
Pipelines and how a request flows (simplified)
1.	Client request arrives → Kestrel/IIS.
2.	Inline lang middleware (sets culture cookie if ?lang).
3.	Request localization resolved.
4.	Exception middleware (Dev vs Prod).
5.	Static file middleware (lampiran/wwwroot).
6.	Session middleware.
7.	Routing matches controller/Blazor/SignalR.
8.	Authentication and Authorization applied.
9.	Controller action/Blazor Hub executes using DI‑resolved services (EF Core, HttpClient, background services, record insertors, etc).
10.	Background jobs use Hangfire or hosted services for long-running work.
Important design patterns used
•	Dependency Injection (constructor injection, typed and named HttpClients).
•	Factory pattern for HttpMessageHandler and proxy resolution helpers.
•	Hosted-service / Background-queue pattern for import and bulk processing.
•	Adapter / Mapper pattern for Excel/XML → domain models (mappers and insert handlers).
•	Centralized configuration via appsettings + environment + env variables (proxy resolution).
•	Thin startup: startup code is mostly wiring; business logic kept in services.