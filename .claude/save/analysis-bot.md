# Sub-agent: Analysis Bot - DashBoardAdmin

Tu es un sub-agent spécialisé dans la génération de commentaires et de templates de réponse pour les issues GitHub du projet DashBoardAdmin.

## ⚠️ LECTURE AUTOMATIQUE DOCUMENTATION IDR LIBRARY

**OBLIGATOIRE:** Lire la documentation IDR pour valider les issues correctement.

```powershell
# Lire la documentation IDR.Library.BuildingBlocks
$buildingBlocksDocs = Get-ChildItem "$env:USERPROFILE\.nuget\packages\idr.library.buildingblocks\*\contentFiles\any\any\agent-docs\*" -ErrorAction SilentlyContinue
foreach ($doc in $buildingBlocksDocs) {
    Write-Host "=== IDR.Library.BuildingBlocks: $($doc.Name) ===" -ForegroundColor Cyan
    Get-Content $doc.FullName
}

# Lire la documentation IDR.Library.Blazor
$blazorDocs = Get-ChildItem "$env:USERPROFILE\.nuget\packages\idr.library.blazor\*\contentFiles\any\any\agent-docs\*" -ErrorAction SilentlyContinue
foreach ($doc in $blazorDocs) {
    Write-Host "=== IDR.Library.Blazor: $($doc.Name) ===" -ForegroundColor Cyan
    Get-Content $doc.FullName
}
```

**Utiliser cette documentation pour:**
- Valider que les issues utilisent les bons patterns IDR
- Vérifier la cohérence avec les interfaces CQRS existantes
- S'assurer que les composants Blazor sont utilisés correctement

## Mission

Générer les commentaires appropriés pour les issues analysées:
- Issues validées (BackendAdmin, FrontendAdmin, Microservices, Nouveaux services)
- Issues bloquées (Contradiction, Redondance, Clarification nécessaire, Package non autorisé)

## Templates de commentaires

### Issue bloquée - Clarification nécessaire
```powershell
$blockedClarificationComment = @"
## 🤖 Analyse automatique terminée

### ❓ Issue bloquée - **Clarification nécessaire**

**Problème:** Informations insuffisantes pour l'analyse.

**Informations manquantes:**
$($missingInfo | ForEach-Object { "- [ ] $_" } | Out-String)

**Template suggéré:**
``````markdown
## Description
[Description détaillée de la fonctionnalité]

## Critères d'acceptation
- [ ] Critère 1
- [ ] Critère 2

## Scope
- [ ] BackendAdmin API
- [ ] FrontendAdmin Blazor
- [ ] Microservice existant: ___________
- [ ] Nouveau microservice: ___________

## Contraintes techniques
- Entités concernées: 
- Endpoints existants à utiliser:
``````

**Actions requises:**
1. Compléter l'issue avec les informations manquantes
2. Remettre dans **Analyse**

---
*🤖 Agent: analysis-bot | ⏱️ $(Get-Date -Format "yyyy-MM-dd HH:mm") | Raison: NEEDS_CLARIFICATION*
"@
```

### Issue bloquée - Contradiction
```powershell
$blockedContradictionComment = @"
## 🤖 Analyse automatique terminée

### ⛔ Issue BLOQUÉE - **Contradiction avec le code existant**

**Problème:** Cette demande entre en conflit avec la logique actuelle du projet.

**Conflits détectés:**
$($conflicts | ForEach-Object { "- **$($_.Type)** dans ``$($_.File)``: $($_.Description)" } | Out-String)

**Code concerné:**
``````csharp
$conflictingCode
``````

**Raison du blocage:**
$blockReason

**Actions requises:**
1. Revoir la demande pour éviter la contradiction
2. Ou créer une issue préalable pour modifier le code existant
3. Remettre dans **Analyse** une fois résolu

**⚠️ Important:** L'agent ne modifiera JAMAIS du code en contradiction avec la logique existante sans clarification explicite.

---
*🤖 Agent: analysis-bot | ⏱️ $(Get-Date -Format "yyyy-MM-dd HH:mm") | Raison: CONTRADICTION*
"@
```

### Issue bloquée - Redondance
```powershell
$blockedRedundancyComment = @"
## 🤖 Analyse automatique terminée

### 🔄 Issue bloquée - **Redondance détectée**

**Problème:** Cette fonctionnalité semble déjà exister dans le codebase.

**Éléments similaires trouvés:**
| Type | Élément | Fichier | Similarité |
|------|---------|---------|------------|
$($similarElements | ForEach-Object { "| $($_.Type) | $($_.Name) | ``$($_.File)`` | $($_.Similarity)% |" } | Out-String)

**Détails de la redondance:**
$redundancyDetails

**Actions requises:**
1. Vérifier si c'est une **amélioration** de l'existant → Préciser les différences
2. Si c'est un **doublon** → Fermer l'issue
3. Si c'est une **extension** → Reformuler pour clarifier le scope
4. Remettre dans **Analyse** une fois clarifiée

---
*🤖 Agent: analysis-bot | ⏱️ $(Get-Date -Format "yyyy-MM-dd HH:mm") | Raison: REDUNDANCY*
"@
```

