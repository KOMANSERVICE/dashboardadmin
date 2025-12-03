## Configuration de la migration

# Installation de ef
dotnet tool install --global dotnet-ef

# commande de migration
dotnet ef migrations add InitialCreate --project Services/MenuService/MenuService.Infrastructure --startup-project Services/MenuService/MenuService.Api --output-dir Data/Migrations




# Architecture du Projet MenuService

    MenuService/
    ├── MenuService.Api/           → REST Controllers
    │   ├── Controllers/
    │   │   └── MenuController.cs
    │   ├── Program.cs
    │   ├── appsettings.json
    │   └── ...
    │
    ├── MenuService.Grpc/          → Implémentation gRPC
    │   ├── Protos/
    │   │   └── menu.proto
    │   ├── Services/
    │   │   └── MenuGrpcService.cs
    │   └── Program.cs
    │
    ├── MenuService.Application/          → Application logic
    │   ├── Repositories/
    │   │   └── IMenuRepository.cs
    │   ├── Services/
    │   │   └── MenuService.cs
    │   └── DTOs/
    │
    ├── MenuService.Domain/          → Application logic
    │   ├── Entities/
    │       └── Menu.cs
    │
    │
    └── MenuService.Infrastructure/ → EF Core, Persistence
        ├── Data/
        │   └── MenuContext.cs
        ├── Repositories/
        └── ...

# 🤖 Système d'Agents Autonomes - DashBoardAdmin

Système d'agents IA pour l'analyse, le développement et la maintenance de la solution DashBoardAdmin.

## 📋 Table des matières

