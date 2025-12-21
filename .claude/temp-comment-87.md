## 🔍 Analyse de l'issue #87 - US-036 : Prévisions de trésorerie

### ✅ Statut : VALIDE - Prête pour implémentation

---

### 📋 Classification

| Attribut | Valeur |
|----------|--------|
| **Scope** | Microservice |
| **Service** | TresorerieService |
| **Type** | Nouvelle fonctionnalité (Query CQRS) |
| **Endpoint** | `GET /api/reports/cash-flow-forecast?days=30` |
| **Complexité** | Moyenne |

---

### 🏗️ Architecture existante utilisée

Le TresorerieService existe et contient déjà :
- ✅ Entité `Account` avec `CurrentBalance`, `AlertThreshold`
- ✅ Entité `CashFlow` avec `Status` (PENDING, APPROVED, etc.), `Date`, `Amount`, `Type`
- ✅ Entité `RecurringCashFlow` avec `NextOccurrence`, `Frequency`, `Interval`, `Amount`, `Type`
- ✅ Pattern CQRS avec IDR.Library.BuildingBlocks
- ✅ Authentification JWT + Headers `X-Application-Id`, `X-Boutique-Id`

---

### 📁 Fichiers à créer

```
Services/TresorerieService/TresorerieService.Application/Features/Reports/
├── Queries/
│   └── GetCashFlowForecast/
│       ├── GetCashFlowForecastQuery.cs
│       ├── GetCashFlowForecastHandler.cs
│       └── GetCashFlowForecastValidator.cs
└── DTOs/
    ├── CashFlowForecastDto.cs
    └── DailyForecastDto.cs

Services/TresorerieService/TresorerieService.Api/Endpoints/Reports/
└── GetCashFlowForecastEndpoint.cs
```

---

### 📊 Structure de la réponse proposée

```json
{
  "success": true,
  "data": {
    "startDate": "2024-01-15",
    "endDate": "2024-02-14",
    "days": 30,
    "currency": "XOF",
    "currentBalance": 1500000.00,
    "forecastedEndBalance": 1250000.00,
    "hasNegativeRisk": true,
    "criticalDates": [
      {
        "date": "2024-01-28",
        "forecastedBalance": -50000.00,
        "reason": "Loyer mensuel"
      }
    ],
    "dailyForecast": [
      {
        "date": "2024-01-15",
        "openingBalance": 1500000.00,
        "income": 0.00,
        "expense": 0.00,
        "pendingIncome": 50000.00,
        "pendingExpense": 0.00,
        "recurringIncome": 0.00,
        "recurringExpense": 0.00,
        "closingBalance": 1500000.00,
        "isNegative": false,
        "isCritical": false
      }
    ],
    "summary": {
      "totalForecastedIncome": 500000.00,
      "totalForecastedExpense": 750000.00,
      "totalRecurringIncome": 200000.00,
      "totalRecurringExpense": 450000.00,
      "totalPendingIncome": 100000.00,
      "totalPendingExpense": 50000.00,
      "netVariation": -250000.00
    },
    "includePending": true,
    "calculatedAt": "2024-01-15T10:30:00Z"
  },
  "message": "Previsions de tresorerie calculees avec succes"
}
```

---

### 🧪 Scénarios Gherkin

