## Analyse US-038 : Exporter les flux de tresorerie

### Classification
| Attribut | Valeur |
|----------|--------|
| **Scope** | Microservice (TresorerieService) |
| **Type** | Nouvelle fonctionnalite |
| **Complexite** | Moyenne |
| **Statut** | VALIDE |

---

### Resume de l'analyse

L'implementation de l'export des flux de tresorerie (CSV/Excel) est **faisable** et bien alignee avec l'architecture existante du TresorerieService.

---

### Architecture existante analysee

#### Entite CashFlow
- **Table**: `TC00001`
- **Champs disponibles**: 35+ colonnes incluant Reference, Type, Status, CategoryId, Label, Description, Amount, TaxAmount, TaxRate, Currency, AccountId, DestinationAccountId, PaymentMethod, Date, ThirdPartyType, ThirdPartyName, ThirdPartyId, etc.
- **Fichier**: `TresorerieService.Domain/Entities/CashFlow.cs`

#### Query existante reutilisable
- **GetCashFlowsQuery**: Deja implementee avec tous les filtres necessaires
  - Filtres: Type, Status, AccountId, CategoryId, StartDate, EndDate, Search
  - Securite: Employee voit ses flux, Manager/Admin voit tous
  - Fichier: `TresorerieService.Application/Features/CashFlows/Queries/GetCashFlows/`

#### DTO existant
- **CashFlowListDto**: 17 champs pour la liste
- **CashFlowDetailDto**: 47 champs pour le detail complet

---

### Plan d'implementation

#### Fichiers a creer

| Fichier | Chemin | Description |
|---------|--------|-------------|
| `ExportCashFlowsQuery.cs` | `Application/Features/CashFlows/Queries/ExportCashFlows/` | Query CQRS |
| `ExportCashFlowsHandler.cs` | `Application/Features/CashFlows/Queries/ExportCashFlows/` | Handler generation export |
| `ExportCashFlowsEndpoint.cs` | `Api/Endpoints/CashFlows/` | Endpoint Carter |
| `CashFlowExportDto.cs` | `Application/Features/CashFlows/DTOs/` | DTO export avec noms FR |
| `ICsvExportService.cs` | `Application/Services/` | Interface service CSV |
| `IExcelExportService.cs` | `Application/Services/` | Interface service Excel |
| `CsvExportService.cs` | `Infrastructure/Services/` | Implementation CSV |
| `ExcelExportService.cs` | `Infrastructure/Services/` | Implementation Excel |

#### Packages a ajouter

| Package | Version | Projet | Usage |
|---------|---------|--------|-------|
| `CsvHelper` | 33.0.1 | Infrastructure | Generation CSV |
| `ClosedXML` | 0.104.2 | Infrastructure | Generation Excel (MIT, actif) |

#### Endpoint API

```
GET /api/cash-flows/export
```

**Query Parameters:**
| Parametre | Type | Defaut | Description |
|-----------|------|--------|-------------|
| `format` | string | csv | Format: `csv` ou `excel` |
| `columns` | string | (tous) | Colonnes a exporter (separees par virgule) |
| `type` | enum | - | Filtre par type (INCOME/EXPENSE/TRANSFER) |
| `status` | enum | - | Filtre par statut |
| `accountId` | guid | - | Filtre par compte |
| `categoryId` | guid | - | Filtre par categorie |
| `startDate` | datetime | - | Date de debut |
| `endDate` | datetime | - | Date de fin |
| `search` | string | - | Recherche textuelle |

**Headers obligatoires:**
- `Authorization: Bearer {JWT_TOKEN}`
- `X-Application-Id: {APPLICATION_ID}`
- `X-Boutique-Id: {BOUTIQUE_ID}`

**Reponses:**
- `200 OK` - Fichier CSV/Excel
- `400 Bad Request` - Parametres invalides
- `401 Unauthorized` - Non authentifie

---

### Colonnes exportees (par defaut)

| Nom FR | Champ source | Type |
|--------|--------------|------|
| Reference | Reference | string |
| Type | Type | enum -> string |
| Statut | Status | enum -> string |
| Categorie | CategoryName | string |
| Libelle | Label | string |
| Description | Description | string |
| Montant | Amount | decimal |
| Taxes | TaxAmount | decimal |
| Taux TVA | TaxRate | decimal |
| Devise | Currency | string |
| Compte | AccountName | string |
| Compte destination | DestinationAccountName | string |
| Mode de paiement | PaymentMethod | string |
| Date | Date | datetime |
| Type de tiers | ThirdPartyType | enum -> string |
| Nom du tiers | ThirdPartyName | string |
| ID tiers | ThirdPartyId | string |
| Cree le | CreatedAt | datetime |
| Cree par | CreatedBy | string |
| Soumis le | SubmittedAt | datetime |
| Soumis par | SubmittedBy | string |
| Valide le | ValidatedAt | datetime |
| Valide par | ValidatedBy | string |
| Raison de rejet | RejectionReason | string |
| Rapproche | IsReconciled | bool -> Oui/Non |
| Reference bancaire | BankStatementReference | string |

---

### Securite

- **Employee**: Exporte uniquement ses propres flux (`CreatedBy = UserId`)
- **Manager/Admin**: Exporte tous les flux de la boutique
- Logique identique a `GetCashFlowsHandler` existant

---

### Format du nom de fichier

- CSV: `flux-tresorerie-{YYYY-MM-DD}.csv`
- Excel: `flux-tresorerie-{YYYY-MM-DD}.xlsx`

---

### Scenarios Gherkin

Fichier cree: `.claude/features/US-038-ExportCashFlows.feature`

**25 scenarios couverts:**
- Export CSV (base, filtres, recherche)
- Export Excel (base, filtres combines)
- Verification des colonnes
- Selection des colonnes
- Nom de fichier avec date
- Cas d'erreur (format invalide, headers manquants, non authentifie)
- Securite (employee vs manager)
- Performance (grands volumes, streaming)

---

### Estimation de la complexite

| Tache | Effort |
|-------|--------|
| Query + Handler | 2h |
| Endpoint | 1h |
| Service CSV | 2h |
| Service Excel | 3h |
| DTOs et mapping | 1h |
| Tests unitaires | 3h |
| Tests integration | 2h |
| **Total** | **14h** |

---

### Dependances

Aucune dependance bloquante. Les fonctionnalites requises (filtrage, securite) sont deja implementees dans `GetCashFlowsQuery`.

---

### Risques identifies

| Risque | Mitigation |
|--------|------------|
| Performance sur gros volumes | Utiliser streaming + pagination interne |
| Memoire pour Excel | ClosedXML supporte le streaming |

---

### Decision

**VALIDE** - L'issue peut passer en implementation.
