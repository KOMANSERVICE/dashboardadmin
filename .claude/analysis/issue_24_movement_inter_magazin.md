# Analyse Issue #24 - Movement Inter-Magazin

## 📋 Informations de base
- **Issue**: #24
- **Titre**: Movement inter-magazin
- **Microservice**: MagasinService
- **Type**: Feature

## 🎯 Objectif
Mettre en place le mouvement inter-magasin des quantités de produit, permettant de transférer des produits entre différents magasins (StockLocation).

## 🔍 Analyse du contexte existant

### Entités existantes
1. **StockLocation**: Représente un lieu de stockage (magasin, entrepôt, chantier)
   - `Id`: StockLocationId (Value Object)
   - `Name`: string
   - `Address`: string
   - `Type`: StockLocationType (Sale, Store, Site)
   - `BoutiqueId`: Guid (provient d'une autre application)

2. **Entity<T>**: Classe de base abstraite
   - `Id`: T (avec Column("ch1"))

3. **DomainException**: Exception spécifique au domaine

### Architecture actuelle
- Utilise IDR.Library.BuildingBlocks (CQRS, Repositories, UnitOfWork)
- Pattern Value Object pour les IDs (StockLocationId)
- Configuration EF Core avec conversions
- Handlers CQRS pour les commands et queries

## 📐 Modélisation proposée

### Nouvelles entités

#### StockMovement
```csharp
public class StockMovement : Entity<StockMovementId>
{
    public int Quantity { get; set; }
    public DateTime Date { get; set; }
    public string Reference { get; set; } = string.Empty;
    public StockMovementType MovementType { get; set; }

    public Guid ProductId { get; set; } // Produit provenant d'une autre application
    public StockLocationId SourceLocationId { get; set; }
    public StockLocationId DestinationLocationId { get; set; }

    // Relations
    public StockLocation SourceLocation { get; set; }
    public StockLocation DestinationLocation { get; set; }
}
```

#### StockSlip
```csharp
public class StockSlip : Entity<StockSlipId>
{
    public string Reference { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Note { get; set; } = string.Empty;
    public StockSlipType SlipType { get; set; } // Entrée ou Sortie
    public SlipStatus Status { get; set; } // Draft, Validated, Cancelled
    public Guid BoutiqueId { get; set; }
    public Guid CreatedBy { get; set; } // User ID

    // Relations
    public ICollection<StockSlipItem> StockSlipItems { get; set; }
}
```

#### StockSlipItem
```csharp
public class StockSlipItem : Entity<StockSlipItemId>
{
    public StockSlipId StockSlipId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string Note { get; set; } = string.Empty;

    // Relations
    public StockSlip StockSlip { get; set; }
}
```

### Value Objects
- `StockMovementId`
- `StockSlipId`
- `StockSlipItemId`

### Enums
- `StockMovementType`: Transfer, Adjustment, Return
- `StockSlipType`: Inbound, Outbound
- `SlipStatus`: Draft, Validated, Cancelled

### Mise à jour de StockLocation
```csharp
public ICollection<StockMovement> SourceMovements { get; set; }
public ICollection<StockMovement> DestinationMovements { get; set; }
```

## 🚀 Scénarios Gherkin

### Feature: Movement inter-magazin

```gherkin
Feature: Movement inter-magazin
  En tant qu'utilisateur du système
  Je veux pouvoir transférer des produits entre magasins
  Afin de gérer efficacement les stocks entre différents points de stockage

  Background:
    Given les magasins suivants existent dans le système:
      | Id                                   | Name         | Type  | BoutiqueId                           |
      | 11111111-1111-1111-1111-111111111111 | Magasin A    | Sale  | 22222222-2222-2222-2222-222222222222 |
      | 33333333-3333-3333-3333-333333333333 | Entrepôt B   | Store | 22222222-2222-2222-2222-222222222222 |
    And les produits suivants existent dans le système externe:
      | ProductId                            | Name           |
      | 44444444-4444-4444-4444-444444444444 | Produit Alpha  |
      | 55555555-5555-5555-5555-555555555555 | Produit Beta   |

  Scenario: Création d'un bordereau de transfert avec un seul produit
    Given je suis connecté en tant qu'utilisateur autorisé
    When je crée un bordereau de transfert avec les données suivantes:
      | Field         | Value                                |
      | Reference     | TRF-2024-001                        |
      | BoutiqueId    | 22222222-2222-2222-2222-222222222222 |
      | Note          | Transfert urgent                     |
    And j'ajoute le produit suivant au bordereau:
      | ProductId                            | Quantity |
      | 44444444-4444-4444-4444-444444444444 | 10       |
    And je définis le transfert du "Magasin A" vers "Entrepôt B"
    And je valide le bordereau
    Then un bordereau de transfert est créé avec le statut "Validated"
    And un mouvement de stock est enregistré avec:
      | Field                | Value                                |
      | ProductId           | 44444444-4444-4444-4444-444444444444 |
      | Quantity            | 10                                   |
      | SourceLocationId    | 11111111-1111-1111-1111-111111111111 |
      | DestinationLocationId| 33333333-3333-3333-3333-333333333333 |
      | MovementType        | Transfer                             |

  Scenario: Création d'un bordereau de transfert avec plusieurs produits
    Given je suis connecté en tant qu'utilisateur autorisé
    When je crée un bordereau de transfert avec les données suivantes:
      | Field         | Value                                |
      | Reference     | TRF-2024-002                        |
      | BoutiqueId    | 22222222-2222-2222-2222-222222222222 |
    And j'ajoute les produits suivants au bordereau:
      | ProductId                            | Quantity | Note            |
      | 44444444-4444-4444-4444-444444444444 | 5        | Stock rotation  |
      | 55555555-5555-5555-5555-555555555555 | 15       | Client request  |
    And je définis le transfert de "Entrepôt B" vers "Magasin A"
    And je valide le bordereau
    Then un bordereau de transfert est créé avec 2 items
    And 2 mouvements de stock sont enregistrés

  Scenario: Tentative de création d'un transfert avec quantité négative
    Given je suis connecté en tant qu'utilisateur autorisé
    When je crée un bordereau de transfert
    And j'essaie d'ajouter un produit avec une quantité de -5
    Then une erreur de validation est retournée avec le message "La quantité doit être positive"

  Scenario: Tentative de transfert entre le même magasin
    Given je suis connecté en tant qu'utilisateur autorisé
    When je crée un bordereau de transfert
    And j'ajoute un produit valide
    And j'essaie de définir le transfert du "Magasin A" vers "Magasin A"
    Then une erreur de validation est retournée avec le message "Le magasin source et destination doivent être différents"

  Scenario: Annulation d'un bordereau validé
    Given un bordereau de transfert validé existe avec la référence "TRF-2024-003"
    When j'annule le bordereau "TRF-2024-003"
    Then le statut du bordereau passe à "Cancelled"
    And des mouvements de compensation sont créés pour annuler les transferts

  Scenario: Consultation de l'historique des mouvements d'un produit
    Given plusieurs mouvements existent pour le produit "44444444-4444-4444-4444-444444444444"
    When je consulte l'historique des mouvements du produit
    Then je vois la liste des mouvements triés par date décroissante
    And chaque mouvement affiche:
      | Field               | Description                          |
      | Date                | Date du mouvement                    |
      | Reference           | Référence du bordereau               |
      | SourceLocation      | Magasin d'origine                   |
      | DestinationLocation | Magasin de destination              |
      | Quantity            | Quantité transférée                 |
      | MovementType        | Type de mouvement                   |

  Scenario: Recherche de bordereaux par période
    Given plusieurs bordereaux existent dans le système
    When je recherche les bordereaux entre le "2024-01-01" et le "2024-12-31"
    And je filtre par boutique "22222222-2222-2222-2222-222222222222"
    Then je reçois la liste des bordereaux correspondants
    And les résultats incluent le nombre total d'items et la quantité totale transférée
```

## 🏗️ Plan d'implémentation

### 1. Domain Layer
- [ ] Créer les Value Objects (StockMovementId, StockSlipId, StockSlipItemId)
- [ ] Créer les Enums (StockMovementType, StockSlipType, SlipStatus)
- [ ] Créer les entités (StockMovement, StockSlip, StockSlipItem)
- [ ] Mettre à jour StockLocation avec les collections de mouvements
- [ ] Ajouter les règles métier de validation

### 2. Infrastructure Layer
- [ ] Créer les configurations EF Core pour les nouvelles entités
- [ ] Mettre à jour StockLocationConfiguration
- [ ] Créer la migration pour les nouvelles tables
- [ ] Ajouter les index nécessaires pour les performances

### 3. Application Layer
- [ ] Commands:
  - [ ] CreateStockSlipCommand/Handler
  - [ ] AddItemToStockSlipCommand/Handler
  - [ ] ValidateStockSlipCommand/Handler
  - [ ] CancelStockSlipCommand/Handler
- [ ] Queries:
  - [ ] GetStockMovementsByProductQuery/Handler
  - [ ] GetStockSlipsByPeriodQuery/Handler
  - [ ] GetStockSlipDetailsQuery/Handler
- [ ] DTOs pour les transferts
- [ ] Validators FluentValidation

### 4. API Layer
- [ ] Endpoints pour la gestion des bordereaux
- [ ] Endpoints pour consulter les mouvements
- [ ] Documentation Swagger/OpenAPI

## ⚠️ Points d'attention

1. **Intégration externe**: Les ProductId et BoutiqueId proviennent d'autres applications
   - Pas de foreign keys dans la DB
   - Validation via appels API externes si nécessaire

2. **Traçabilité**: Chaque mouvement doit être traçable
   - User qui a créé le mouvement
   - Date/heure précise
   - Référence unique

3. **Performances**:
   - Index sur ProductId, BoutiqueId
   - Pagination pour les historiques
   - Projection pour éviter les N+1

4. **Cohérence avec IDR.Library**:
   - Utiliser ICommand/IQuery de BuildingBlocks
   - Utiliser les exceptions du framework (DomainException, ValidationException)
   - Implémenter les validators FluentValidation

## 🔄 Migrations nécessaires

1. Création des tables:
   - StockMovements
   - StockSlips
   - StockSlipItems

2. Mise à jour de StockLocations:
   - Pas de modification de structure nécessaire (relations via FK dans StockMovements)

## ✅ Critères de validation

- [ ] Tous les tests unitaires passent
- [ ] Les scénarios Gherkin sont implémentés
- [ ] La documentation AI est à jour
- [ ] Les performances sont acceptables (< 200ms pour les queries simples)
- [ ] L'audit trail est complet