```gherkin
Feature: US-036 Prévisions de trésorerie
  En tant que manager
  Je veux voir les prévisions de trésorerie
  Afin d'anticiper les besoins

  Background:
    Given un utilisateur authentifié avec le rôle "manager"
    And les headers "X-Application-Id" et "X-Boutique-Id" sont fournis
    And un compte de trésorerie existe avec un solde de 1500000 XOF
    And un seuil d'alerte de 100000 XOF

  # --- Critère 1: Solde prévisionnel pour les 30 prochains jours ---

  Scenario: Consulter les prévisions sur 30 jours par défaut
    Given des flux récurrents actifs existent
    When je consulte "GET /api/reports/cash-flow-forecast"
    Then je reçois un statut 200
    And la réponse contient "dailyForecast" avec 30 entrées
    And chaque jour contient "date", "openingBalance", "closingBalance"
    And "forecastedEndBalance" correspond au solde du dernier jour

  Scenario: Consulter les prévisions sur une période personnalisée
    When je consulte "GET /api/reports/cash-flow-forecast?days=7"
    Then je reçois un statut 200
    And la réponse contient "dailyForecast" avec 7 entrées
    And "endDate" est 7 jours après "startDate"

  Scenario: Limite maximale de jours de prévision
    When je consulte "GET /api/reports/cash-flow-forecast?days=365"
    Then je reçois un statut 400
    And le message d'erreur indique "days doit être entre 1 et 90"

  # --- Critère 2: Flux récurrents pris en compte ---

  Scenario: Inclure les flux récurrents INCOME dans les prévisions
    Given un flux récurrent INCOME de 100000 XOF mensuel le 20 du mois
    And la date actuelle est le 15 janvier
    When je consulte "GET /api/reports/cash-flow-forecast?days=30"
    Then le jour 20 janvier contient "recurringIncome" de 100000
    And "closingBalance" du 20 janvier augmente de 100000

  Scenario: Inclure les flux récurrents EXPENSE dans les prévisions
    Given un flux récurrent EXPENSE de 500000 XOF mensuel le 25 du mois (loyer)
    When je consulte "GET /api/reports/cash-flow-forecast?days=30"
    Then le jour 25 contient "recurringExpense" de 500000
    And "closingBalance" du 25 diminue de 500000

  Scenario: Gérer les flux récurrents avec fréquence hebdomadaire
    Given un flux récurrent EXPENSE de 50000 XOF hebdomadaire le lundi
    When je consulte "GET /api/reports/cash-flow-forecast?days=30"
    Then les lundis contiennent "recurringExpense" de 50000

  Scenario: Ignorer les flux récurrents inactifs
    Given un flux récurrent EXPENSE inactif de 200000 XOF
    When je consulte "GET /api/reports/cash-flow-forecast?days=30"
    Then ce flux n'apparaît dans aucun jour de prévision

  Scenario: Respecter la date de fin des flux récurrents
    Given un flux récurrent INCOME qui se termine le 20 janvier
    And la date actuelle est le 15 janvier
    When je consulte "GET /api/reports/cash-flow-forecast?days=30"
    Then le flux n'apparaît pas après le 20 janvier

  # --- Critère 3: Flux PENDING inclus (optionnel) ---

  Scenario: Inclure les flux PENDING par défaut
    Given un flux INCOME en statut PENDING de 200000 XOF prévu le 18 janvier
    When je consulte "GET /api/reports/cash-flow-forecast?days=30"
    Then le jour 18 janvier contient "pendingIncome" de 200000
    And "includePending" est true

  Scenario: Exclure les flux PENDING sur demande
    Given un flux EXPENSE en statut PENDING de 100000 XOF prévu le 22 janvier
    When je consulte "GET /api/reports/cash-flow-forecast?days=30&includePending=false"
    Then le jour 22 janvier ne contient pas ce flux PENDING
    And "includePending" est false

  Scenario: Les flux APPROVED ne sont pas dupliqués
    Given un flux APPROVED de 50000 XOF enregistré hier
    When je consulte "GET /api/reports/cash-flow-forecast?days=30"
    Then ce flux n'apparaît pas dans les prévisions futures
    And le solde actuel du compte inclut déjà ce flux

  # --- Critère 4: Avertissement si risque de trésorerie négative ---

  Scenario: Détecter un risque de trésorerie négative
    Given un solde actuel de 500000 XOF
    And un flux récurrent EXPENSE de 600000 XOF le 20 du mois
    When je consulte "GET /api/reports/cash-flow-forecast?days=30"
    Then "hasNegativeRisk" est true
    And le jour 20 a "isNegative" à true
    And "closingBalance" du 20 est -100000

  Scenario: Aucun risque de trésorerie négative
    Given un solde actuel de 2000000 XOF
    And des flux récurrents EXPENSE totalisant 500000 XOF
    When je consulte "GET /api/reports/cash-flow-forecast?days=30"
    Then "hasNegativeRisk" est false
    And aucun jour n'a "isNegative" à true

  # --- Critère 5: Dates où la trésorerie sera critique ---

  Scenario: Identifier les dates critiques (solde négatif)
    Given un solde actuel de 300000 XOF
    And un flux EXPENSE récurrent de 400000 le 25
    When je consulte "GET /api/reports/cash-flow-forecast?days=30"
    Then "criticalDates" contient une entrée pour le 25
    And l'entrée contient "forecastedBalance" négatif
    And l'entrée contient "reason" avec le libellé du flux

  Scenario: Identifier les dates critiques (solde sous le seuil d'alerte)
    Given un solde actuel de 500000 XOF
    And un seuil d'alerte de 100000 XOF
    And un flux EXPENSE de 450000 le 28
    When je consulte "GET /api/reports/cash-flow-forecast?days=30"
    Then le jour 28 a "isCritical" à true
    And "criticalDates" contient le 28

  Scenario: Plusieurs dates critiques identifiées
    Given un solde actuel de 200000 XOF
    And un flux EXPENSE de 250000 le 15
    And un flux INCOME de 100000 le 18
    And un flux EXPENSE de 100000 le 25
    When je consulte "GET /api/reports/cash-flow-forecast?days=30"
    Then "criticalDates" contient au moins 2 entrées

  # --- Cas d'erreur ---

  Scenario: Erreur si header X-Application-Id manquant
    When je consulte "GET /api/reports/cash-flow-forecast" sans header "X-Application-Id"
    Then je reçois un statut 400
    And le message d'erreur indique "X-Application-Id est obligatoire"

  Scenario: Erreur si header X-Boutique-Id manquant
    When je consulte "GET /api/reports/cash-flow-forecast" sans header "X-Boutique-Id"
    Then je reçois un statut 400
    And le message d'erreur indique "X-Boutique-Id est obligatoire"

  Scenario: Erreur si non authentifié
    Given un utilisateur non authentifié
    When je consulte "GET /api/reports/cash-flow-forecast"
    Then je reçois un statut 401

  Scenario: Erreur si aucun compte de trésorerie
    Given aucun compte de trésorerie n'existe pour cette boutique
    When je consulte "GET /api/reports/cash-flow-forecast"
    Then je reçois un statut 404
    And le message indique "Aucun compte de tresorerie trouve"

  # --- Résumé des prévisions ---

  Scenario: Voir le résumé des totaux prévisionnels
    When je consulte "GET /api/reports/cash-flow-forecast?days=30"
    Then "summary" contient:
      | Champ                 | Description                        |
      | totalForecastedIncome | Total des revenus prévus           |
      | totalForecastedExpense| Total des dépenses prévues         |
      | totalRecurringIncome  | Total revenus récurrents           |
      | totalRecurringExpense | Total dépenses récurrentes         |
      | totalPendingIncome    | Total revenus en attente           |
      | totalPendingExpense   | Total dépenses en attente          |
      | netVariation          | Variation nette sur la période     |
```

