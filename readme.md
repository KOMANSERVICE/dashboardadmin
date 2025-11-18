## Configuration de la migration

# Installation de ef
dotnet tool install --global dotnet-ef

# commande de migration
dotnet ef migrations add InitialCreate --project Services/MenuService/MenuService.Infrastructure --startup-project Services/MenuService/MenuService.Api --output-dir Data/Migrations




# Architecture du Projet MenuService

    MenuService/
    ├── MenuService.Api/           → REST Controllers
    │   ├── Controllers/
    │   │   └── MenuController.cs
    │   ├── Program.cs
    │   ├── appsettings.json
    │   └── ...
    │
    ├── MenuService.Grpc/          → Implémentation gRPC
    │   ├── Protos/
    │   │   └── menu.proto
    │   ├── Services/
    │   │   └── MenuGrpcService.cs
    │   └── Program.cs
    │
    ├── MenuService.Application/          → Application logic
    │   ├── Repositories/
    │   │   └── IMenuRepository.cs
    │   ├── Services/
    │   │   └── MenuService.cs
    │   └── DTOs/
    │
    ├── MenuService.Domain/          → Application logic
    │   ├── Entities/
    │       └── Menu.cs
    │
    │
    └── MenuService.Infrastructure/ → EF Core, Persistence
        ├── Data/
        │   └── MenuContext.cs
        ├── Repositories/
        └── ...
