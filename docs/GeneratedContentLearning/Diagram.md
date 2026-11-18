graph LR
    Client["Client (Browser)"] --> Kestrel["Kestrel / IIS / Kestrel Host"]
    
    subgraph RequestPipeline["HTTP Request Pipeline"]
        direction TB
        Lang["Inline lang middleware (?lang -> set culture cookie)"]
        Loc["RequestLocalization"]
        Ex["Exception handling (Dev/Prod)"]
        Static["StaticFiles (wwwroot, /lampiran)"]
        Sess["Session middleware"]
        Route["Routing"]
        AuthN["Authentication (Cookie / JWT / StaticToken)"]
        AuthZ["Authorization"]
        Endpoints["Endpoint Dispatch"]
    end

    Kestrel --> Lang --> Loc --> Ex --> Static --> Sess --> Route --> AuthN --> AuthZ --> Endpoints

    subgraph EndpointsGroup["Endpoints / Handlers"]
        Controllers["Controllers / MVC Actions"]
        Blazor["Server-side Blazor Hub"]
        SignalR["SignalR Hubs (/hubs/bulkProcessing)"]
    end

    Endpoints --> Controllers
    Endpoints --> Blazor
    Endpoints --> SignalR

    subgraph DI["Dependency Injection / Services"]
        direction LR
        EF["EF Core - AppDbContext (Oracle)"]
        Dapper["Dapper custom mapping"]
        HttpFactory["HttpClientFactory / Named & Typed HttpClients"]
        Services["Application Services (Scoped / Transient / Singleton)"]
        Hosted["Hosted Services & BackgroundWorkers (Import, Bulk, Warmup)"]
        Hangfire["Hangfire Server / Dashboard"]
        Cache["MemoryCache / DataProtection / EncryptionService"]
    end

    Controllers --> Services
    Blazor --> Services
    SignalR --> Services

    Services --> EF
    Services --> Dapper
    Services --> HttpFactory
    Services --> Cache
    Hosted --> Services
    Hangfire --> Hosted

    subgraph External["External Systems"]
        DB["Oracle DB (Production)"]
        HITS["HITS / MyTax / DMS / BNM APIs (via HttpClients)"]
        FileStore["File storage (configured Lampiran path)"]
    end

    EF --> DB
    HttpFactory --> HITS
    Services --> FileStore
    Hosted --> FileStore

    %% Notes (non-rendered nodes for quick reference)
    style RequestPipeline fill:#f9f,stroke:#333,stroke-width:1px
    style DI fill:#efe,stroke:#333,stroke-width:1px
    style External fill:#eef,stroke:#333,stroke-width:1px