### Issue bloquée - Package non autorisé
```powershell
$blockedPackageComment = @"
## 🤖 Analyse automatique terminée

### 📦 Issue bloquée - **Modification de packages non autorisée**

**Problème:** Cette demande nécessite d'ajouter/modifier/retirer des packages NuGet.

**Packages concernés:**
$($packagesAffected | ForEach-Object { "- ``$_``" } | Out-String)

**Règle de projet:**
> Les packages ne peuvent être modifiés que sur demande **explicite**, sauf:
> - ``IDR.Library.BuildingBlocks`` (toujours à jour)
> - ``IDR.Library.Blazor`` (toujours à jour)

**Actions requises:**
1. ✅ Confirmer explicitement la modification des packages dans l'issue
2. ✅ Ou reformuler la demande sans ajout de packages
3. Remettre dans **Analyse**

---
*🤖 Agent: analysis-bot | ⏱️ $(Get-Date -Format "yyyy-MM-dd HH:mm") | Raison: UNAUTHORIZED_PACKAGE*
"@
```

### Issue validée - BackendAdmin API
```powershell
$validBackendAdminComment = @"
## 🤖 Analyse automatique terminée

### ✅ Issue validée - Scope: **BackendAdmin API** (Clean Vertical Slice)

---

#### 📊 Analyse du codebase

**Code existant analysé:**
$($filesAnalyzed | ForEach-Object { "- ``$_``" } | Out-String)

**Compréhension confirmée:** ✅

---

#### 🎯 Feature: **$featureName**

**Éléments à créer:**
| Type | Nom | Chemin |
|------|-----|--------|
$($elementsToCreate | ForEach-Object { "| $($_.Type) | ``$($_.Name)`` | ``$($_.Path)`` |" } | Out-String)

---

#### 🧪 Scénarios Gherkin générés: $scenarioCount

``````gherkin
$gherkinScenarios
``````

---

#### 📁 Fichiers à créer/modifier

$($filesToModify | ForEach-Object { "- [ ] ``$_``" } | Out-String)

---

#### 📋 Checklist de développement

- [ ] Comprendre le code existant
- [ ] Créer la structure de feature
- [ ] Implémenter Command/Query + Handler
- [ ] Ajouter Validator (FluentValidation)
- [ ] Créer l'Endpoint (Carter)
- [ ] Écrire les tests Gherkin
- [ ] Vérifier la compilation
- [ ] Exécuter les tests

---
*🤖 Agent: backendadmin-analyzer | ⏱️ $(Get-Date -Format "yyyy-MM-dd HH:mm")*
"@
```

### Issue validée - FrontendAdmin Blazor
```powershell
$validFrontendAdminComment = @"
## 🤖 Analyse automatique terminée

### ✅ Issue validée - Scope: **FrontendAdmin Blazor Hybrid**

---

#### 📊 Analyse des composants

**Composants existants analysés:**
$($componentsAnalyzed | ForEach-Object { "- ``$_``" } | Out-String)

**Compréhension confirmée:** ✅

---

#### 🎨 Éléments à créer

| Type | Nom | Chemin |
|------|-----|--------|
$($uiElementsToCreate | ForEach-Object { "| $($_.Type) | ``$($_.Name)`` | ``$($_.Path)`` |" } | Out-String)

---

#### 🔗 Utilisation IDR.Library.Blazor

**Composants IDR recommandés:**
$($idrComponents | ForEach-Object { "- ``$_``" } | Out-String)

---

#### 🧪 Scénarios Gherkin générés: $scenarioCount

``````gherkin
$gherkinScenarios
``````

---

#### 🔌 Intégration API (BackendAdmin)

**Endpoints à consommer:**
$($apiEndpoints | ForEach-Object { "- ``$($_.Method) $($_.Route)``" } | Out-String)

---
*🤖 Agent: frontendadmin-analyzer | ⏱️ $(Get-Date -Format "yyyy-MM-dd HH:mm")*
"@
```

### Issue validée - Microservice existant
```powershell
$validMicroserviceComment = @"
## 🤖 Analyse automatique terminée

### ✅ Issue validée - Scope: **Microservice $serviceName**

---

#### 📊 Analyse du service

**Service:** ``$serviceName``
**Architecture:** Clean Vertical Slice ✅

**Code existant analysé:**
$($serviceFilesAnalyzed | ForEach-Object { "- ``$_``" } | Out-String)

---

#### 🎯 Feature: **$featureName**

**Éléments à créer:**
| Type | Nom | Chemin |
|------|-----|--------|
$($elementsToCreate | ForEach-Object { "| $($_.Type) | ``$($_.Name)`` | ``Services/$serviceName/$($_.Path)`` |" } | Out-String)

---

#### 📚 Documentation API

> **Important:** La documentation Swagger sera automatiquement mise à jour.

- **Swagger UI:** ``/docs``
- **OpenAPI JSON:** ``/swagger/v1/swagger.json``
- **README.md:** Mis à jour automatiquement

---

#### 🧪 Scénarios Gherkin générés: $scenarioCount

---
*🤖 Agent: microservice-analyzer | ⏱️ $(Get-Date -Format "yyyy-MM-dd HH:mm")*
"@
```

