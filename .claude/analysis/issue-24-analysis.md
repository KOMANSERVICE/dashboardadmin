# Analyse Issue #24 - Mouvement inter-magasin

## Résumé
Implementation de la gestion des mouvements inter-magasins de quantités de produits dans MagasinService.

## Statut: ✅ VALIDE

## Analyse détaillée

### Architecture
- ✅ Compatible avec Clean Vertical Slice Architecture
- ✅ Extension logique du MagasinService existant
- ✅ Utilise correctement IDR.Library.BuildingBlocks

### Entités à créer

#### 1. StockMovement
```csharp
public class StockMovement : Entity<StockMovementId>
{
    public int Quantity { get; set; }
    public DateTime Date { get; set; }
    public string Reference { get; set; } = string.Empty;
    public StockMovementType MovementType { get; set; }
    public Guid ProductId { get; set; }
    public StockLocationId SourceLocationId { get; set; }
    public StockLocationId DestinationLocationId { get; set; }
    public StockLocation SourceLocation { get; set; }
    public StockLocation DestinationLocation { get; set; }
}
```

#### 2. StockSlip
```csharp
public class StockSlip : Entity<StockSlipId>
{
    public string Reference { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Note { get; set; } = string.Empty;
    public Guid BoutiqueId { get; set; }
    public StockSlipType SlipType { get; set; } // À ajouter
    public ICollection<StockSlipItem> StockSlipItems { get; set; }
}
```

#### 3. StockSlipItem (MANQUANT - À CRÉER)
```csharp
public class StockSlipItem : Entity<StockSlipItemId>
{
    public StockSlipId StockSlipId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public StockSlip StockSlip { get; set; }
    public StockMovementId? StockMovementId { get; set; }
    public StockMovement? StockMovement { get; set; }
}
```

### Value Objects à créer
- ✅ StockMovementId
- ✅ StockSlipId
- 🔧 StockSlipItemId (à ajouter)

### Enums à créer
- 🔧 StockMovementType (Transfer, Entry, Exit, Adjustment, etc.)
- 🔧 StockSlipType (Entry, Exit)

### Modifications StockLocation
Ajouter les propriétés de navigation:
```csharp
public ICollection<StockMovement> SourceMovements { get; set; }
public ICollection<StockMovement> DestinationMovements { get; set; }
```

### Structure Features à créer
```
Features/
├── Mouvements/
│   ├── Commands/
│   │   ├── CreateStockMovement/
│   │   ├── CreateStockSlip/
│   │   └── CancelStockMovement/
│   ├── Queries/
│   │   ├── GetMovementsByLocation/
│   │   ├── GetMovementsBySlip/
│   │   └── GetMovementsReport/
│   └── DTOs/
│       ├── StockMovementDto.cs
│       ├── StockSlipDto.cs
│       └── StockSlipItemDto.cs
```

### Endpoints à créer
- POST /api/movements - Créer un mouvement
- POST /api/slips - Créer un bordereau
- GET /api/movements/location/{id} - Mouvements par magasin
- GET /api/movements/slip/{reference} - Mouvements par bordereau
- POST /api/movements/{id}/cancel - Annuler un mouvement
- GET /api/movements/report - Rapport des mouvements

### Règles métier identifiées
1. Quantité > 0
2. Source != Destination
3. ProductId requis
4. Reference requise
5. Validation existence des StockLocation
6. Mouvement inverse lors d'annulation

### Points nécessitant clarification
1. **Gestion multi-produits dans StockMovement**: Actuellement un seul ProductId, mais les bordereaux peuvent contenir plusieurs produits
   - Solution proposée: Un StockMovement par produit, liés via StockSlipItem
2. **Types de mouvements**: Définir exhaustivement les types possibles
3. **Validation des stocks**: Faut-il vérifier les quantités disponibles?

### Migration EF Core requise
- Nouvelles tables: StockMovements, StockSlips, StockSlipItems
- Mise à jour StockLocations avec relations
- Indices sur Reference, BoutiqueId, ProductId

### Recommandations
1. Implémenter d'abord les entités et value objects
2. Créer la migration EF Core
3. Implémenter les commandes de base (Create)
4. Ajouter les queries de consultation
5. Implémenter la logique d'annulation
6. Ajouter les validations métier
7. Mettre à jour la documentation AI

## Décision finale
✅ **PROCÉDER** - L'issue est valide et peut être implémentée selon les spécifications fournies avec les ajustements proposés.