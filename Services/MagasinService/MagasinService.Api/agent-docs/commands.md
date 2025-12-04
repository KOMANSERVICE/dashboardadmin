# Commands CQRS - MagasinService

## Liste des commandes

| Command | Handler | Validation | Description |
|---------|---------|------------|-------------|
| `CreateMagasinCommand` | ✅ | ✅ | Créer un nouveau magasin/localisation |
| `CreateStockMovementCommand` | ✅ | ✅ | Créer un mouvement inter-magasin |
| `CreateStockSlipCommand` | ✅ | ✅ | Créer un bordereau de transfert |

## Détails des commandes

### CreateMagasinCommand
Crée une nouvelle localisation de stock pour une boutique.

**Propriétés:**
- `StockLocationDTO StockLocation` - Données du magasin
- `Guid BoutiqueId` - ID de la boutique propriétaire

**Validation:**
- Name requis et non vide (max 100 caractères)
- Address requis et non vide (max 200 caractères)
- Type valide (Sale, Store, Site)
- BoutiqueId non vide

**Handler:**
1. Crée l'entité StockLocation
2. Assigne le BoutiqueId
3. Persiste via repository
4. Retourne le DTO du magasin créé

### CreateStockMovementCommand
Crée un mouvement de stock entre deux magasins.

**Propriétés:**
- `CreateStockMovementRequest Request` contenant:
  - `int Quantity` - Quantité à transférer
  - `string Reference` - Référence unique
  - `StockMovementType MovementType` - Type de mouvement
  - `Guid ProductId` - Produit concerné
  - `Guid SourceLocationId` - Magasin source
  - `Guid DestinationLocationId` - Magasin destination

**Validation:**
- Quantity > 0
- Reference non vide (max 50 caractères)
- Source != Destination
- MovementType = Transfer si source et destination spécifiées
- ProductId, SourceLocationId, DestinationLocationId non vides

**Handler:**
1. Valide l'existence des magasins source et destination
2. Vérifie qu'ils appartiennent à la même boutique
3. Crée l'entité StockMovement avec Date = DateTime.UtcNow
4. Persiste via repository
5. Retourne le DTO avec relations incluses

### CreateStockSlipCommand
Crée un bordereau de transfert avec plusieurs produits.

**Propriétés:**
- `CreateStockSlipRequest Request` contenant:
  - `string Reference` - Référence unique du bordereau
  - `string Note` - Note optionnelle
  - `Guid BoutiqueId` - Boutique propriétaire
  - `StockSlipType SlipType` - Type (Entry, Exit, Transfer)
  - `Guid SourceLocationId` - Magasin source
  - `Guid? DestinationLocationId` - Magasin destination (nullable)
  - `List<CreateStockSlipItemRequest> Items` - Lignes de produits

**Validation:**
- Reference unique et non vide (max 100 caractères)
- Au moins un item dans la liste
- BoutiqueId non vide
- Pour Transfer: DestinationLocationId requis
- Pour chaque item: ProductId non vide, Quantity > 0
- Vérifie que la référence n'existe pas déjà

**Handler:**
1. Vérifie l'unicité de la référence
2. Valide l'existence des magasins
3. Crée l'entité StockSlip avec Date = DateTime.UtcNow
4. Pour chaque item:
   - Crée StockSlipItem
   - Si Transfer: crée StockMovement associé
5. Persiste via Unit of Work (transaction)
6. Retourne le DTO complet avec items