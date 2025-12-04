# Documentation MagasinService pour AI Agent

## Vue d'ensemble

MagasinService est un microservice de gestion des magasins et mouvements de stock. Il permet de gérer les mouvements inter-magasin, les bordereaux de transfert, et la traçabilité des produits entre différents emplacements.

## Architecture

### Structure DDD
```
MagasinService/
├── Domain/
│   ├── Entities/
│   │   ├── StockLocation.cs      # Emplacement de stock (magasin/entrepôt)
│   │   ├── StockMovement.cs      # Mouvement de stock inter-magasin
│   │   ├── StockSlip.cs          # Bordereau de transfert
│   │   └── StockSlipItem.cs      # Ligne de bordereau
│   ├── ValueObjects/
│   │   ├── StockLocationId.cs
│   │   ├── StockMovementId.cs
│   │   ├── StockSlipId.cs
│   │   └── StockSlipItemId.cs
│   └── Enums/
│       ├── StockLocationType.cs   # Store, Warehouse
│       ├── StockMovementType.cs   # In, Out, Transfer, Adjustment, Return
│       └── StockSlipType.cs       # Entry, Exit, Transfer
├── Application/           # CQRS Commands/Queries
├── Infrastructure/        # EF Core, Repositories
└── Api/                  # Endpoints Carter
```

## Fonctionnalités principales

### 1. Gestion des emplacements de stock (StockLocation)