- [Vue d'ensemble](#vue-densemble)
- [Architecture de la solution](#architecture-de-la-solution)
- [Les Agents](#les-agents)
- [Installation](#installation)
- [Configuration](#configuration)
- [Utilisation](#utilisation)
- [Règles critiques](#règles-critiques)
- [Librairies internes](#librairies-internes)

## 🎯 Vue d'ensemble

Ce système d'agents automatise:
- ✅ L'analyse des issues GitHub
- ✅ La validation de la cohérence avec le code existant
- ✅ La génération de tests Gherkin (BDD)
- ✅ L'implémentation du code
- ✅ La création de nouveaux microservices
- ✅ La documentation API automatique

## 🏗️ Architecture de la solution

```
DashBoardAdmin/
├── BackendAdmin/                    # API Administration
│   ├── BackendAdmin.Api/            # Minimal APIs (Carter)
│   ├── BackendAdmin.Application/    # CQRS (Commands/Queries)
│   ├── BackendAdmin.Domain/         # Entités
│   └── BackendAdmin.Infrastructure/ # Data Access
│
├── FrontendAdmin/                   # UI Administration
│   ├── FrontendAdmin/               # MAUI App
│   ├── FrontendAdmin.Shared/        # Composants Blazor partagés
│   ├── FrontendAdmin.Web/           # Blazor Server
│   └── FrontendAdmin.Web.Client/    # Blazor WASM
│
├── Services/                        # Microservices
│   ├── MagasinService/              # Gestion des magasins
│   ├── MenuService/                 # Gestion des menus
│   └── {NouveauService}/            # Futurs services...
│
└── docker-compose/                  # Orchestration Docker
```

### Patterns utilisés

| Projet | Architecture | Pattern |
|--------|--------------|---------|
| BackendAdmin | Clean Vertical Slice | CQRS + Minimal APIs |
| FrontendAdmin | Blazor Hybrid | MAUI + Shared Components |
| Microservices | Clean Vertical Slice | CQRS + Minimal APIs |

## 🤖 Les Agents

### 1. Orchestrator (`orchestrator.md`)
Agent principal qui coordonne tous les autres.

**Responsabilités:**
- Classifier les issues
- Dispatcher vers les bons agents
- Consolider les résultats

### 2. BackendAdmin Analyzer (`backendadmin-analyzer.md`)
Analyse le code API avec architecture Clean Vertical Slice.

**Commandes PowerShell:**
```powershell
# Lister les endpoints
Get-ChildItem -Path "BackendAdmin\BackendAdmin.Api\Endpoints" -Filter "*.cs" -Recurse

# Lister les commands CQRS
Get-ChildItem -Path "BackendAdmin\BackendAdmin.Application\Features\**\Commands" -Filter "*Command.cs" -Recurse
```

### 3. FrontendAdmin Analyzer (`frontendadmin-analyzer.md`)
Analyse le code Blazor Hybrid.

**Commandes PowerShell:**
```powershell
# Lister les pages
Get-ChildItem -Path "FrontendAdmin\FrontendAdmin.Shared\Pages" -Filter "*.razor" -Recurse

# Lister les composants
Get-ChildItem -Path "FrontendAdmin\FrontendAdmin.Shared\Components" -Filter "*.razor" -Recurse
```

### 4. Microservice Analyzer (`microservice-analyzer.md`)
Analyse les microservices existants.

### 5. Microservice Creator (`microservice-creator.md`)
Crée de nouveaux microservices avec la structure complète.

**Usage:**
```powershell
New-Microservice -ServiceName "AbonnementService" -MainFeature "Abonnements" -MainEntity "Abonnement"
```

### 6. Doc Generator (`doc-generator.md`)
Génère la documentation API (Swagger/OpenAPI).

### 7. Gherkin Generator (`gherkin-generator.md`)
Génère les scénarios de test BDD.

### 8. GitHub Manager (`github-manager.md`)
Gère les interactions GitHub (issues, PR, Project Board).

### 9. Coder (`coder.md`)
Implémente le code selon les analyses.

### 10. Analysis Bot (`analysis-bot.md`)
Valide ou bloque les issues.

### 11. Migration Manager (`migration-manager.md`) 🆕
Gère les migrations EF Core avec sécurité production.

**Fonctionnalités:**
- Détection automatique des changements d'entités
- Génération sécurisée des migrations
- Analyse des opérations dangereuses
- Correction automatique des problèmes courants
- Blocage si risque de perte de données

## 📦 Installation

### Prérequis

```powershell
# Vérifier les prérequis
gh --version      # GitHub CLI
claude --version  # Claude CLI
dotnet --version  # .NET SDK 10+
git --version     # Git
```

### Installation

```powershell
# 1. Cloner le repository
git clone https://github.com/VOTRE-ORG/DashBoardAdmin.git
cd DashBoardAdmin

# 2. Copier les agents
Copy-Item -Path "dashboardadmin-agents\.claude" -Destination ".\" -Recurse
Copy-Item -Path "dashboardadmin-agents\scripts" -Destination ".\" -Recurse
Copy-Item -Path "dashboardadmin-agents\config" -Destination ".\" -Recurse

# 3. Authentifier GitHub
gh auth login

# 4. Configurer
notepad config\agents-config.ps1
```

## ⚙️ Configuration

### Fichier de configuration (`config/agents-config.ps1`)

```powershell
$Config = @{
    GitHub = @{
        Owner = "VOTRE-ORG"
        Repo = "DashBoardAdmin"
        ProjectNumber = 1
    }
    
    Claude = @{
        # Modèles disponibles
        Model = "claude-sonnet-4-20250514"  # Par défaut
        # Model = "claude-opus-4-20250514"  # Pour tâches complexes
    }
}
```

### Choix du modèle

| Modèle | Usage recommandé |
|--------|------------------|
| `claude-sonnet-4-20250514` | Analyse et codage standard |
| `claude-opus-4-20250514` | Création de nouveaux microservices |

## 🚀 Utilisation

### Démarrer l'agent complet

```powershell
.\scripts\Start-DashBoardAdminAgent.ps1 `
    -Owner "VOTRE-ORG" `
    -Repo "DashBoardAdmin" `
    -ProjectNumber 1 `
    -Model "claude-sonnet-4-20250514"
```

### Mode analyse uniquement

```powershell
.\scripts\Start-DashBoardAdminAgent.ps1 -AnalysisOnly
```

### Mode codeur uniquement

```powershell
.\scripts\Start-DashBoardAdminAgent.ps1 -CoderOnly
```

### Mode simulation (Dry Run)

```powershell
.\scripts\Start-DashBoardAdminAgent.ps1 -DryRun -Verbose
```

### Paramètres disponibles

| Paramètre | Description | Défaut |
|-----------|-------------|--------|
| `-Owner` | Organisation GitHub | - |
| `-Repo` | Nom du repository | DashBoardAdmin |
| `-ProjectNumber` | Numéro du Project Board | 1 |
| `-Model` | Modèle Claude à utiliser | claude-sonnet-4-20250514 |
| `-PollingInterval` | Intervalle en secondes | 60 |
| `-AnalysisOnly` | Mode analyse uniquement | false |
| `-CoderOnly` | Mode codeur uniquement | false |
| `-DryRun` | Simulation sans modification | false |
| `-Verbose` | Afficher les détails | false |

## ⚠️ Règles critiques

### 1. Comprendre avant de modifier
L'agent **DOIT** lire et analyser le code existant avant toute modification.

### 2. Ne jamais contredire
Si une modification contredit la logique existante → **BLOQUER**.

### 3. Ne jamais inventer
Si une information manque → **DEMANDER** clarification, ne pas supposer.

### 4. Respecter les packages
- **NE PAS** ajouter/retirer/modifier de packages sauf demande explicite
- **EXCEPTION**: `IDR.Library.BuildingBlocks` et `IDR.Library.Blazor` doivent toujours être à jour

### 5. Documenter les microservices
- Swagger/OpenAPI configuré sur `/docs`
- README.md à jour

## 📚 Librairies internes

### Lecture automatique de la documentation

La documentation des librairies IDR est lue **automatiquement** depuis les packages NuGet installés au démarrage de l'agent:

```powershell
# Chemin de la documentation (détecté automatiquement selon version installée)
$buildingBlocksDocs = dir "$env:USERPROFILE\.nuget\packages\idr.library.buildingblocks\*\contentFiles\any\any\agent-docs\*"
$blazorDocs = dir "$env:USERPROFILE\.nuget\packages\idr.library.blazor\*\contentFiles\any\any\agent-docs\*"
```

### IDR.Library.BuildingBlocks

Contient:
- CQRS (ICommand, IQuery, ICommandHandler, IQueryHandler)
- Authentification JWT et refresh token
- Sécurité et encryption
- Vault de secrets (VaultSharp)
- FluentValidation (AbstractValidator<T>)
- Mapster (Adapt, AdaptTo)

**Documentation:** Lue automatiquement depuis `agent-docs/` dans le package NuGet

### IDR.Library.Blazor

Contient les composants Blazor partagés:
- Composants UI (préfixe `Idr*`)
- Formulaires (IdrForm, IdrInput, IdrSelect)
- Layout (IdrLayout, IdrNavMenu, IdrHeader)

**Documentation:** Lue automatiquement depuis `agent-docs/` dans le package NuGet

### Fonctions utilitaires

```powershell
# Afficher la documentation disponible
Show-IDRDocumentation

# Lire la documentation d'un package spécifique
Read-IDRLibraryDocs -PackageName "IDR.Library.BuildingBlocks"

# Obtenir la version installée
Get-IDRLibraryVersion -PackageName "IDR.Library.Blazor"

# Lire toute la documentation
$docs = Read-AllIDRLibraryDocs
$docs.BuildingBlocks.Content  # Contenu de la doc
$docs.Blazor.Version          # Version installée
```

### Mise à jour

Les librairies IDR sont **toujours** mises à jour automatiquement.

```powershell
# Vérifier les versions
dotnet list package --outdated | Select-String "IDR.Library"

# Mettre à jour
dotnet add package IDR.Library.BuildingBlocks --source https://nuget.votre-org.com/v3/index.json
dotnet add package IDR.Library.Blazor --source https://nuget.votre-org.com/v3/index.json
```

## 📊 Workflow du Project Board

```
┌─────────┐    ┌──────┐    ┌─────────────┐    ┌───────────┐    ┌──────────┐    ┌──────┐
│ Analyse │───►│ Todo │───►│ In Progress │───►│ In Review │───►│ A Tester │───►│ Done │
└─────────┘    └──────┘    └─────────────┘    └───────────┘    └──────────┘    └──────┘
     │              │                                                               ▲
     │              │                                                               │
     ▼              │                                                               │
┌─────────────┐     └───────────────────────────────────────────────────────────────┘
│AnalyseBlock │     (flux normal)
└─────────────┘
```

## 🧪 Tests

### Framework de test

- **xUnit** - Framework principal
- **Xunit.Gherkin.Quick** - BDD/Gherkin
- **FluentAssertions** - Assertions
- **Moq** - Mocking
- **bUnit** - Tests Blazor

### Structure des tests

```
tests/
├── BackendAdmin.Tests/
├── FrontendAdmin.Tests/
├── MagasinService.Tests/
└── MenuService.Tests/
```

## 🗄️ Migrations EF Core

### Workflow automatique

Lors de toute modification d'entités (Domain/Entities), l'agent:

1. **Détecte** les fichiers d'entités modifiés
2. **Génère** la migration automatiquement
3. **Analyse** les opérations pour sécurité production
4. **Corrige** automatiquement les problèmes corrigeables
5. **Bloque** si opérations dangereuses non corrigeables

### Opérations et actions

| Opération | Risque | Action automatique |
|-----------|--------|-------------------|
| `DropTable` | ⛔ Perte de données | BLOQUER + Commentaire d'avertissement |
| `DropColumn` | ⛔ Perte de données | BLOQUER + Commentaire d'avertissement |
| `AddColumn NOT NULL` (sans default) | ❌ Échec si table non vide | 🔧 Ajouter `defaultValue` |
| `AlterColumn` (type) | ⚠️ Risque de perte | WARNING dans les logs |
| `RenameTable/Column` | ⚠️ Breaking change | WARNING dans les logs |
| `CreateTable` | ✅ Sûr | Aucune action |
| `AddColumn` (nullable) | ✅ Sûr | Aucune action |
| `CreateIndex` | ✅ Sûr | Aucune action |

### Commandes manuelles

```powershell
# Générer une migration pour BackendAdmin
dotnet ef migrations add NomMigration `
    --project "BackendAdmin\BackendAdmin.Infrastructure" `
    --startup-project "BackendAdmin\BackendAdmin.Api" `
    --context ApplicationDbContext

# Générer une migration pour un microservice
dotnet ef migrations add NomMigration `
    --project "Services\MagasinService\MagasinService.Infrastructure" `
    --startup-project "Services\MagasinService\MagasinService.Api" `
    --context MagasinServiceDbContext

# Générer le script SQL pour review
dotnet ef migrations script `
    --project "BackendAdmin\BackendAdmin.Infrastructure" `
    --startup-project "BackendAdmin\BackendAdmin.Api" `
    --output "migrations\script.sql" `
    --idempotent

# Appliquer la migration
dotnet ef database update `
    --project "BackendAdmin\BackendAdmin.Infrastructure" `
    --startup-project "BackendAdmin\BackendAdmin.Api"
```

### Corrections automatiques

L'agent corrige automatiquement ces problèmes:

**AddColumn NOT NULL sans defaultValue:**
```csharp
// ❌ AVANT (échouera si table non vide)
migrationBuilder.AddColumn<string>(
    name: "NouvelleColonne",
    table: "MaTable",
    nullable: false);

// ✅ APRÈS (corrigé automatiquement)
migrationBuilder.AddColumn<string>(
    name: "NouvelleColonne",
    table: "MaTable",
    nullable: false,
    defaultValue: "");  // ← Ajouté automatiquement
```

**Valeurs par défaut par type:**

| Type C# | defaultValue ajouté |
|---------|---------------------|
| `string` | `""` |
| `int`, `long` | `0` |
| `bool` | `false` |
| `Guid` | `Guid.Empty` |
| `DateTime` | `GETUTCDATE()` (SQL) |
| `decimal` | `0m` |

### Exemple de blocage

Si une migration contient une opération dangereuse:

```csharp
// Ce code sera marqué comme BLOQUÉ:
migrationBuilder.DropColumn(
    name: "AncienneColonne",
    table: "MaTable");
```

L'agent:
1. Ajoute un commentaire d'avertissement dans le fichier
2. Bloque l'issue avec un message explicatif
3. Demande une confirmation manuelle

## 📝 Création d'un nouveau microservice

```powershell
# L'agent créera automatiquement:
# - Structure complète du service
# - Dockerfile
# - Documentation Swagger
# - README.md
# - Configuration docker-compose

# Trigger: Issue avec scope "Nouveau microservice: AbonnementService"
```

## 🔧 Dépannage

### L'agent ne démarre pas

```powershell
# Vérifier les prérequis
gh auth status
claude --version
```

### Issues non traitées

```powershell
# Vérifier le fichier de suivi
Get-Content ".claude\processed-analysis.json"

# Réinitialiser si nécessaire
"[]" | Out-File ".claude\processed-analysis.json" -Encoding utf8
```

### Erreurs de déplacement d'issue

```powershell
# Vérifier les colonnes du Project
gh project field-list $ProjectNumber --owner $Owner --format json
```

## 📄 License

Propriétaire - Usage interne uniquement.
