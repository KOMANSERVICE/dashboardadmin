# Analyse Issue #85 - US-034 : Tableau de bord tresorerie

## Classification

| Element | Valeur |
|---------|--------|
| **Type** | Microservice (TresorerieService) |
| **Scope** | Backend API + Frontend (nouveau) |
| **Complexite** | Moyenne |
| **Statut Analyse** | VALIDE |

## Resume de l'analyse

### Backend - TresorerieService

Le microservice **TresorerieService** existe et est bien structure avec:

#### Entites existantes utilisables:
- `Account` (comptes: CASH, BANK, MOBILE_MONEY)
- `CashFlow` (flux: INCOME, EXPENSE, TRANSFER)
- `Category` (categories: INCOME, EXPENSE)
- `RecurringCashFlow` (flux recurrents)

#### DTOs existants reutilisables:
- `AccountBalanceDto` - Solde temps reel avec variations et alertes
- `BalanceEvolutionDto` - Point d'evolution (Date, Balance, TotalIncome, TotalExpense)
- `PendingCashFlowDto` - Flux en attente de validation
- `CashFlowListDto` - Liste des flux

#### Patterns deja implementes:
- Calcul de variation jour/mois (dans `GetAccountBalanceHandler`)
- Gestion des alertes (seuil configurable par compte)
- Filtrage par ApplicationId/BoutiqueId

### Frontend - FrontendAdmin

Aucune page de tresorerie n'existe actuellement. A creer:
- Page `TreasuryDashboard.razor` dans `Pages/Tresorerie/`
- Composants: StatCard, AlertCard, BalanceChart
- Service HTTP pour appeler l'API

---

## Implementation requise

### 1. Backend - Nouvelle Feature Reports

**Dossier:** `TresorerieService.Application/Features/Reports/`

#### Nouveau DTO - TreasuryDashboardDto:
```csharp
public record TreasuryDashboardDto(
    // Totaux
    decimal TotalBalance,                    // Tous comptes
    Dictionary<AccountType, decimal> BalanceByType,  // Par type

    // Mois courant
    decimal MonthlyIncome,
    decimal MonthlyExpense,
    decimal NetBalance,                      // Income - Expense

    // En attente
    int PendingCount,
    decimal PendingAmount,

    // Alertes
    List<AccountAlertDto> Alerts,

    // Evolution 6 mois
    List<BalanceEvolutionDto> Evolution,

    // Metadata
    DateTime CalculatedAt
);

public record AccountAlertDto(
    Guid AccountId,
    string AccountName,
    AccountType Type,
    decimal CurrentBalance,
    decimal? AlertThreshold,
    string AlertType                         // "LOW_BALANCE" | "BUDGET_EXCEEDED"
);
```

#### Nouvelle Query - GetTreasuryDashboardQuery:
```csharp
public record GetTreasuryDashboardQuery(
    string ApplicationId,
    string BoutiqueId
) : IQuery<TreasuryDashboardDto>;
```

#### Nouveau Handler - GetTreasuryDashboardHandler:
- Agreger les soldes de tous les comptes actifs
- Calculer les revenus/depenses du mois
- Compter les flux PENDING
- Detecter les alertes (solde < seuil)
- Construire l'evolution sur 6 mois (mensuel)

### 2. Backend - Nouvel Endpoint

**Fichier:** `TresorerieService.Api/Endpoints/Reports/GetTreasuryDashboardEndpoint.cs`

```csharp
app.MapGet("/api/reports/treasury-dashboard", ...)
    .WithName("GetTreasuryDashboard")
    .WithTags("Reports")
```

### 3. Frontend - Nouvelle Page

**Fichier:** `FrontendAdmin.Shared/Pages/Tresorerie/TreasuryDashboard.razor`