**Entité StockLocation** :
- `Id`: StockLocationId
- `Name`: Nom du magasin/entrepôt
- `Address`: Adresse physique
- `Type`: StockLocationType (Store/Warehouse)
- `BoutiqueId`: ID de la boutique (provient d'une autre application)
- Collections de navigation pour les mouvements

### 2. Mouvements de stock (StockMovement)

**Entité StockMovement** :
- `Id`: StockMovementId
- `Quantity`: Quantité déplacée
- `Date`: Date du mouvement
- `Reference`: Référence unique
- `MovementType`: Type de mouvement
- `ProductId`: ID du produit (provient d'une autre application)
- `SourceLocationId`: Emplacement source
- `DestinationLocationId`: Emplacement destination

**Règles métier** :
- Source et destination doivent appartenir à la même boutique
- Source et destination ne peuvent pas être identiques
- Les quantités peuvent être négatives (ajustements)

### 3. Bordereaux de transfert (StockSlip)

**Entité StockSlip** :
- `Id`: StockSlipId
- `Reference`: Référence unique du bordereau
- `Date`: Date de création
- `Note`: Note/commentaire
- `BoutiqueId`: ID de la boutique
- `SlipType`: Type de bordereau
- `SourceLocationId`: Emplacement source
- `DestinationLocationId`: Emplacement destination (nullable)
- `StockSlipItems`: Collection des lignes du bordereau

**Types de bordereaux** :
- **Transfer** : Mouvement entre deux magasins (crée des StockMovements)
- **Entry** : Entrée de stock (sans destination)
- **Exit** : Sortie de stock (sans destination)

**Règles métier** :
- La référence doit être unique
- Un bordereau Transfer doit avoir une destination
- Un bordereau Transfer crée automatiquement les StockMovements correspondants

## Commands & Queries CQRS

### Commands

#### CreateStockMovement
```csharp
public record CreateStockMovementCommand(CreateStockMovementRequest Request) : ICommand<StockMovementDto>;

public record CreateStockMovementRequest
{
    public int Quantity { get; init; }
    public string Reference { get; init; }
    public StockMovementType MovementType { get; init; }
    public Guid ProductId { get; init; }
    public Guid SourceLocationId { get; init; }
    public Guid DestinationLocationId { get; init; }
}
```

#### CreateStockSlip
```csharp
public record CreateStockSlipCommand(CreateStockSlipRequest Request) : ICommand<StockSlipDto>;

public record CreateStockSlipRequest
{
    public string Reference { get; init; }
    public string Note { get; init; }
    public Guid BoutiqueId { get; init; }
    public StockSlipType SlipType { get; init; }
    public Guid SourceLocationId { get; init; }
    public Guid? DestinationLocationId { get; init; }
    public List<CreateStockSlipItemRequest> Items { get; init; }
}
```

### Queries

#### GetStockMovementsByProduct
```csharp
public record GetStockMovementsByProductQuery(Guid ProductId, Guid BoutiqueId) : IQuery<IEnumerable<StockMovementDto>>;
```

#### GetStockMovementsByLocation
```csharp
public record GetStockMovementsByLocationQuery(Guid LocationId, DateTime? StartDate, DateTime? EndDate) : IQuery<IEnumerable<StockMovementDto>>;
```

#### GetStockSlipById
```csharp
public record GetStockSlipByIdQuery(Guid Id) : IQuery<StockSlipDto>;
```

#### GetStockSlipsByBoutique
```csharp
public record GetStockSlipsByBoutiqueQuery(Guid BoutiqueId, StockSlipType? SlipType) : IQuery<IEnumerable<StockSlipDto>>;
```

## Endpoints API

### Stock Movements
- `POST /stock-movements` - Créer un mouvement de stock
- `GET /stock-movements/product/{productId}` - Mouvements par produit
- `GET /stock-movements/location/{locationId}` - Mouvements par emplacement
- `GET /stock-movements/boutique/{boutiqueId}` - Mouvements par boutique

### Stock Slips
- `POST /stock-slips` - Créer un bordereau de transfert
- `GET /stock-slips/{id}` - Récupérer un bordereau par ID
- `GET /stock-slips/boutique/{boutiqueId}` - Bordereaux par boutique

## Scénarios d'utilisation

### 1. Transfert inter-magasin
```gherkin
Given une boutique avec deux magasins
When je crée un bordereau de type Transfer
  And j'ajoute des produits avec leurs quantités
Then un bordereau est créé
  And des mouvements de stock sont créés pour chaque produit
  And les mouvements tracent le transfert du magasin source vers destination
```

### 2. Réception de marchandises
```gherkin
Given un entrepôt de réception
When je crée un bordereau de type Entry
  And j'ajoute les produits reçus
Then un bordereau d'entrée est créé
  And aucun mouvement n'est créé (pas de transfert)
```

### 3. Sortie pour vente
```gherkin
Given un magasin avec du stock
When je crée un bordereau de type Exit
  And j'indique les produits vendus
Then un bordereau de sortie est créé
  And aucun mouvement n'est créé
```

## Configuration Technique

### Dependencies
- IDR.Library.BuildingBlocks (CQRS, Repositories, Exceptions)
- Entity Framework Core 9.0
- Carter pour les endpoints
- FluentValidation pour la validation

### Base de données
- Tables : StockLocations, StockMovements, StockSlips, StockSlipItems
- Relations configurées avec Fluent API
- ValueObjects configurés comme owned entities

### Patterns utilisés
- Domain-Driven Design (DDD)
- CQRS avec IDR.Library.BuildingBlocks
- Repository Pattern
- Value Objects pour les IDs
- Unit of Work pour les transactions

## Points d'attention pour l'AI Agent

1. **IDs externes** : ProductId et BoutiqueId proviennent d'autres applications, pas de FK
2. **Création de mouvements** : Seuls les bordereaux de type Transfer créent des StockMovements
3. **Validation boutique** : Source et destination doivent appartenir à la même boutique
4. **Références uniques** : Les références de bordereaux doivent être uniques
5. **Quantités négatives** : Autorisées pour les ajustements d'inventaire

## Tests

Le projet contient des tests unitaires et d'intégration :
- Tests des Value Objects
- Tests des handlers CQRS avec mocks
- Tests d'intégration avec base en mémoire
- Validation des règles métier