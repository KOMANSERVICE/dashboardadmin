# MagasinService

## Description
MagasinService est un microservice responsable de la gestion des magasins (emplacements de stock) dans l'écosystème DashBoardAdmin. Il suit l'architecture Clean Vertical Slice et expose une API RESTful documentée avec OpenAPI.

## Architecture

### Structure du projet
```
MagasinService/
├── MagasinService.Api/           # Couche de présentation (Minimal API avec Carter)
│   ├── Endpoints/                # Endpoints HTTP
│   ├── DependencyInjection.cs    # Configuration des services API
│   └── Program.cs                # Point d'entrée
│
├── MagasinService.Application/   # Logique métier (CQRS)
│   ├── Commons/                  # Interfaces communes
│   ├── Features/                 # Features organisées par domaine
│   │   └── Magasins/
│   │       ├── Commands/         # Commands (write operations)
│   │       ├── Queries/          # Queries (read operations)
│   │       └── DTOs/            # Data Transfer Objects
│   └── DependencyInjection.cs
│
├── MagasinService.Domain/        # Entités et logique du domaine
│   ├── Abstractions/            # Interfaces du domaine
│   ├── Entities/                # Entités métier
│   ├── Enums/                   # Énumérations
│   ├── Exceptions/              # Exceptions métier
│   └── ValueObjects/            # Value Objects
│
└── MagasinService.Infrastructure/ # Implémentation des dépendances externes
    ├── Data/                    # DbContext et configurations EF
    └── DependencyInjection.cs
```

## Endpoints API

### Documentation
- **OpenAPI JSON**: `/openapi/v1.json`
- **Swagger UI**: `/` (en développement uniquement)

### Endpoints disponibles

#### GET /magasin/{BoutiqueId}
Récupère tous les magasins d'une boutique spécifique.

**Paramètres**:
- `BoutiqueId` (GUID, requis) : Identifiant de la boutique

**Réponse**:
```json
{
  "success": true,
  "data": {
    "stockLocations": [
      {
        "id": "guid",
        "nom": "string",
        "description": "string",
        "adresse": "string",
        "isActive": true
      }
    ]
  },
  "message": "Liste des magasins récupérée avec succès",
  "statusCode": 200
}
```

#### POST /magasin
Crée un nouveau magasin.

**Body**: `CreateMagasinRequest`
```json
{
  "nom": "string",
  "description": "string",
  "adresse": "string",
  "boutiqueId": "guid"
}
```

**Réponse**: 201 Created

#### GET /magasin/{id}
Récupère un magasin spécifique par son identifiant.

**Paramètres**:
- `id` (GUID, requis) : Identifiant du magasin

**Réponse**: `StockLocationDTO`

#### PUT /magasin/{id}
Met à jour un magasin existant.

**Paramètres**:
- `id` (GUID, requis) : Identifiant du magasin

**Body**: `UpdateMagasinRequest`
```json
{
  "nom": "string",
  "description": "string",
  "adresse": "string",
  "isActive": true
}
```

**Réponse**: 200 OK

## Technologies utilisées

### Production
- **.NET 9.0** - Framework principal
- **ASP.NET Core** - Framework web
- **Carter** - Minimal API routing
- **IDR.Library.BuildingBlocks** - CQRS, Auth, Validation
- **Entity Framework Core** - ORM
- **PostgreSQL** - Base de données
- **OpenAPI/Swagger** - Documentation API

### Tests
- **xUnit** - Framework de test
- **Xunit.Gherkin.Quick** - Tests BDD
- **FluentAssertions** - Assertions fluides
- **Moq** - Mocking
- **EF Core InMemory** - Tests d'intégration

## Configuration

### Développement local
1. Assurez-vous que PostgreSQL est installé et en cours d'exécution
2. Mettez à jour la connection string dans `appsettings.Development.json`
3. Exécutez les migrations EF Core :
   ```bash
   dotnet ef migrations add InitialCreate --project Services/MagasinService/MagasinService.Infrastructure --startup-project Services/MagasinService/MagasinService.Api --output-dir Data/Migrations
   dotnet ef database update --project Services/MagasinService/MagasinService.Infrastructure --startup-project Services/MagasinService/MagasinService.Api
   ```

### Variables d'environnement
- `ASPNETCORE_ENVIRONMENT` : Environnement d'exécution (Development, Staging, Production)
- `ConnectionStrings__DefaultConnection` : Chaîne de connexion PostgreSQL

## Authentification et autorisation
Le service utilise l'authentification JWT fournie par IDR.Library.BuildingBlocks. Les endpoints nécessitent un token Bearer valide (actuellement commenté dans le code).

## Patterns et bonnes pratiques

### CQRS (Command Query Responsibility Segregation)
- **Commands** : Opérations d'écriture (Create, Update, Delete)
- **Queries** : Opérations de lecture (Get, GetAll)
- **Handlers** : Logique métier pour chaque Command/Query

### Clean Architecture
- Séparation des responsabilités en couches
- Dépendances unidirectionnelles (extérieur vers intérieur)
- Domain au centre, indépendant des détails d'implémentation

### Vertical Slice Architecture
- Features organisées par domaine métier
- Cohésion forte au sein d'une feature
- Facilite la maintenance et l'évolution

## Features à implémenter
- **Mouvements inter-magasins** : Transfert de stock entre magasins
- **Inventaire** : Gestion des inventaires par magasin
- **Historique** : Traçabilité des modifications

## Tests
Les tests sont organisés dans `tests/MagasinService.Tests/` et utilisent Gherkin pour les scénarios BDD.

Pour exécuter les tests :
```bash
dotnet test tests/MagasinService.Tests/MagasinService.Tests.csproj
```

## Docker
Un Dockerfile est disponible pour containeriser le service :
```bash
docker build -t magasin-service -f Services/MagasinService/MagasinService.Api/Dockerfile .
docker run -p 5001:80 magasin-service
```

## Monitoring et logs
Le service utilise les capacités de logging d'ASP.NET Core. Les logs sont configurables via `appsettings.json`.

## Contact et support
Pour toute question ou problème, veuillez créer une issue dans le repository principal DashBoardAdmin.