Composants a utiliser:
- `StatCard` (ou creer si n'existe pas dans IDR.Library.Blazor)
- Graphique avec bibliotheque existante ou ApexCharts

---

## Fichiers a creer/modifier

### Backend (TresorerieService)

| Action | Fichier |
|--------|---------|
| CREER | `Application/Features/Reports/DTOs/TreasuryDashboardDto.cs` |
| CREER | `Application/Features/Reports/DTOs/AccountAlertDto.cs` |
| CREER | `Application/Features/Reports/Queries/GetTreasuryDashboard/GetTreasuryDashboardQuery.cs` |
| CREER | `Application/Features/Reports/Queries/GetTreasuryDashboard/GetTreasuryDashboardHandler.cs` |
| CREER | `Api/Endpoints/Reports/GetTreasuryDashboardEndpoint.cs` |

### Frontend (FrontendAdmin)

| Action | Fichier |
|--------|---------|
| CREER | `Shared/Pages/Tresorerie/TreasuryDashboard.razor` |
| CREER | `Shared/Pages/Tresorerie/Components/TreasuryStatCard.razor` |
| CREER | `Shared/Pages/Tresorerie/Components/TreasuryAlertCard.razor` |
| CREER | `Shared/Pages/Tresorerie/Components/TreasuryChart.razor` |
| CREER | `Shared/Services/ITresorerieService.cs` |
| CREER | `Shared/Services/TresorerieService.cs` |
| CREER | `Shared/Pages/Tresorerie/Models/TreasuryDashboardModel.cs` |

---

## Migrations EF Core

**Aucune migration necessaire** - L'endpoint utilise les entites existantes (Account, CashFlow) sans modification.

---

## Gherkin - Criteres d'acceptation

```gherkin
Feature: US-034 Tableau de bord tresorerie
  En tant que manager
  Je veux voir un tableau de bord de tresorerie
  Afin d'avoir une vue d'ensemble de ma situation financiere

  Background:
    Given je suis authentifie en tant que manager
    And j'ai acces a la boutique "MA_BOUTIQUE"

  # ============================================
  # SOLDE TOTAL
  # ============================================

  @backend @api
  Scenario: Voir le total de tresorerie disponible
    Given les comptes suivants existent pour ma boutique:
      | Nom      | Type   | Solde    | Actif |
      | Caisse 1 | CASH   | 150000   | true  |
      | BNP      | BANK   | 500000   | true  |
      | Orange   | MOBILE | 75000    | true  |
      | Caisse 2 | CASH   | 50000    | false |
    When je consulte le tableau de bord tresorerie
    Then je vois le total de tresorerie "725000 XOF"
    And le compte inactif "Caisse 2" n'est pas inclus

  # ============================================
  # SOLDE PAR TYPE
  # ============================================

  @backend @api
  Scenario: Voir le total par type de compte
    Given les comptes suivants existent pour ma boutique:
      | Nom      | Type         | Solde   |
      | Caisse 1 | CASH         | 150000  |
      | Caisse 2 | CASH         | 100000  |
      | BNP      | BANK         | 500000  |
      | Orange   | MOBILE_MONEY | 75000   |
    When je consulte le tableau de bord tresorerie
    Then je vois les totaux par type:
      | Type         | Total   |
      | CASH         | 250000  |
      | BANK         | 500000  |
      | MOBILE_MONEY | 75000   |

  # ============================================
  # REVENUS ET DEPENSES DU MOIS
  # ============================================

  @backend @api
  Scenario: Voir les revenus du mois courant
    Given nous sommes le "15/01/2024"
    And les flux suivants existent pour ma boutique:
      | Type    | Montant | Date       | Status   |
      | INCOME  | 200000  | 05/01/2024 | APPROVED |
      | INCOME  | 150000  | 10/01/2024 | APPROVED |
      | INCOME  | 50000   | 12/01/2024 | PENDING  |
      | INCOME  | 100000  | 20/12/2023 | APPROVED |
    When je consulte le tableau de bord tresorerie
    Then je vois les revenus du mois "350000 XOF"
    And les flux PENDING ne sont pas inclus dans les revenus
    And les flux de decembre ne sont pas inclus

  @backend @api
  Scenario: Voir les depenses du mois courant
    Given nous sommes le "15/01/2024"
    And les flux suivants existent pour ma boutique:
      | Type    | Montant | Date       | Status   |
      | EXPENSE | 80000   | 03/01/2024 | APPROVED |
      | EXPENSE | 45000   | 08/01/2024 | APPROVED |
      | EXPENSE | 20000   | 14/01/2024 | DRAFT    |
    When je consulte le tableau de bord tresorerie
    Then je vois les depenses du mois "125000 XOF"
    And les flux DRAFT ne sont pas inclus dans les depenses

  @backend @api
  Scenario: Voir le solde net du mois
    Given les revenus du mois sont "350000 XOF"
    And les depenses du mois sont "125000 XOF"
    When je consulte le tableau de bord tresorerie
    Then je vois le solde net "225000 XOF"
    And le solde net est positif (vert)

  @backend @api
  Scenario: Solde net negatif
    Given les revenus du mois sont "100000 XOF"
    And les depenses du mois sont "180000 XOF"
    When je consulte le tableau de bord tresorerie
    Then je vois le solde net "-80000 XOF"
    And le solde net est negatif (rouge)

  # ============================================
  # FLUX EN ATTENTE
  # ============================================

  @backend @api
  Scenario: Voir les flux en attente de validation
    Given les flux suivants existent pour ma boutique:
      | Type    | Montant | Status   |
      | INCOME  | 50000   | PENDING  |
      | EXPENSE | 30000   | PENDING  |
      | INCOME  | 25000   | PENDING  |
      | EXPENSE | 10000   | APPROVED |
    When je consulte le tableau de bord tresorerie
    Then je vois "3" flux en attente de validation
    And je vois le montant total en attente "105000 XOF"

  # ============================================
  # ALERTES
  # ============================================

  @backend @api
  Scenario: Voir les alertes de solde bas
    Given les comptes suivants existent pour ma boutique:
      | Nom      | Solde  | Seuil Alerte |
      | Caisse 1 | 15000  | 50000        |
      | BNP      | 200000 | 100000       |
      | Orange   | 5000   | 10000        |
    When je consulte le tableau de bord tresorerie
    Then je vois 2 alertes de solde bas:
      | Compte   | Solde | Seuil  |
      | Caisse 1 | 15000 | 50000  |
      | Orange   | 5000  | 10000  |

  @backend @api
  Scenario: Aucune alerte si tous les soldes sont au-dessus du seuil
    Given les comptes suivants existent pour ma boutique:
      | Nom      | Solde   | Seuil Alerte |
      | Caisse 1 | 150000  | 50000        |
      | BNP      | 500000  | 100000       |
    When je consulte le tableau de bord tresorerie
    Then je ne vois aucune alerte

  # ============================================
  # EVOLUTION SUR 6 MOIS
  # ============================================

  @backend @api
  Scenario: Voir le graphique d'evolution sur 6 mois
    Given nous sommes le "15/03/2024"
    And j'ai des mouvements sur les 6 derniers mois
    When je consulte le tableau de bord tresorerie
    Then je vois l'evolution du solde sur 6 mois:
      | Mois        | Solde   | Revenus | Depenses |
      | Octobre 23  | 400000  | 200000  | 150000   |
      | Novembre 23 | 450000  | 180000  | 130000   |
      | Decembre 23 | 480000  | 220000  | 190000   |
      | Janvier 24  | 550000  | 250000  | 180000   |
      | Fevrier 24  | 620000  | 200000  | 130000   |
      | Mars 24     | 700000  | 230000  | 150000   |

  # ============================================
  # SECURITE & FILTRAGE
  # ============================================

  @backend @api @security
  Scenario: Le dashboard filtre par boutique
    Given j'ai acces a la boutique "BOUTIQUE_A"
    And des comptes existent pour "BOUTIQUE_A" et "BOUTIQUE_B"
    When je consulte le tableau de bord tresorerie
    Then je vois uniquement les donnees de "BOUTIQUE_A"
    And je ne vois pas les donnees de "BOUTIQUE_B"

  @backend @api @security
  Scenario: Acces refuse sans authentification
    Given je ne suis pas authentifie
    When je tente de consulter le tableau de bord tresorerie
    Then je recois une erreur 401 Unauthorized

  @backend @api @security
  Scenario: Headers obligatoires
    Given je suis authentifie
    When je consulte "/api/reports/treasury-dashboard" sans le header "X-Boutique-Id"
    Then je recois une erreur 400 Bad Request
    And le message indique "L'en-tete X-Boutique-Id est obligatoire"

  # ============================================
  # FRONTEND
  # ============================================

  @frontend @ui
  Scenario: Affichage du tableau de bord tresorerie
    When je navigue vers la page "/tresorerie/dashboard"
    Then je vois une carte "Total Tresorerie" avec le montant total
    And je vois une carte par type de compte (Caisse, Banque, Mobile Money)
    And je vois une carte "Revenus du mois"
    And je vois une carte "Depenses du mois"
    And je vois une carte "Solde net"
    And je vois une section "Flux en attente"
    And je vois une section "Alertes" si des alertes existent
    And je vois un graphique d'evolution sur 6 mois

  @frontend @ui
  Scenario: Chargement du tableau de bord
    When je navigue vers la page "/tresorerie/dashboard"
    Then je vois un indicateur de chargement
    And les donnees se chargent en arriere-plan
    And l'indicateur disparait quand les donnees sont chargees

  @frontend @ui
  Scenario: Rafraichissement des donnees
    Given je suis sur le tableau de bord tresorerie
    When je clique sur le bouton "Rafraichir"
    Then les donnees sont rechargees depuis l'API
    And je vois un indicateur de rafraichissement
```

---

## Estimation

| Composant | Effort |
|-----------|--------|
| Backend - DTOs | 0.5h |
| Backend - Query + Handler | 2h |
| Backend - Endpoint | 0.5h |
| Backend - Tests | 2h |
| Frontend - Page + Composants | 3h |
| Frontend - Service HTTP | 0.5h |
| Frontend - Tests | 1.5h |
| **Total** | **~10h** |

---

## Decision

**ANALYSE VALIDE** - L'issue peut passer en developpement.

### Points de vigilance:
1. Le calcul d'evolution sur 6 mois peut etre couteux -> considerer une mise en cache ou calcul asynchrone
2. Les alertes "budget depasse" necessitent une fonctionnalite de budget (non mentionnee dans les criteres) -> a confirmer si necessaire

### Questions/Suggestions:
- Le critere "budgets depasses" n'est pas detaille. S'agit-il de budgets par categorie? A clarifier si necessaire pour une future issue.
