## Analyse Technique - US-025

### Scope
**Microservice**: TresorerieService
**Type**: Tache planifiee (CRON Job)

### Resume de l analyse

L analyse du codebase revele que:

1. **L entite RecurringCashFlow existe** avec tous les champs necessaires:
   - `NextOccurrence` (DateTime) - Prochaine occurrence calculee
   - `AutoValidate` (bool) - Si true, Status = APPROVED
   - `LastGeneratedAt` (DateTime?) - Date de derniere generation
   - `IsActive` (bool) - Etat du flux recurrent

2. **L entite CashFlow** a les champs requis:
   - `IsRecurring` (bool) - fld29
   - `RecurringCashFlowId` (string?) - fld30
   - `IsSystemGenerated` (bool) - fld34
   - `AutoApproved` (bool) - fld35

3. **La logique d approbation existe** dans `ApproveCashFlowHandler.cs`:
   - Met a jour le solde du compte (credit/debit)
   - Cree une entree dans CashFlowHistory

4. **AUCUN Background Job n existe actuellement** - C est le coeur de cette US

### Fichiers a creer

```
Services/TresorerieService/
├── TresorerieService.Api/
│   └── BackgroundJobs/
│       └── RecurringCashFlowGeneratorJob.cs
│
├── TresorerieService.Application/
│   └── Features/
│       └── RecurringCashFlows/
│           └── Jobs/
│               ├── GenerateRecurringCashFlowsCommand.cs
│               └── GenerateRecurringCashFlowsHandler.cs
```

### Fichiers a modifier

| Fichier | Modification |
|---------|-------------|
| `TresorerieService.Api/Program.cs` | Enregistrer le HostedService/Quartz |
| `TresorerieService.Api/TresorerieService.Api.csproj` | Ajouter package si necessaire (ex: Quartz) |

### Packages suggeres

- **Option 1**: `Microsoft.Extensions.Hosting` (BackgroundService natif .NET)
- **Option 2**: `Quartz` (plus robuste, expressions CRON)

### Logique du Job

```
1. CHAQUE JOUR A MINUIT:
   a. Recuperer tous RecurringCashFlow WHERE:
      - IsActive = true
      - NextOccurrence <= DateTime.UtcNow.Date
      - (EndDate IS NULL OR EndDate >= DateTime.UtcNow.Date)

   b. Pour CHAQUE RecurringCashFlow:
      - Creer CashFlow avec:
        - IsRecurring = true
        - RecurringCashFlowId = RecurringCashFlow.Id
        - IsSystemGenerated = true
        - Date = NextOccurrence
        - Status = AutoValidate ? APPROVED : PENDING
        - AutoApproved = AutoValidate
        - Copier: Type, CategoryId, Label, Amount, AccountId, PaymentMethod, ThirdPartyName

      - Si AutoValidate = true:
        - Mettre a jour Account.CurrentBalance
        - Creer CashFlowHistory

      - Mettre a jour RecurringCashFlow:
        - LastGeneratedAt = DateTime.UtcNow
        - NextOccurrence = CalculateNextOccurrence()

   c. Sauvegarder toutes les modifications
```

### Calcul NextOccurrence (existant dans CreateRecurringCashFlowHandler)

La logique existe deja et doit etre reutilisee:
- DAILY: +interval jours
- WEEKLY: +interval semaines (ajuste au DayOfWeek)
- MONTHLY: +interval mois (ajuste au DayOfMonth)
- QUARTERLY: +3*interval mois
- YEARLY: +interval annees

---

## Scenarios Gherkin

