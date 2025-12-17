# Analyse Issue #84 - US-033 : Lister les flux non réconciliés

## Statut: ✅ VALIDE

## Classification
- **Scope**: Microservice (TresorerieService)
- **Type**: Nouvelle fonctionnalité (Query + Command)
- **Confiance**: 95%

## Analyse du code existant

### Entités concernées
- **CashFlow** (`TresorerieService.Domain.Entities.CashFlow.cs`)
  - `Status` (CashFlowStatus) - fld19 - Workflow du flux
  - `IsReconciled` (bool) - fld25 - Indicateur de réconciliation
  - `ReconciledAt` (DateTime?) - fld26
  - `ReconciledBy` (string?) - fld27
  - `BankStatementReference` (string?) - fld28
  - `AccountId` (Guid) - fld12 - FK vers Account

### Enums existants
- **CashFlowStatus**: DRAFT=1, PENDING=2, **APPROVED=3**, REJECTED=4, CANCELLED=5
- **CashFlowAction**: CREATED=1, UPDATED=2, SUBMITTED=3, APPROVED=4, REJECTED=5, CANCELLED=6, **RECONCILED=7** ✅

### Pattern à suivre
- Référence: `GetPendingCashFlowsEndpoint` / `GetPendingCashFlowsQuery` / `GetPendingCashFlowsHandler`
- Authentification: Headers `X-Application-Id`, `X-Boutique-Id` + JWT `Authorization`
- Framework: Carter (Minimal APIs) + CQRS (IDR.Library.BuildingBlocks)

## Implémentation requise

### 1. Query - Lister les flux non réconciliés
**Endpoint**: `GET /api/cash-flows/unreconciled`

**Fichiers à créer**:
```
Services/TresorerieService/
├── TresorerieService.Api/Endpoints/CashFlows/
│   └── GetUnreconciledCashFlowsEndpoint.cs
└── TresorerieService.Application/Features/CashFlows/Queries/GetUnreconciledCashFlows/
    ├── GetUnreconciledCashFlowsQuery.cs
    ├── GetUnreconciledCashFlowsHandler.cs
    ├── GetUnreconciledCashFlowsResponse.cs
    └── UnreconciledCashFlowDto.cs
```

**Critères**:
- Filtrer `Status == APPROVED` ET `IsReconciled == false`
- Filtre optionnel par `accountId` (query param)
- Filtre optionnel par période (`startDate`, `endDate`)
- Retourner le montant total non réconcilié (`TotalUnreconciledAmount`)
- Pagination optionnelle

### 2. Command - Réconcilier plusieurs flux en masse
**Endpoint**: `POST /api/cash-flows/reconcile`

**Fichiers à créer**:
```
Services/TresorerieService/
├── TresorerieService.Api/Endpoints/CashFlows/
│   └── ReconcileCashFlowsEndpoint.cs
└── TresorerieService.Application/Features/CashFlows/Commands/ReconcileCashFlows/
    ├── ReconcileCashFlowsCommand.cs
    ├── ReconcileCashFlowsHandler.cs
    ├── ReconcileCashFlowsValidator.cs
    └── ReconcileCashFlowsResult.cs
```

**Critères**:
- Recevoir une liste d'IDs de flux (`cashFlowIds: Guid[]`)
- Optionnel: `bankStatementReference` pour tous les flux
- Vérifier que tous les flux sont APPROVED et non encore réconciliés
- Mettre à jour: `IsReconciled = true`, `ReconciledAt`, `ReconciledBy`, `BankStatementReference`
- Créer une entrée `CashFlowHistory` avec `Action = RECONCILED`
- Retourner le nombre de flux réconciliés

## Vérifications de sécurité
- ✅ Seul un manager/admin peut effectuer la réconciliation (rôle à vérifier)
- ✅ ApplicationId et BoutiqueId obligatoires
- ✅ Authentification JWT requise

## Tests requis
- Tests unitaires des handlers
- Tests d'intégration Gherkin

## agent-docs à mettre à jour
Après implémentation:
- `endpoints.md` - Ajouter les 2 nouveaux endpoints
- `queries.md` - Ajouter GetUnreconciledCashFlowsQuery
- `commands.md` - Ajouter ReconcileCashFlowsCommand
- `README.md` - Mettre à jour la date