---

### 🔧 Algorithme de calcul proposé

```
Pour chaque jour de la période (J = aujourd'hui à aujourd'hui + N jours):

1. Solde d'ouverture = Solde de fermeture du jour précédent
   (Premier jour = CurrentBalance du compte)

2. Calculer les flux récurrents du jour J:
   - Pour chaque RecurringCashFlow actif où:
     - StartDate <= J
     - EndDate est null ou EndDate >= J
     - NextOccurrence calculée tombe sur J
   - Ajouter au recurringIncome (si INCOME) ou recurringExpense (si EXPENSE)

3. Calculer les flux PENDING du jour J (si includePending=true):
   - Pour chaque CashFlow où Status=PENDING et Date=J
   - Ajouter au pendingIncome ou pendingExpense selon Type

4. Solde de fermeture = Ouverture + Income - Expense

5. Marquer:
   - isNegative = true si closingBalance < 0
   - isCritical = true si closingBalance < AlertThreshold
```

---

### ⚠️ Points d'attention

1. **Performance**: Limiter `days` à 90 max pour éviter des calculs trop lourds
2. **Fuseaux horaires**: Utiliser UTC pour tous les calculs de dates
3. **Devise**: Respecter la devise du compte (XOF par défaut)
4. **Multi-comptes**: Option future pour agréger plusieurs comptes

---

### 📦 Packages IDR utilisés

- `IDR.Library.BuildingBlocks.CQRS` - IQuery, IQueryHandler
- `IDR.Library.BuildingBlocks.Repositories` - IGenericRepository
- `IDR.Library.BuildingBlocks.Responses` - ResponseFactory
- `FluentValidation` - AbstractValidator

---

### ✅ Prêt pour implémentation

Cette issue est **VALIDE** et peut être déplacée vers **Todo**.