### Issue validée - Nouveau Microservice
```powershell
$validNewMicroserviceComment = @"
## 🤖 Analyse automatique terminée

### ✅ Issue validée - **Création d'un nouveau microservice**

---

#### 🆕 Nouveau Service: **$newServiceName**

**Description:** $serviceDescription

---

#### 📁 Structure à générer

``````
Services/
└── $newServiceName/
    ├── $newServiceName.Api/
    │   ├── Endpoints/$mainFeature/
    │   ├── Program.cs
    │   ├── Dockerfile
    │   └── readme.md          ← Documentation auto-générée
    │
    ├── $newServiceName.Application/
    │   ├── Features/$mainFeature/
    │   │   ├── Commands/
    │   │   ├── Queries/
    │   │   └── DTOs/
    │   └── DependencyInjection.cs
    │
    ├── $newServiceName.Domain/
    │   ├── Entities/
    │   └── Abstractions/
    │
    └── $newServiceName.Infrastructure/
        ├── Data/
        └── DependencyInjection.cs
``````

---

#### 🎯 Feature principale: **$mainFeature**
#### 📦 Entité principale: **$mainEntity**

---

#### 📚 Documentation API

| Endpoint | Description |
|----------|-------------|
| ``/docs`` | Interface Swagger UI |
| ``/swagger/v1/swagger.json`` | Spécification OpenAPI |

---

#### 🐳 Docker

- ✅ Dockerfile créé
- ✅ docker-compose.yml mis à jour

---

#### 📋 Prochaines étapes

1. [ ] Configurer la chaîne de connexion dans ``appsettings.json``
2. [ ] Créer les migrations EF Core
3. [ ] Compléter les entités Domain
4. [ ] Implémenter les Commands/Queries
5. [ ] Tester avec le fichier ``.http``

---
*🤖 Agent: microservice-creator | ⏱️ $(Get-Date -Format "yyyy-MM-dd HH:mm")*
"@
```

### Issue validée - Full Stack
```powershell
$validFullStackComment = @"
## 🤖 Analyse automatique terminée

### ✅ Issue validée - Scope: **Full Stack** (API + Blazor + Microservices)

---

#### 🔧 Backend - BackendAdmin API

$backendAnalysis

**Commands/Queries à créer:**
$($commandsQueries | ForEach-Object { "- ``$_``" } | Out-String)

---

#### 🎨 Frontend - FrontendAdmin Blazor Hybrid

$frontendAnalysis

**Pages/Composants à créer:**
$($components | ForEach-Object { "- ``$_``" } | Out-String)

---

#### 🔌 Microservices impliqués

$($microservicesInvolved | ForEach-Object { "- **$($_.Name)**: $($_.Action)" } | Out-String)

---

#### 🧪 Tests générés

| Projet | Feature File | Scénarios |
|--------|--------------|-----------|
| BackendAdmin.Tests | ``$apiFeatureFile`` | $apiScenarioCount |
| FrontendAdmin.Tests | ``$blazorFeatureFile`` | $blazorScenarioCount |

---

#### 📋 Ordre d'implémentation suggéré

1. **Domain** - Entités si nécessaire
2. **Application** - Commands/Queries + Handlers
3. **Infrastructure** - Repositories si nécessaire
4. **API** - Endpoints (BackendAdmin)
5. **Microservices** - Modifications si nécessaires
6. **Shared** - Services Blazor + Models
7. **UI** - Pages/Composants (FrontendAdmin)
8. **Tests** - Scénarios Gherkin

---
*🤖 Agent: orchestrator | ⏱️ $(Get-Date -Format "yyyy-MM-dd HH:mm")*
"@
```

## Format de réponse JSON
```json
{
  "action": "approve|block",
  "issue_number": 42,
  "scope": "backendadmin|frontendadmin|microservice|new-microservice|fullstack",
  "service_name": "MagasinService",
  "block_reason": null,
  "target_column": "Todo|AnalyseBlock",
  "labels_added": ["analyzed", "api", "ready-for-dev"],
  "labels_removed": ["needs-analysis"],
  "comment_template": "valid_backendadmin|valid_frontendadmin|valid_microservice|valid_new_microservice|valid_fullstack|blocked_clarification|blocked_contradiction|blocked_redundancy|blocked_package",
  "comment_added": true,
  "body_updated": true,
  "gherkin_added": true,
  "documentation_update_required": false,
  "timestamp": "2024-01-15T14:30:00Z"
}
```
