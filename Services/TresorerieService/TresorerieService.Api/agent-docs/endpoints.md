# Endpoints - TresorerieService

## Accounts

### GET /api/accounts
Liste des comptes de tresorerie.

**Headers:**
- X-Application-Id (required)
- X-Boutique-Id (required)

**Query params:**
- type (AccountType?, optional)
- isActive (bool?, optional)

### POST /api/accounts
Creer un compte.

**Headers:**
- X-Application-Id (required)
- X-Boutique-Id (required)

**Body:** CreateAccountCommand

### GET /api/accounts/{id}
Detail d'un compte.

### PUT /api/accounts/{id}
Modifier un compte.

### GET /api/accounts/{id}/balance
Solde d'un compte.

---

## Categories

### GET /api/categories
Liste des categories.

**Headers:**
- X-Application-Id (required)

### POST /api/categories
Creer une categorie.

---

## CashFlows

### GET /api/cash-flows
Liste des flux de tresorerie avec pagination et filtres.

**Headers:**
- X-Application-Id (required)
- X-Boutique-Id (required)

**Query params:**
- type (CashFlowType?, optional)
- status (CashFlowStatus?, optional)
- accountId (Guid?, optional)
- categoryId (Guid?, optional)
- startDate (DateTime?, optional)
- endDate (DateTime?, optional)
- search (string?, optional)
- page (int, default=1)
- pageSize (int, default=20)
- sortBy (string, default="date")
- sortOrder (string, default="desc")

### GET /api/cash-flows/pending
Liste des flux en attente de validation (Status=PENDING).
Tries par date de soumission (les plus anciens en premier).

**Headers:**
- X-Application-Id (required)
- X-Boutique-Id (required)

**Query params:**
- type (CashFlowType?, optional)
- accountId (Guid?, optional)

**Response:**
- CashFlows: Liste de PendingCashFlowDto
- PendingCount: Nombre de flux en attente

### POST /api/cash-flows
Creer un flux.

### GET /api/cash-flows/{id}
Detail d'un flux.

### PUT /api/cash-flows/{id}
Modifier un flux (brouillon uniquement).

### DELETE /api/cash-flows/{id}
Supprimer un flux (brouillon uniquement).

### POST /api/cash-flows/transfer
Creer un transfert entre comptes.

### POST /api/cash-flows/{id}/submit
Soumettre un flux pour validation (DRAFT -> PENDING).

### POST /api/cash-flows/{id}/approve
Approuver un flux (PENDING -> APPROVED).

### POST /api/cash-flows/{id}/reject
Rejeter un flux (PENDING -> REJECTED).

### POST /api/cash-flows/{id}/reconcile
Reconcilier un flux approuve avec le releve bancaire.

**Headers:**
- X-Application-Id (required)
- X-Boutique-Id (required)
- Authorization: Bearer {JWT} (required)

**Body:**
- bankStatementReference (string?, optional)

**Response:** CashFlowDetailDto

**Notes:**
- Seul un manager ou admin peut reconcilier
- Le flux doit etre APPROVED et non reconcilie
- Met a jour IsReconciled, ReconciledAt, ReconciledBy, BankStatementReference

### GET /api/cash-flows/unreconciled
Liste des flux non reconcilies (APPROVED et IsReconciled=false).
Tries par date (les plus anciens en premier).

**Headers:**
- X-Application-Id (required)
- X-Boutique-Id (required)

**Query params:**
- accountId (Guid?, optional) - Filtrer par compte
- startDate (DateTime?, optional) - Date de debut
- endDate (DateTime?, optional) - Date de fin

**Response:**
- CashFlows: Liste de UnreconciledCashFlowDto
- UnreconciledCount: Nombre de flux non reconcilies
- TotalUnreconciledAmount: Montant total non reconcilie

### POST /api/cash-flows/reconcile
Reconcilier plusieurs flux en masse.

**Headers:**
- X-Application-Id (required)
- X-Boutique-Id (required)
- Authorization: Bearer {JWT} (required)

**Body:**
- cashFlowIds (Guid[], required) - Liste des IDs de flux a reconcilier
- bankStatementReference (string?, optional) - Reference du releve bancaire

**Response:**
- ReconciledCount: Nombre de flux reconcilies
- ReconciledCashFlows: Liste de ReconciledCashFlowDto

**Notes:**
- Seul un manager ou admin peut reconcilier
- Tous les flux doivent etre APPROVED et non reconcilies
- Transaction atomique (tout ou rien)