---

# Gherkin - Tests d'acceptation

```gherkin
Feature: US-033 - Lister les flux non réconciliés
  En tant que manager
  Je veux voir les flux non réconciliés
  Afin de faire ma réconciliation bancaire

  Background:
    Given Je suis authentifié en tant que "manager"
    And L'application "APP001" existe
    And La boutique "BOUT001" existe
    And Le compte bancaire "Compte Principal" existe avec l'ID "11111111-1111-1111-1111-111111111111"
    And Le compte bancaire "Compte Secondaire" existe avec l'ID "22222222-2222-2222-2222-222222222222"

  # ===============================
  # Scénarios GET /api/cash-flows/unreconciled
  # ===============================

  @query @unreconciled
  Scenario: Lister les flux non réconciliés - Cas nominal
    Given Les flux de trésorerie suivants existent:
      | Id                                   | Type    | Status   | IsReconciled | AccountId                            | Amount | Label          |
      | aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa | INCOME  | APPROVED | false        | 11111111-1111-1111-1111-111111111111 | 5000   | Vente client A |
      | bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb | EXPENSE | APPROVED | false        | 11111111-1111-1111-1111-111111111111 | 2000   | Achat stock    |
      | cccccccc-cccc-cccc-cccc-cccccccccccc | INCOME  | APPROVED | true         | 11111111-1111-1111-1111-111111111111 | 3000   | Déjà réconcilié|
      | dddddddd-dddd-dddd-dddd-dddddddddddd | INCOME  | PENDING  | false        | 11111111-1111-1111-1111-111111111111 | 1000   | En attente     |
    When J'appelle GET "/api/cash-flows/unreconciled"
    Then Le code de réponse est 200
    And La réponse contient 2 flux non réconciliés
    And Le montant total non réconcilié est 7000
    And Les flux "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa" et "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb" sont présents
    And Le flux "cccccccc-cccc-cccc-cccc-cccccccccccc" n'est pas présent
    And Le flux "dddddddd-dddd-dddd-dddd-dddddddddddd" n'est pas présent

  @query @unreconciled @filter
  Scenario: Filtrer par compte bancaire
    Given Les flux de trésorerie suivants existent:
      | Id                                   | Type    | Status   | IsReconciled | AccountId                            | Amount |
      | aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa | INCOME  | APPROVED | false        | 11111111-1111-1111-1111-111111111111 | 5000   |
      | bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb | EXPENSE | APPROVED | false        | 22222222-2222-2222-2222-222222222222 | 2000   |
    When J'appelle GET "/api/cash-flows/unreconciled?accountId=11111111-1111-1111-1111-111111111111"
    Then Le code de réponse est 200
    And La réponse contient 1 flux non réconcilié
    And Le montant total non réconcilié est 5000

  @query @unreconciled @filter
  Scenario: Filtrer par période
    Given Les flux de trésorerie suivants existent:
      | Id                                   | Status   | IsReconciled | Date       | Amount |
      | aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa | APPROVED | false        | 2024-01-15 | 5000   |
      | bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb | APPROVED | false        | 2024-02-15 | 2000   |
      | cccccccc-cccc-cccc-cccc-cccccccccccc | APPROVED | false        | 2024-03-15 | 3000   |
    When J'appelle GET "/api/cash-flows/unreconciled?startDate=2024-02-01&endDate=2024-02-28"
    Then Le code de réponse est 200
    And La réponse contient 1 flux non réconcilié
    And Le flux "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb" est présent

  @query @unreconciled @empty
  Scenario: Aucun flux non réconcilié
    Given Tous les flux approuvés sont réconciliés
    When J'appelle GET "/api/cash-flows/unreconciled"
    Then Le code de réponse est 200
    And La réponse contient 0 flux non réconcilié
    And Le montant total non réconcilié est 0

  @query @unreconciled @auth
  Scenario: Accès refusé sans authentification
    Given Je ne suis pas authentifié
    When J'appelle GET "/api/cash-flows/unreconciled"
    Then Le code de réponse est 401

  @query @unreconciled @header
  Scenario: En-tête X-Application-Id manquant
    Given Je suis authentifié
    And L'en-tête X-Application-Id n'est pas fourni
    When J'appelle GET "/api/cash-flows/unreconciled"
    Then Le code de réponse est 400
    And Le message d'erreur contient "X-Application-Id est obligatoire"

  # ===============================
  # Scénarios POST /api/cash-flows/reconcile
  # ===============================

  @command @reconcile
  Scenario: Réconcilier plusieurs flux en masse - Cas nominal
    Given Les flux de trésorerie suivants existent:
      | Id                                   | Status   | IsReconciled |
      | aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa | APPROVED | false        |
      | bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb | APPROVED | false        |
    When J'appelle POST "/api/cash-flows/reconcile" avec:
      """
      {
        "cashFlowIds": [
          "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
          "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"
        ],
        "bankStatementReference": "REL-2024-001"
      }
      """
    Then Le code de réponse est 200
    And 2 flux ont été réconciliés
    And Le flux "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa" a IsReconciled = true
    And Le flux "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa" a BankStatementReference = "REL-2024-001"
    And Une entrée CashFlowHistory avec Action = "RECONCILED" existe pour chaque flux

  @command @reconcile
  Scenario: Réconcilier sans référence bancaire
    Given Un flux approuvé non réconcilié "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa" existe
    When J'appelle POST "/api/cash-flows/reconcile" avec:
      """
      {
        "cashFlowIds": ["aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"]
      }
      """
    Then Le code de réponse est 200
    And 1 flux a été réconcilié
    And Le flux a BankStatementReference = null

  @command @reconcile @error
  Scenario: Échec - Flux non approuvé
    Given Un flux en attente "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa" avec Status = PENDING existe
    When J'appelle POST "/api/cash-flows/reconcile" avec:
      """
      {
        "cashFlowIds": ["aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"]
      }
      """
    Then Le code de réponse est 400
    And Le message d'erreur contient "Seul un flux approuvé peut être réconcilié"

  @command @reconcile @error
  Scenario: Échec - Flux déjà réconcilié
    Given Un flux approuvé et réconcilié "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa" existe
    When J'appelle POST "/api/cash-flows/reconcile" avec:
      """
      {
        "cashFlowIds": ["aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"]
      }
      """
    Then Le code de réponse est 400
    And Le message d'erreur contient "déjà réconcilié"

  @command @reconcile @error
  Scenario: Échec - Flux inexistant
    When J'appelle POST "/api/cash-flows/reconcile" avec:
      """
      {
        "cashFlowIds": ["99999999-9999-9999-9999-999999999999"]
      }
      """
    Then Le code de réponse est 404
    And Le message d'erreur contient "n'existe pas"

  @command @reconcile @error
  Scenario: Échec - Liste vide
    When J'appelle POST "/api/cash-flows/reconcile" avec:
      """
      {
        "cashFlowIds": []
      }
      """
    Then Le code de réponse est 400
    And Le message d'erreur contient "Au moins un flux est requis"

  @command @reconcile @auth
  Scenario: Accès refusé pour un employé
    Given Je suis authentifié en tant que "employe"
    And Un flux approuvé non réconcilié existe
    When J'appelle POST "/api/cash-flows/reconcile" avec des IDs valides
    Then Le code de réponse est 403
    And Le message d'erreur contient "seul un manager ou admin"

  @command @reconcile @partial
  Scenario: Réconciliation partielle - Certains flux invalides
    Given Les flux suivants existent:
      | Id                                   | Status   | IsReconciled |
      | aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa | APPROVED | false        |
      | bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb | PENDING  | false        |
    When J'appelle POST "/api/cash-flows/reconcile" avec les deux IDs
    Then Le code de réponse est 400
    And Le message d'erreur liste les flux invalides
```

---

## Résumé de l'analyse

| Critère | Statut |
|---------|--------|
| Code existant compris | ✅ |
| Pas de contradiction | ✅ |
| Pattern CQRS respecté | ✅ |
| IDR.Library utilisé | ✅ |
| Migration requise | ❌ (pas de modification d'entité) |
| Documentation à jour | ⚠️ agent-docs à mettre à jour après implémentation |

**Décision**: ✅ **VALIDE** - Prêt pour implémentation
