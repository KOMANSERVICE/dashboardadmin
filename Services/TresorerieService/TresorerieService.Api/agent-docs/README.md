# TresorerieService

Service de gestion de la tresorerie pour DashBoardAdmin.

## Description

TresorerieService gere les flux de tresorerie (CashFlows), les comptes bancaires (Accounts), et les categories de depenses/revenus.

## Entites

### CashFlow
Flux de tresorerie avec workflow de validation.

- **Statuts**: DRAFT, PENDING, APPROVED, REJECTED, CANCELLED
- **Types**: INCOME, EXPENSE, TRANSFER

### Account
Comptes bancaires ou caisses.

- **Types**: BANK, CASH, MOBILE_MONEY

### Category
Categories de flux (revenus, depenses).

## Endpoints

### Accounts
- `GET /api/accounts` - Lister les comptes
- `POST /api/accounts` - Creer un compte
- `GET /api/accounts/{id}` - Detail d'un compte
- `PUT /api/accounts/{id}` - Modifier un compte
- `GET /api/accounts/{id}/balance` - Solde d'un compte

### Categories
- `GET /api/categories` - Lister les categories
- `POST /api/categories` - Creer une categorie

### CashFlows
- `GET /api/cash-flows` - Lister les flux de tresorerie
- `POST /api/cash-flows` - Creer un flux
- `PUT /api/cash-flows/{id}` - Modifier un flux
- `DELETE /api/cash-flows/{id}` - Supprimer un flux (brouillon)
- `GET /api/cash-flows/{id}` - Detail d'un flux
- `POST /api/cash-flows/transfer` - Creer un transfert
- `POST /api/cash-flows/{id}/submit` - Soumettre pour validation
- `POST /api/cash-flows/{id}/approve` - Approuver un flux
- `POST /api/cash-flows/{id}/reject` - Rejeter un flux
- `GET /api/cash-flows/pending` - Lister les flux en attente de validation

## Queries CQRS

- `GetAccountsQuery` - Liste des comptes
- `GetAccountDetailQuery` - Detail d'un compte
- `GetAccountBalanceQuery` - Solde d'un compte
- `GetCategoriesQuery` - Liste des categories
- `GetCashFlowsQuery` - Liste des flux avec pagination et filtres
- `GetCashFlowDetailQuery` - Detail d'un flux
- `GetPendingCashFlowsQuery` - Liste des flux en attente (Status=PENDING)

## Commands CQRS

- `CreateAccountCommand` - Creer un compte
- `UpdateAccountCommand` - Modifier un compte
- `CreateCategoryCommand` - Creer une categorie
- `CreateCashFlowCommand` - Creer un flux
- `CreateTransferCommand` - Creer un transfert entre comptes
- `UpdateCashFlowCommand` - Modifier un flux
- `DeleteCashFlowCommand` - Supprimer un flux brouillon
- `SubmitCashFlowCommand` - Soumettre un flux pour validation
- `ApproveCashFlowCommand` - Approuver un flux
- `RejectCashFlowCommand` - Rejeter un flux

## Authentification

Headers requis:
- `Authorization: Bearer {JWT_TOKEN}`
- `X-Application-Id: {APPLICATION_ID}`
- `X-Boutique-Id: {BOUTIQUE_ID}`
