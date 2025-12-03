# MenuService

## Description
MenuService est un microservice responsable de la gestion des menus d'application dans l'écosystème DashBoardAdmin. Il permet de créer, modifier et gérer l'état (actif/inactif) des menus pour différentes applications. Le service suit l'architecture Clean Vertical Slice et expose une API RESTful documentée avec OpenAPI.

## Architecture

### Structure du projet
```
MenuService/
├── MenuService.Api/              # Couche de présentation (Minimal API avec Carter)
│   ├── Endpoints/                # Endpoints HTTP
│   │   └── Menus/               # Endpoints des menus
│   ├── DependencyInjection.cs    # Configuration des services API
│   └── Program.cs                # Point d'entrée
│
├── MenuService.Application/      # Logique métier (CQRS)
│   ├── Data/                     # Interfaces de données
│   ├── Features/                 # Features organisées par domaine
│   │   └── Menus/
│   │       ├── Commands/         # Commands (write operations)
│   │       ├── Queries/          # Queries (read operations)
│   │       └── DTOs/            # Data Transfer Objects
│   └── DependencyInjection.cs
│
├── MenuService.Domain/           # Entités et logique du domaine
│   ├── Abstractions/            # Interfaces du domaine
│   └── Models/                  # Modèles métier
│
├── MenuService.Infrastructure/   # Implémentation des dépendances externes
│   ├── Data/                    # DbContext et configurations EF
│   └── DependencyInjection.cs
│
└── MenuService.Grpc/            # Service gRPC (optionnel)
    └── Program.cs
```

## Endpoints API

### Documentation
- **OpenAPI JSON**: `/openapi/v1.json`
- **Swagger UI**: `/swagger` (en développement uniquement)

### Endpoints disponibles

#### GET /menu
Récupère tous les menus de l'application.

**Réponse**:
```json
{
  "success": true,
  "data": {
    "menus": [
      {
        "id": "guid",
        "nom": "string",
        "description": "string",
        "route": "string",
        "icon": "string",
        "ordre": 1,
        "isActive": true,
        "applicationId": "guid",
        "parentMenuId": "guid"
      }
    ]
  },
  "message": "Liste des menus récupérée avec succès",
  "statusCode": 200
}
```

#### GET /menu/actif
Récupère uniquement les menus actifs.

**Réponse**: Même structure que GET /menu, mais filtrée sur les menus actifs uniquement.

#### POST /menu
Crée un nouveau menu.

**Body**: `CreateMenuRequest`
```json
{
  "nom": "string",
  "description": "string",
  "route": "string",
  "icon": "string",
  "ordre": 1,
  "applicationId": "guid",
  "parentMenuId": "guid" // optionnel pour sous-menu
}
```

**Réponses**:
- 201 Created : Menu créé avec succès
- 400 Bad Request : Données invalides
- 401 Unauthorized : Non authentifié
- 403 Forbidden : Permissions insuffisantes
- 409 Conflict : Menu avec ce nom existe déjà

#### PUT /menu/{id}
Met à jour un menu existant.

**Paramètres**:
- `id` (GUID, requis) : Identifiant du menu

**Body**: `UpdateMenuRequest`
```json
{
  "nom": "string",
  "description": "string",
  "route": "string",
  "icon": "string",
  "ordre": 1,
  "parentMenuId": "guid"
}
```

**Réponse**: 200 OK

#### PATCH /menu/{id}/active
Active un menu désactivé.

**Paramètres**:
- `id` (GUID, requis) : Identifiant du menu

**Réponse**: 200 OK

#### PATCH /menu/{id}/inactive
Désactive un menu actif.

**Paramètres**:
- `id` (GUID, requis) : Identifiant du menu

**Réponse**: 200 OK

## Technologies utilisées

### Production
- **.NET 9.0** - Framework principal
- **ASP.NET Core** - Framework web
- **Carter** - Minimal API routing
- **IDR.Library.BuildingBlocks** - CQRS, Auth, Validation, Vault
- **Entity Framework Core** - ORM
- **PostgreSQL** - Base de données
- **OpenAPI/Swagger** - Documentation API
- **gRPC** - Communication inter-services (optionnel)

