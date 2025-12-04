# DTOs - MagasinService

## Vue d'ensemble
Les DTOs (Data Transfer Objects) utilisés pour la communication entre les couches et l'API.

## Magasins (StockLocation)

### StockLocationDTO
DTO complet pour un magasin.

```csharp
public record StockLocationDTO : StockLocationUpdateDTO
{
    public Guid Id { get; set; }
    public StockLocationType Type { get; set; } = StockLocationType.Sale;
}
```

### StockLocationUpdateDTO
DTO pour la mise à jour d'un magasin.

```csharp
public record StockLocationUpdateDTO
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}
```

### CreateMagasinRequest
Requête pour créer un magasin.

```csharp
public record CreateMagasinRequest
{
    public StockLocationDTO StockLocation { get; init; } = null!;
}
```

### CreateMagasinResult/Response
Résultat de la création d'un magasin.

```csharp
public record CreateMagasinResult(StockLocationDTO StockLocation);
public record CreateMagasinResponse(StockLocationDTO StockLocation);
```

## Mouvements de Stock (StockMovement)

### StockMovementDto
DTO complet pour un mouvement de stock.

```csharp
public record StockMovementDto
{
    public Guid Id { get; init; }
    public int Quantity { get; init; }
    public DateTime Date { get; init; }
    public string Reference { get; init; } = string.Empty;
    public StockMovementType MovementType { get; init; }
    public Guid ProductId { get; init; }
    public Guid SourceLocationId { get; init; }
    public Guid DestinationLocationId { get; init; }
    public StockLocationDTO? SourceLocation { get; init; }
    public StockLocationDTO? DestinationLocation { get; init; }
}
```

### CreateStockMovementRequest
Requête pour créer un mouvement inter-magasin.

```csharp
public record CreateStockMovementRequest
{
    public int Quantity { get; init; }
    public string Reference { get; init; } = string.Empty;
    public StockMovementType MovementType { get; init; }
    public Guid ProductId { get; init; }
    public Guid SourceLocationId { get; init; }
    public Guid DestinationLocationId { get; init; }
}
```

## Bordereaux (StockSlip)

### StockSlipDto
DTO complet pour un bordereau avec ses items.

```csharp
public record StockSlipDto
{
    public Guid Id { get; init; }
    public string Reference { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public string Note { get; init; } = string.Empty;
    public Guid BoutiqueId { get; init; }
    public StockSlipType SlipType { get; init; }
    public Guid SourceLocationId { get; init; }
    public Guid? DestinationLocationId { get; init; }
    public StockLocationDTO? SourceLocation { get; init; }
    public StockLocationDTO? DestinationLocation { get; init; }
    public List<StockSlipItemDto> Items { get; init; } = new();
}
```

### StockSlipItemDto
DTO pour une ligne de bordereau.

```csharp
public record StockSlipItemDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public string Note { get; init; } = string.Empty;
}
```

### CreateStockSlipRequest
Requête pour créer un bordereau de transfert.

```csharp
public record CreateStockSlipRequest
{
    public string Reference { get; init; } = string.Empty;
    public string Note { get; init; } = string.Empty;
    public Guid BoutiqueId { get; init; }
    public StockSlipType SlipType { get; init; }
    public Guid SourceLocationId { get; init; }
    public Guid? DestinationLocationId { get; init; }
    public List<CreateStockSlipItemRequest> Items { get; init; } = new();
}
```

### CreateStockSlipItemRequest
Requête pour une ligne de bordereau.

```csharp
public record CreateStockSlipItemRequest
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public string Note { get; init; } = string.Empty;
}
```

## Résultats de Queries

### GetAllMagasinResult
Résultat de la récupération de tous les magasins.

```csharp
public record GetAllMagasinResult(List<StockLocationDTO> StockLocations);
```

### GetOneMagasinResult
Résultat de la récupération d'un magasin.

```csharp
public record GetOneMagasinResult(StockLocationDTO? StockLocation);
```

### GetStockMovementsByBoutiqueResult
Résultat paginé des mouvements par boutique.

```csharp
public record GetStockMovementsByBoutiqueResult(
    List<StockMovementDto> Movements,
    int TotalCount,
    int PageNumber,
    int PageSize
);
```

### GetStockMovementsByLocationResult
Résultat des mouvements par localisation.

```csharp
public record GetStockMovementsByLocationResult(
    List<StockMovementDto> Movements
);
```

### GetStockMovementsByProductResult
Résultat des mouvements par produit.

```csharp
public record GetStockMovementsByProductResult(
    List<StockMovementDto> Movements
);
```

### GetStockSlipByIdResult
Résultat de la récupération d'un bordereau.

```csharp
public record GetStockSlipByIdResult(StockSlipDto? StockSlip);
```

### GetStockSlipsByBoutiqueResult
Résultat de la récupération des bordereaux.

```csharp
public record GetStockSlipsByBoutiqueResult(
    List<StockSlipDto> StockSlips
);
```

## Mapping

Les DTOs sont mappés vers/depuis les entités du domaine en utilisant **Mapster**.

Exemples:
```csharp
// Entity vers DTO
var dto = entity.Adapt<StockMovementDto>();

// Request vers Entity
var entity = request.Adapt<StockMovement>();

// Mapping avec configuration
TypeAdapterConfig<StockSlip, StockSlipDto>
    .NewConfig()
    .Map(dest => dest.Items, src => src.StockSlipItems);
```