# Design Patterns Learning Solution

This solution contains two projects:

- **DesignPatternsLearning** – your original .NET console application, retained with the 20 GoF demos and menu.
- **BookLibraryApi** – an ASP.NET Core Web API that demonstrates the 10 architectural patterns (21–30) in a running Product API.

Open `DesignPatternsLearning.sln` in Visual Studio.

## Run the console demonstrations

Set `DesignPatternsLearning` as the startup project, or run:

```bash
dotnet run --project DesignPatternsLearning/DesignPatternsLearning.csproj
```

Choose a pattern number from the menu.

## Run the ASP.NET Core demonstrations

```bash
dotnet run --project BookLibraryApi/BookLibraryApi.csproj
```

See `BookLibraryApi/README.md` for endpoints and pattern mapping.

## Learning note

The original project uses **Object Pool** as pattern 5. The 30-item summary in `Program.cs` is preserved: it includes five creational, five structural, ten behavioral, and ten architectural patterns. The API project focuses on the architectural patterns because the original 20 GoF examples already exist in the console project.