### Tests
- **xUnit** - Framework de test
- **Xunit.Gherkin.Quick** - Tests BDD
- **FluentAssertions** - Assertions fluides
- **Moq** - Mocking
- **EF Core InMemory** - Tests d'intégration

## Configuration

### Développement local
1. Assurez-vous que PostgreSQL est installé et en cours d'exécution
2. Configurez Vault pour les secrets (ou utilisez les variables d'environnement)
3. Mettez à jour la connection string dans `appsettings.Development.json`
4. Exécutez les migrations EF Core :
   ```bash
   dotnet ef migrations add InitialCreate --project Services/MenuService/MenuService.Infrastructure --startup-project Services/MenuService/MenuService.Api --output-dir Data/Migrations
   dotnet ef database update --project Services/MenuService/MenuService.Infrastructure --startup-project Services/MenuService/MenuService.Api
   ```

### Variables d'environnement
- `ASPNETCORE_ENVIRONMENT` : Environnement d'exécution (Development, Staging, Production)
- `ConnectionStrings__DefaultConnection` : Chaîne de connexion PostgreSQL
- `Allow__Origins` : Chemin Vault pour les origines CORS autorisées

## Sécurité

### Authentification et autorisation
Le service utilise l'authentification JWT fournie par IDR.Library.BuildingBlocks. Tous les endpoints nécessitent un token Bearer valide.

### CORS (Cross-Origin Resource Sharing)
Le service implémente une politique CORS configurable via Vault. Les origines autorisées sont récupérées de manière sécurisée au démarrage du service.

### Gestion des secrets
Utilisation de ISecureSecretProvider (IDR.Library.BuildingBlocks) pour récupérer les secrets depuis Vault.

## Patterns et bonnes pratiques

### CQRS (Command Query Responsibility Segregation)
- **Commands** : CreateMenu, UpdateMenu, ActiveMenu, InactiveMenu
- **Queries** : GetAllMenu, GetAllActifMenu
- **Handlers** : Logique métier isolée pour chaque opération

### Clean Architecture
- Séparation stricte des responsabilités
- Domain indépendant des détails techniques
- Infrastructure injectable et testable

### Vertical Slice Architecture
- Chaque feature (Menus) contient toute sa logique
- Réduction du couplage entre features
- Facilite l'ajout de nouvelles fonctionnalités

## Features spécifiques

### Hiérarchie de menus
Support des menus parents/enfants via `parentMenuId` pour créer des structures de navigation complexes.

### Gestion d'état
Les menus peuvent être activés/désactivés sans être supprimés, permettant une gestion flexible de la navigation.

### Multi-applications
Chaque menu est associé à une application spécifique via `applicationId`.

## Tests
Les tests sont organisés dans `tests/MenuService.Tests/` et utilisent Gherkin pour les scénarios BDD.

Pour exécuter les tests :
```bash
dotnet test tests/MenuService.Tests/MenuService.Tests.csproj
```

## Docker
Un Dockerfile est prévu pour containeriser le service :
```bash
docker build -t menu-service -f Services/MenuService/MenuService.Api/Dockerfile .
docker run -p 5002:80 menu-service
```

## Intégration gRPC
Le service expose également une interface gRPC (MenuService.Grpc) pour une communication haute performance avec d'autres microservices.

## Monitoring et logs
Le service utilise les capacités de logging d'ASP.NET Core avec support pour :
- Structured logging
- Correlation IDs pour le tracing distribué
- Métriques de performance

## Évolutions futures
- **Permissions granulaires** : Gestion des permissions par menu
- **Templates de menus** : Création de templates réutilisables
- **Analytics** : Tracking de l'utilisation des menus
- **Localisation** : Support multi-langues pour les menus

## Contact et support
Pour toute question ou problème, veuillez créer une issue dans le repository principal DashBoardAdmin.