```gherkin
Feature: US-025 Generation automatique des flux recurrents

  En tant que systeme
  Je veux generer automatiquement les flux recurrents
  Afin d automatiser le processus

  Background:
    Given une boutique "B001" avec l application "APP001"
    And un compte "Compte Principal" avec un solde de 10000.00 XOF
    And une categorie "Loyer" de type EXPENSE

  # =====================================================
  # Scenario 1: Job s execute a minuit
  # =====================================================
  @job @cron
  Scenario: Le job s execute chaque jour a minuit
    Given le systeme est configure avec un job planifie
    When l heure systeme atteint minuit (00:00 UTC)
    Then le job de generation des flux recurrents doit s executer

  # =====================================================
  # Scenario 2: Generation avec AutoValidate = false
  # =====================================================
  @generation @pending
  Scenario: Generation d un flux recurrent sans validation automatique
    Given un flux recurrent "Loyer Mensuel" de 500000.00 XOF
    And la frequence est MONTHLY avec intervalle 1 et jour 15
    And AutoValidate est false
    And NextOccurrence est le "2024-01-15"
    And la date du jour est le "2024-01-15"
    When le job de generation s execute
    Then un nouveau CashFlow doit etre cree avec:
      | Champ                | Valeur           |
      | Type                 | EXPENSE          |
      | Amount               | 500000.00        |
      | Status               | PENDING          |
      | IsRecurring          | true             |
      | IsSystemGenerated    | true             |
      | AutoApproved         | false            |
      | Date                 | 2024-01-15       |
    And le solde du compte doit rester 10000.00 XOF
    And NextOccurrence du flux recurrent doit etre "2024-02-15"
    And LastGeneratedAt doit etre la date/heure actuelle

  # =====================================================
  # Scenario 3: Generation avec AutoValidate = true
  # =====================================================
  @generation @approved @compte
  Scenario: Generation d un flux recurrent avec validation automatique
    Given un flux recurrent "Abonnement Internet" de 25000.00 XOF
    And la frequence est MONTHLY avec intervalle 1 et jour 1
    And AutoValidate est true
    And NextOccurrence est le "2024-02-01"
    And la date du jour est le "2024-02-01"
    When le job de generation s execute
    Then un nouveau CashFlow doit etre cree avec:
      | Champ                | Valeur           |
      | Type                 | EXPENSE          |
      | Amount               | 25000.00         |
      | Status               | APPROVED         |
      | IsRecurring          | true             |
      | IsSystemGenerated    | true             |
      | AutoApproved         | true             |
    And le solde du compte doit etre 9975000.00 XOF (10000.00 - 25000.00)
    And une entree CashFlowHistory doit etre creee avec Action = APPROVED
    And NextOccurrence du flux recurrent doit etre "2024-03-01"

  # =====================================================
  # Scenario 4: Flux INCOME avec AutoValidate
  # =====================================================
  @generation @income
  Scenario: Generation d un revenu recurrent avec validation automatique
    Given un flux recurrent "Loyer Percu" de type INCOME de 150000.00 XOF
    And la frequence est MONTHLY avec intervalle 1 et jour 5
    And AutoValidate est true
    And NextOccurrence est le "2024-01-05"
    And la date du jour est le "2024-01-05"
    When le job de generation s execute
    Then un nouveau CashFlow doit etre cree avec:
      | Champ                | Valeur           |
      | Type                 | INCOME           |
      | Status               | APPROVED         |
    And le solde du compte doit etre 160000.00 XOF (10000.00 + 150000.00)

  # =====================================================
  # Scenario 5: Flux desactive non genere
  # =====================================================
  @skip @inactive
  Scenario: Un flux recurrent desactive n est pas genere
    Given un flux recurrent "Ancien Loyer" de 300000.00 XOF
    And IsActive est false
    And NextOccurrence est le "2024-01-15"
    And la date du jour est le "2024-01-15"
    When le job de generation s execute
    Then aucun CashFlow ne doit etre cree pour ce flux recurrent
    And NextOccurrence doit rester "2024-01-15"

  # =====================================================
  # Scenario 6: Flux avec EndDate depassee
  # =====================================================
  @skip @ended
  Scenario: Un flux recurrent avec date de fin depassee n est pas genere
    Given un flux recurrent "Contrat Termine" de 100000.00 XOF
    And IsActive est true
    And EndDate est le "2024-01-01"
    And NextOccurrence est le "2024-01-15"
    And la date du jour est le "2024-01-15"
    When le job de generation s execute
    Then aucun CashFlow ne doit etre cree pour ce flux recurrent

  # =====================================================
  # Scenario 7: Frequence DAILY
  # =====================================================
  @frequency @daily
  Scenario: Generation d un flux quotidien
    Given un flux recurrent "Depense Quotidienne" de 5000.00 XOF
    And la frequence est DAILY avec intervalle 1
    And NextOccurrence est le "2024-01-15"
    And la date du jour est le "2024-01-15"
    When le job de generation s execute
    Then NextOccurrence du flux recurrent doit etre "2024-01-16"

  # =====================================================
  # Scenario 8: Frequence WEEKLY
  # =====================================================
  @frequency @weekly
  Scenario: Generation d un flux hebdomadaire (tous les lundis)
    Given un flux recurrent "Depense Hebdomadaire" de 10000.00 XOF
    And la frequence est WEEKLY avec intervalle 1 et DayOfWeek 1 (Lundi)
    And NextOccurrence est le "2024-01-15" (un Lundi)
    And la date du jour est le "2024-01-15"
    When le job de generation s execute
    Then NextOccurrence du flux recurrent doit etre "2024-01-22" (Lundi suivant)

  # =====================================================
  # Scenario 9: Frequence QUARTERLY
  # =====================================================
  @frequency @quarterly
  Scenario: Generation d un flux trimestriel
    Given un flux recurrent "Charges Trimestrielles" de 75000.00 XOF
    And la frequence est QUARTERLY avec intervalle 1
    And NextOccurrence est le "2024-01-01"
    And la date du jour est le "2024-01-01"
    When le job de generation s execute
    Then NextOccurrence du flux recurrent doit etre "2024-04-01" (+3 mois)

  # =====================================================
  # Scenario 10: Frequence YEARLY
  # =====================================================
  @frequency @yearly
  Scenario: Generation d un flux annuel
    Given un flux recurrent "Assurance Annuelle" de 500000.00 XOF
    And la frequence est YEARLY avec intervalle 1
    And NextOccurrence est le "2024-01-01"
    And la date du jour est le "2024-01-01"
    When le job de generation s execute
    Then NextOccurrence du flux recurrent doit etre "2025-01-01" (+1 an)

  # =====================================================
  # Scenario 11: NextOccurrence dans le passe (rattrapage)
  # =====================================================
  @rattrapage
  Scenario: Generation pour une NextOccurrence passee (rattrapage)
    Given un flux recurrent "Loyer Oublie" de 500000.00 XOF
    And la frequence est MONTHLY avec intervalle 1 et jour 1
    And NextOccurrence est le "2024-01-01"
    And la date du jour est le "2024-01-05"
    When le job de generation s execute
    Then un nouveau CashFlow doit etre cree avec Date = "2024-01-01"
    And NextOccurrence du flux recurrent doit etre "2024-02-01"

  # =====================================================
  # Scenario 12: Plusieurs flux recurrents le meme jour
  # =====================================================
  @batch
  Scenario: Generation de plusieurs flux recurrents le meme jour
    Given les flux recurrents suivants avec NextOccurrence = "2024-01-15":
      | Label              | Amount    | AutoValidate |
      | Loyer              | 500000.00 | false        |
      | Internet           | 25000.00  | true         |
      | Salaire Employe    | 150000.00 | true         |
    And la date du jour est le "2024-01-15"
    When le job de generation s execute
    Then 3 nouveaux CashFlows doivent etre crees
    And chaque flux recurrent doit avoir sa NextOccurrence mise a jour

  # =====================================================
  # Scenario 13: Intervalle > 1
  # =====================================================
  @intervalle
  Scenario: Generation avec intervalle de 2 mois
    Given un flux recurrent "Paiement Bimestriel" de 100000.00 XOF
    And la frequence est MONTHLY avec intervalle 2 et jour 1
    And NextOccurrence est le "2024-01-01"
    And la date du jour est le "2024-01-01"
    When le job de generation s execute
    Then NextOccurrence du flux recurrent doit etre "2024-03-01" (+2 mois)

  # =====================================================
  # Scenario 14: Jour du mois > 28 (gestion fin de mois)
  # =====================================================
  @edge-case @fin-de-mois
  Scenario: Generation pour le jour 31 dans un mois de 30 jours
    Given un flux recurrent "Fin de Mois" de 50000.00 XOF
    And la frequence est MONTHLY avec intervalle 1 et jour 31
    And NextOccurrence est le "2024-01-31"
    And la date du jour est le "2024-01-31"
    When le job de generation s execute
    Then NextOccurrence du flux recurrent doit etre "2024-02-29" (dernier jour de fevrier 2024)
```

---

## Estimation de complexite

| Element | Estimation |
|---------|------------|
| Command + Handler | Moyenne |
| BackgroundService | Moyenne |
| Tests unitaires | Elevee (nombreux scenarios) |
| Integration tests | Moyenne |

### Points d attention

1. **Transaction**: Toutes les operations doivent etre dans une meme transaction
2. **Idempotence**: Eviter de generer plusieurs fois le meme flux si le job est relance
3. **Logs**: Logger chaque flux genere pour audit
4. **Gestion des erreurs**: Un flux en erreur ne doit pas bloquer les autres

---

## Validation

:white_check_mark: L analyse est complete
:white_check_mark: Les entites requises existent
:white_check_mark: La logique d approbation existe
:white_check_mark: Les scenarios Gherkin couvrent les criteres d acceptation

**Statut: VALIDE - Pret pour implementation**
