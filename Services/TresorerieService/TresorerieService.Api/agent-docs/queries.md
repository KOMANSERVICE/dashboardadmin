# Queries CQRS - TresorerieService

## GetAccountsQuery
Liste des comptes de tresorerie.

**Input:**
- ApplicationId (string, required)
- BoutiqueId (string, required)
- Type (AccountType?, optional)
- IsActive (bool?, optional)

**Output:** GetAccountsResponse
- Accounts: Liste de AccountListDto

## GetAccountDetailQuery
Detail d'un compte.

**Input:**
- ApplicationId (string, required)
- BoutiqueId (string, required)
- AccountId (Guid, required)

**Output:** GetAccountDetailResponse
- Account: AccountDTO

## GetAccountBalanceQuery
Solde actuel d'un compte.

**Input:**
- ApplicationId (string, required)
- BoutiqueId (string, required)
- AccountId (Guid, required)

**Output:** AccountBalanceDto

## GetCategoriesQuery
Liste des categories.

**Input:**
- ApplicationId (string, required)
- Type (CategoryType?, optional)

**Output:** GetCategoriesResponse
- Categories: Liste de CategoryDTO

## GetCashFlowsQuery
Liste des flux avec pagination et filtres.

**Input:**
- ApplicationId (string, required)
- BoutiqueId (string, required)
- UserId (string, required)
- IsManager (bool, required)
- Type (CashFlowType?, optional)
- Status (CashFlowStatus?, optional)
- AccountId (Guid?, optional)
- CategoryId (Guid?, optional)
- StartDate (DateTime?, optional)
- EndDate (DateTime?, optional)
- Search (string?, optional)
- Page (int, default=1)
- PageSize (int, default=20)
- SortBy (string, default="date")
- SortOrder (string, default="desc")

**Output:** GetCashFlowsResponse
- CashFlows: Liste de CashFlowListDto
- TotalCount, Page, PageSize, TotalPages, HasPrevious, HasNext

## GetCashFlowDetailQuery
Detail d'un flux.

**Input:**
- ApplicationId (string, required)
- BoutiqueId (string, required)
- CashFlowId (Guid, required)

**Output:** GetCashFlowDetailResponse
- CashFlow: CashFlowDTO

## GetPendingCashFlowsQuery
Liste des flux en attente de validation (Status=PENDING).
Tri par date de soumission (les plus anciens en premier).

**Input:**
- ApplicationId (string, required)
- BoutiqueId (string, required)
- Type (CashFlowType?, optional) - Filtrer par type
- AccountId (Guid?, optional) - Filtrer par compte

**Output:** GetPendingCashFlowsResponse
- CashFlows: Liste de PendingCashFlowDto
- PendingCount: Nombre total de flux en attente

## GetUnreconciledCashFlowsQuery
Liste des flux non reconcilies (Status=APPROVED et IsReconciled=false).
Tri par date (les plus anciens en premier).

**Input:**
- ApplicationId (string, required)
- BoutiqueId (string, required)
- AccountId (Guid?, optional) - Filtrer par compte
- StartDate (DateTime?, optional) - Date de debut de periode
- EndDate (DateTime?, optional) - Date de fin de periode

**Output:** GetUnreconciledCashFlowsResponse
- CashFlows: Liste de UnreconciledCashFlowDto
- UnreconciledCount: Nombre total de flux non reconcilies
- TotalUnreconciledAmount: Montant total non reconcilie
