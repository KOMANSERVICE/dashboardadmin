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
