# Analyse Issue #86 - US-035 : Etat des flux de tresorerie

## Classification
- **Scope**: Microservice (TresorerieService)
- **Type**: Nouvelle Query (lecture seule)
- **Confidence**: 0.95

## Resume de l'analyse

### Elements existants identifies
Le TresorerieService dispose deja de:

| Element | Description |
|---------|-------------|
| `CashFlow` entity | Entite complete avec Type (INCOME/EXPENSE/TRANSFER), Amount, CategoryId, Date |
| `Category` entity | Categories avec Type (INCOME/EXPENSE) |
| `GetCashFlowsQuery` | Query existante avec filtres StartDate/EndDate |
| Pattern CQRS | IQuery/IQueryHandler depuis IDR.Library.BuildingBlocks |

### Fonctionnalite a creer
Nouvelle Query `GetCashFlowStatementQuery` pour generer un etat des flux avec:
- Total des entrees (INCOME)
- Total des sorties (EXPENSE)
- Solde net (INCOME - EXPENSE)
- Repartition par categorie
- Filtrage par periode
- Comparaison avec periode precedente

## Architecture proposee

### Fichiers a creer

```
Services/TresorerieService/
├── TresorerieService.Application/
│   └── Features/
│       └── Reports/
│           └── GetCashFlowStatement/
│               ├── GetCashFlowStatementQuery.cs
│               ├── GetCashFlowStatementHandler.cs
│               └── GetCashFlowStatementResponse.cs
│
└── TresorerieService.Api/
    └── Endpoints/
        └── Reports/
            └── GetCashFlowStatementEndpoint.cs
```

### DTOs a creer

```csharp
// GetCashFlowStatementResponse.cs
public record GetCashFlowStatementResponse(
    // Periode
    DateTime StartDate,
    DateTime EndDate,

    // Totaux
    decimal TotalIncome,
    decimal TotalExpense,
    decimal NetBalance,

    // Nombre de flux
    int IncomeCount,
    int ExpenseCount,

    // Repartition par categorie
    IReadOnlyList<CategoryBreakdownDto> IncomeByCategory,
    IReadOnlyList<CategoryBreakdownDto> ExpenseByCategory,

    // Comparaison periode precedente (optionnel)
    PeriodComparisonDto? Comparison
);

public record CategoryBreakdownDto(
    Guid CategoryId,
    string CategoryName,
    decimal Amount,
    decimal Percentage,
    int TransactionCount
);

public record PeriodComparisonDto(
    DateTime PreviousStartDate,
    DateTime PreviousEndDate,
    decimal PreviousTotalIncome,
    decimal PreviousTotalExpense,
    decimal PreviousNetBalance,
    decimal IncomeVariation,      // % variation
    decimal ExpenseVariation,     // % variation
    decimal NetBalanceVariation   // % variation
);
```

## Specifications Gherkin

