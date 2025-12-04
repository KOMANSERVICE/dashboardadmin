# Entités - MagasinService

## Vue d'ensemble
Le domaine MagasinService gère les magasins (localisations de stock) et les mouvements de produits entre ces magasins.

## Entités principales

### StockLocation
Représente un magasin ou une localisation de stock.

**Propriétés:**
- `StockLocationId Id` - Identifiant unique (Value Object)
- `string Name` - Nom du magasin
- `string Address` - Adresse physique
- `StockLocationType Type` - Type de localisation (Sale, Store, Site)
- `Guid BoutiqueId` - ID de la boutique propriétaire (référence externe)

**Relations:**
- `ICollection<StockMovement> SourceMovements` - Mouvements sortants
- `ICollection<StockMovement> DestinationMovements` - Mouvements entrants
- `ICollection<StockSlip> SourceSlips` - Bordereaux source
- `ICollection<StockSlip> DestinationSlips` - Bordereaux destination

### StockMovement
Représente un mouvement/transfert de stock d'un produit entre deux magasins.

**Propriétés:**
- `StockMovementId Id` - Identifiant unique
- `int Quantity` - Quantité déplacée
- `DateTime Date` - Date du mouvement
- `string Reference` - Référence unique du mouvement
- `StockMovementType MovementType` - Type de mouvement
- `Guid ProductId` - ID du produit concerné (référence externe)
- `StockLocationId SourceLocationId` - Magasin source
- `StockLocationId DestinationLocationId` - Magasin destination

**Relations:**
- `StockLocation SourceLocation` - Navigation vers magasin source
- `StockLocation DestinationLocation` - Navigation vers magasin destination

### StockSlip
Bordereau de transfert pouvant contenir plusieurs produits.

**Propriétés:**
- `StockSlipId Id` - Identifiant unique
- `string Reference` - Référence unique du bordereau
- `DateTime Date` - Date de création
- `string Note` - Note/commentaire optionnel
- `Guid BoutiqueId` - Boutique propriétaire (référence externe)
- `StockSlipType SlipType` - Type de bordereau (Entry, Exit, Transfer)
- `StockLocationId SourceLocationId` - Magasin source
- `StockLocationId? DestinationLocationId` - Magasin destination (nullable)

**Relations:**
- `StockLocation SourceLocation` - Navigation vers magasin source
- `StockLocation? DestinationLocation` - Navigation vers magasin destination
- `ICollection<StockSlipItem> StockSlipItems` - Lignes du bordereau

### StockSlipItem
Ligne d'un bordereau représentant un produit et sa quantité.

**Propriétés:**
- `StockSlipItemId Id` - Identifiant unique
- `StockSlipId StockSlipId` - ID du bordereau parent
- `Guid ProductId` - ID du produit (référence externe)
- `int Quantity` - Quantité
- `string Note` - Note optionnelle sur la ligne

**Relations:**
- `StockSlip StockSlip` - Navigation vers le bordereau parent

## Value Objects

### StockLocationId
Identifiant type-safe pour StockLocation.

**Méthodes:**
- `static StockLocationId Of(Guid value)` - Factory avec validation

### StockMovementId
Identifiant type-safe pour StockMovement.

**Méthodes:**
- `static StockMovementId Of(Guid value)` - Factory avec validation

### StockSlipId
Identifiant type-safe pour StockSlip.

**Méthodes:**
- `static StockSlipId Of(Guid value)` - Factory avec validation

### StockSlipItemId
Identifiant type-safe pour StockSlipItem.

**Méthodes:**
- `static StockSlipItemId Of(Guid value)` - Factory avec validation

## Énumérations

### StockLocationType
```csharp
public enum StockLocationType
{
    Sale = 1,    // Magasin de vente
    Store = 2,   // Entrepôt/Stockage
    Site = 3     // Chantier
}
```

### StockMovementType
```csharp
public enum StockMovementType
{
    In = 1,         // Entrée de stock
    Out = 2,        // Sortie de stock
    Transfer = 3,   // Transfert inter-magasin
    Adjustment = 4, // Ajustement d'inventaire
    Return = 5      // Retour de produits
}
```

### StockSlipType
```csharp
public enum StockSlipType
{
    Entry = 1,    // Bordereau d'entrée
    Exit = 2,     // Bordereau de sortie
    Transfer = 3  // Bordereau de transfert
}
```

## Intégrations externes

### Produits (ProductId)
- Les produits sont référencés via `Guid ProductId`
- Aucune relation directe - provient d'un service externe
- Pas de validation d'existence au niveau domaine

### Boutiques (BoutiqueId)
- Les boutiques sont référencées via `Guid BoutiqueId`
- Aucune relation directe - provient d'un service externe
- Représente le propriétaire du magasin/bordereau

## Règles métier

1. **StockMovement**
   - Quantity doit être > 0
   - Reference est requise et unique
   - Source != Destination
   - Les deux magasins doivent appartenir à la même boutique

2. **StockSlip**
   - Reference unique et requise
   - Au moins un item requis
   - Pour Transfer: destination requise
   - Pour Entry/Exit: destination optionnelle

3. **StockLocation**
   - Name et Address requis
   - BoutiqueId requis
   - Type doit être valide