# Endpoints - MagasinService

## Liste des endpoints

### Magasins (StockLocations)
| Méthode | Route | Description |
|---------|-------|-------------|
| `POST` | `/magasin/{boutiqueId}` | Créer un nouveau magasin |
| `GET` | `/magasin/{boutiqueId}` | Récupérer tous les magasins d'une boutique |
| `GET` | `/magasin/{id}` | Récupérer un magasin spécifique |

### Mouvements de stock (StockMovements)
| Méthode | Route | Description |
|---------|-------|-------------|
| `POST` | `/stock-movements` | Créer un mouvement inter-magasin |
| `GET` | `/stock-movements/boutique/{boutiqueId}` | Récupérer mouvements par boutique (paginé) |
| `GET` | `/stock-movements/location/{locationId}` | Récupérer mouvements par localisation |
| `GET` | `/stock-movements/product/{productId}/boutique/{boutiqueId}` | Récupérer mouvements par produit |

### Bordereaux de transfert (StockSlips)
| Méthode | Route | Description |
|---------|-------|-------------|
| `POST` | `/stock-slips` | Créer un nouveau bordereau |
| `GET` | `/stock-slips/{id}` | Récupérer un bordereau avec ses items |
| `GET` | `/stock-slips/boutique/{boutiqueId}` | Récupérer bordereaux par boutique |

## Détails des endpoints

### POST /stock-movements
Crée un nouveau mouvement de stock entre deux magasins.

**Request Body:**
```json
{
  "quantity": 10,
  "reference": "MVT-2025-001",
  "movementType": "Transfer",
  "productId": "guid",
  "sourceLocationId": "guid",
  "destinationLocationId": "guid"
}
```

**Validation:**
- Quantity > 0
- Reference requis (max 50 caractères)
- Source != Destination
- MovementType = Transfer quand source et destination sont spécifiés
- Les deux magasins doivent appartenir à la même boutique

### POST /stock-slips
Crée un bordereau avec plusieurs produits et génère automatiquement les mouvements associés.

**Request Body:**
```json
{
  "reference": "BS-2025-001",
  "note": "Transfert mensuel",
  "boutiqueId": "guid",
  "slipType": "Transfer",
  "sourceLocationId": "guid",
  "destinationLocationId": "guid",
  "items": [
    {
      "productId": "guid",
      "quantity": 10,
      "note": "Note optionnelle"
    }
  ]
}
```

**Validation:**
- Reference unique et requis
- Au moins un item requis
- Pour SlipType.Transfer, destination requise
- Pour SlipType.Entry/Exit, destination optionnelle