```gherkin
Feature: US-035 Etat des flux de tresorerie
  En tant que manager
  Je veux voir un etat detaille des flux
  Afin d'analyser les mouvements

  Background:
    Given un utilisateur authentifie avec le role "manager"
    And l'en-tete "X-Application-Id" est "APP001"
    And l'en-tete "X-Boutique-Id" est "BOUT001"

  # Scenario 1: Obtenir l'etat des flux pour une periode
  Scenario: Obtenir l'etat des flux de tresorerie avec succes
    Given les flux de tresorerie suivants existent pour la boutique "BOUT001":
      | Id                                   | Type    | Category      | Amount   | Date       | Status   |
      | 11111111-1111-1111-1111-111111111111 | INCOME  | Ventes        | 50000.00 | 2025-01-15 | APPROVED |
      | 22222222-2222-2222-2222-222222222222 | INCOME  | Services      | 25000.00 | 2025-01-20 | APPROVED |
      | 33333333-3333-3333-3333-333333333333 | EXPENSE | Fournisseurs  | 30000.00 | 2025-01-18 | APPROVED |
      | 44444444-4444-4444-4444-444444444444 | EXPENSE | Salaires      | 20000.00 | 2025-01-25 | APPROVED |
    When je fais une requete GET sur "/api/reports/cash-flow-statement?startDate=2025-01-01&endDate=2025-01-31"
    Then le code de reponse est 200
    And la reponse contient "totalIncome" avec la valeur 75000.00
    And la reponse contient "totalExpense" avec la valeur 50000.00
    And la reponse contient "netBalance" avec la valeur 25000.00
    And la reponse contient "incomeCount" avec la valeur 2
    And la reponse contient "expenseCount" avec la valeur 2

  # Scenario 2: Repartition par categorie
  Scenario: L'etat des flux inclut la repartition par categorie
    Given les flux de tresorerie suivants existent pour la boutique "BOUT001":
      | Type    | Category      | Amount   | Date       | Status   |
      | INCOME  | Ventes        | 60000.00 | 2025-01-10 | APPROVED |
      | INCOME  | Ventes        | 40000.00 | 2025-01-15 | APPROVED |
      | INCOME  | Services      | 50000.00 | 2025-01-20 | APPROVED |
    When je fais une requete GET sur "/api/reports/cash-flow-statement?startDate=2025-01-01&endDate=2025-01-31"
    Then le code de reponse est 200
    And la reponse contient "incomeByCategory" avec 2 elements
    And la categorie "Ventes" a un montant de 100000.00 et un pourcentage de 66.67
    And la categorie "Services" a un montant de 50000.00 et un pourcentage de 33.33

  # Scenario 3: Seuls les flux APPROVED sont inclus
  Scenario: Seuls les flux approuves sont comptabilises
    Given les flux de tresorerie suivants existent pour la boutique "BOUT001":
      | Type    | Amount   | Date       | Status    |
      | INCOME  | 50000.00 | 2025-01-15 | APPROVED  |
      | INCOME  | 30000.00 | 2025-01-18 | PENDING   |
      | INCOME  | 20000.00 | 2025-01-20 | DRAFT     |
      | EXPENSE | 25000.00 | 2025-01-22 | APPROVED  |
      | EXPENSE | 15000.00 | 2025-01-25 | REJECTED  |
    When je fais une requete GET sur "/api/reports/cash-flow-statement?startDate=2025-01-01&endDate=2025-01-31"
    Then le code de reponse est 200
    And la reponse contient "totalIncome" avec la valeur 50000.00
    And la reponse contient "totalExpense" avec la valeur 25000.00
    And la reponse contient "netBalance" avec la valeur 25000.00

  # Scenario 4: Les TRANSFER ne sont pas comptabilises
  Scenario: Les transferts ne sont pas inclus dans les totaux
    Given les flux de tresorerie suivants existent pour la boutique "BOUT001":
      | Type     | Amount   | Date       | Status   |
      | INCOME   | 50000.00 | 2025-01-15 | APPROVED |
      | EXPENSE  | 20000.00 | 2025-01-18 | APPROVED |
      | TRANSFER | 10000.00 | 2025-01-20 | APPROVED |
    When je fais une requete GET sur "/api/reports/cash-flow-statement?startDate=2025-01-01&endDate=2025-01-31"
    Then le code de reponse est 200
    And la reponse contient "totalIncome" avec la valeur 50000.00
    And la reponse contient "totalExpense" avec la valeur 20000.00
    And les transferts ne sont pas inclus dans les totaux

  # Scenario 5: Comparaison avec periode precedente
  Scenario: Comparer avec la periode precedente
    Given les flux de tresorerie suivants existent pour la boutique "BOUT001":
      # Periode courante (Janvier 2025)
      | Type    | Amount    | Date       | Status   |
      | INCOME  | 100000.00 | 2025-01-15 | APPROVED |
      | EXPENSE | 60000.00  | 2025-01-20 | APPROVED |
      # Periode precedente (Decembre 2024)
      | INCOME  | 80000.00  | 2024-12-15 | APPROVED |
      | EXPENSE | 50000.00  | 2024-12-20 | APPROVED |
    When je fais une requete GET sur "/api/reports/cash-flow-statement?startDate=2025-01-01&endDate=2025-01-31&comparePrevious=true"
    Then le code de reponse est 200
    And la reponse contient "comparison.previousTotalIncome" avec la valeur 80000.00
    And la reponse contient "comparison.previousTotalExpense" avec la valeur 50000.00
    And la reponse contient "comparison.incomeVariation" avec une valeur positive (25%)
    And la reponse contient "comparison.expenseVariation" avec une valeur positive (20%)

  # Scenario 6: Periode obligatoire
  Scenario: La periode de filtrage est obligatoire
    When je fais une requete GET sur "/api/reports/cash-flow-statement"
    Then le code de reponse est 400
    And la reponse contient un message d'erreur "startDate et endDate sont obligatoires"

  # Scenario 7: Headers obligatoires
  Scenario: Les headers X-Application-Id et X-Boutique-Id sont obligatoires
    Given l'en-tete "X-Application-Id" n'est pas fourni
    When je fais une requete GET sur "/api/reports/cash-flow-statement?startDate=2025-01-01&endDate=2025-01-31"
    Then le code de reponse est 400
    And la reponse contient un message d'erreur "L'en-tete X-Application-Id est obligatoire"

  # Scenario 8: Authentification requise
  Scenario: L'endpoint requiert une authentification
    Given l'utilisateur n'est pas authentifie
    When je fais une requete GET sur "/api/reports/cash-flow-statement?startDate=2025-01-01&endDate=2025-01-31"
    Then le code de reponse est 401

  # Scenario 9: Periode sans flux
  Scenario: Retourner des totaux a zero si aucun flux dans la periode
    Given aucun flux de tresorerie n'existe pour la periode specifiee
    When je fais une requete GET sur "/api/reports/cash-flow-statement?startDate=2025-06-01&endDate=2025-06-30"
    Then le code de reponse est 200
    And la reponse contient "totalIncome" avec la valeur 0.00
    And la reponse contient "totalExpense" avec la valeur 0.00
    And la reponse contient "netBalance" avec la valeur 0.00
    And la reponse contient "incomeByCategory" avec 0 elements
    And la reponse contient "expenseByCategory" avec 0 elements
```

