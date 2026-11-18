graph LR
    A["Client"] --> B["Kestrel / Host"]
    B["Kestrel / Host"] --> C["Request Pipeline"]
    C["Request Pipeline"] --> D["Controllers / Blazor Hub / SignalR"]
    D["Controllers / Blazor Hub / SignalR"] --> E["Services (DI)"]
    E["Services (DI)"] --> F["Oracle DB"]


