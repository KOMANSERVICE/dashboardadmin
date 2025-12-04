# Queries CQRS - MagasinService

## Liste des requêtes

| Query | Handler | Response Type | Description |
|-------|---------|---------------|-------------|
| `GetAllMagasinQuery` | ✅ | `GetAllMagasinResult` | Récupérer tous les magasins d'une boutique |
| `GetOneMagasinQuery` | ✅ | `GetOneMagasinResult` | Récupérer un magasin par ID |
| `GetStockMovementsByBoutiqueQuery` | ✅ | `GetStockMovementsByBoutiqueResult` | Mouvements par boutique (paginé) |
| `GetStockMovementsByLocationQuery` | ✅ | `GetStockMovementsByLocationResult` | Mouvements entrants/sortants par magasin |
| `GetStockMovementsByProductQuery` | ✅ | `GetStockMovementsByProductResult` | Historique mouvements d'un produit |
| `GetStockSlipByIdQuery` | ✅ | `GetStockSlipByIdResult` | Récupérer un bordereau avec items |
| `GetStockSlipsByBoutiqueQuery` | ✅ | `GetStockSlipsByBoutiqueResult` | Tous les bordereaux d'une boutique |

## Détails des queries

### GetAllMagasinQuery
Récupère tous les magasins d'une boutique spécifique.

**Paramètres:**
- `Guid BoutiqueId` - ID de la boutique

**Response:** Liste de `StockLocationDTO`

### GetOneMagasinQuery
Récupère les détails d'un magasin spécifique.

**Paramètres:**
- `Guid Id` - ID du magasin

**Response:** `StockLocationDTO` ou null si non trouvé

### GetStockMovementsByBoutiqueQuery
Récupère les mouvements de stock d'une boutique avec pagination et filtres de date.

**Paramètres:**
- `Guid BoutiqueId` - ID de la boutique
- `DateTime? StartDate` - Date de début (optionnel)
- `DateTime? EndDate` - Date de fin (optionnel)
- `int PageNumber` - Numéro de page (défaut: 1)
- `int PageSize` - Taille de page (défaut: 20)

**Response:**
```csharp
{
    List<StockMovementDto> Movements,
    int TotalCount,
    int PageNumber,
    int PageSize
}
```

**Logique:**
- Filtre par boutique (via les magasins source/destination)
- Filtre optionnel par dates
- Tri par date décroissante
- Pagination avec skip/take

### GetStockMovementsByLocationQuery
Récupère les mouvements entrants et/ou sortants d'un magasin.

**Paramètres:**
- `Guid LocationId` - ID du magasin
- `bool IncludeIncoming` - Inclure les entrées (défaut: true)
- `bool IncludeOutgoing` - Inclure les sorties (défaut: true)
- `DateTime? StartDate` - Date de début (optionnel)
- `DateTime? EndDate` - Date de fin (optionnel)

**Response:** Liste de `StockMovementDto` avec relations incluses

**Logique:**
- Si IncludeIncoming: mouvements où DestinationLocationId = LocationId
- Si IncludeOutgoing: mouvements où SourceLocationId = LocationId
- Union des deux ensembles si les deux sont true
- Tri par date décroissante

### GetStockMovementsByProductQuery
Récupère l'historique des mouvements d'un produit spécifique dans une boutique.

**Paramètres:**
- `Guid ProductId` - ID du produit
- `Guid BoutiqueId` - ID de la boutique
- `DateTime? StartDate` - Date de début (optionnel)
- `DateTime? EndDate` - Date de fin (optionnel)

**Response:** Liste de `StockMovementDto` triés par date

### GetStockSlipByIdQuery
Récupère un bordereau complet avec tous ses items et relations.

**Paramètres:**
- `Guid Id` - ID du bordereau

**Response:** `StockSlipDto` avec:
- Informations du bordereau
- Liste des items (StockSlipItemDto)
- Relations vers les magasins source/destination

### GetStockSlipsByBoutiqueQuery
Récupère tous les bordereaux d'une boutique.

**Paramètres:**
- `Guid BoutiqueId` - ID de la boutique
- `DateTime? StartDate` - Date de début (optionnel)
- `DateTime? EndDate` - Date de fin (optionnel)

**Response:** Liste de `StockSlipDto` triés par date décroissante

**Note:** Les items ne sont pas chargés dans cette query pour des raisons de performance.