## Endpoint API

```
GET /api/reports/cash-flow-statement?startDate=xxx&endDate=xxx&comparePrevious=true

Headers requis:
- Authorization: Bearer {JWT_TOKEN}
- X-Application-Id: {APPLICATION_ID}
- X-Boutique-Id: {BOUTIQUE_ID}

Query Parameters:
- startDate (required): Date de debut (ISO 8601)
- endDate (required): Date de fin (ISO 8601)
- comparePrevious (optional, default: false): Inclure la comparaison avec la periode precedente
```

## Criteres d'acceptation valides

| # | Critere | Implemente via |
|---|---------|----------------|
| 1 | Je vois le total des entrees (INCOME) | `totalIncome` dans la reponse |
| 2 | Je vois le total des sorties (EXPENSE) | `totalExpense` dans la reponse |
| 3 | Je vois le solde net | `netBalance` (totalIncome - totalExpense) |
| 4 | Je vois la repartition par categorie | `incomeByCategory` et `expenseByCategory` |
| 5 | Je peux filtrer par periode | Query params `startDate` et `endDate` |
| 6 | Je peux comparer avec la periode precedente | Query param `comparePrevious=true` |

## Decision

**STATUT: VALIDE** - L'issue peut etre implementee.

### Justification
- Le TresorerieService existe avec toutes les entites necessaires (CashFlow, Category)
- Les patterns CQRS sont en place (IQuery, IQueryHandler)
- Les filtres par periode existent deja dans GetCashFlowsQuery
- Aucune modification d'entite requise (pas de migration)
- Respect des conventions de nommage existantes

### Implementation suggeree
1. Creer le dossier `Features/Reports/GetCashFlowStatement/`
2. Implementer `GetCashFlowStatementQuery` avec IQuery<GetCashFlowStatementResponse>
3. Implementer `GetCashFlowStatementHandler` avec IQueryHandler
4. Creer l'endpoint Carter dans `Endpoints/Reports/`
5. Ajouter les tests Gherkin

### Estimation technique
- Complexite: Moyenne
- Pas de migration EF Core requise
- Pas de nouveau package requis
