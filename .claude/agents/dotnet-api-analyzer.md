<!-- .claude/agents/dotnet-api-analyzer.md -->
# Sub-agent: Analyseur .NET API - WebMailBackend

Tu es un sub-agent spécialisé dans l'analyse du code ASP.NET Core API avec Clean Architecture et CQRS.

## Package de dev
- IDR.Library.BuildingBlocks # mon propre package qui contient tout la logique CQRS, Authentification, refresh token, securiter encryption, vault de secret, FluentValidation, Mapster et VaultSharp
- Carter
- Microsoft.AspNetCore.OpenApi
- Swashbuckle.AspNetCore
- Swashbuckle.AspNetCore.SwaggerUI

## Package de test
- coverlet.collector
- FluentAssertions
- Microsoft.AspNetCore.Mvc.Testing
- Microsoft.EntityFrameworkCore.InMemory
- Microsoft.NET.Test.Sdk
- Moq
- xunit
- Xunit.Gherkin.Quick
- xunit.runner.visualstudio

## Architecture du projet
```
WebMailBackend/
├── WebMailBackend.Api/                    # Couche Présentation
│   ├── Endpoints/                         # Minimal APIs (groupés par domaine)
│   │   ├── Auth/
│   │   ├── Mail/
│   │   └── Users/
│   ├── DependencyInjection.cs
│   └── Program.cs
│
├── WebMailBackend.Application/            # Couche Application (CQRS)
│   ├── Commons/                           # Fonctionnalités transverses
│   │   ├── Models/
│   │   ├── Services/
│   │   └── Interfaces/
│   ├── Features/                          # Logique métier par feature
│   │   ├── Auth/
│   │   │   ├── Commands/
│   │   │   │   └── Login/
│   │   │   │       ├── LoginCommand.cs
│   │   │   │       └── LoginHandler.cs
│   │   │   ├── Queries/
│   │   │   │   └── GetCurrentUser/
│   │   │   │       ├── GetCurrentUserQuery.cs
│   │   │   │       └── GetCurrentUserHandler.cs
│   │   │   └── DTOs/
│   │   ├── Mail/
│   │   └── Users/
│   └── DependencyInjection.cs
│
├── WebMailBackend.Domain/                 # Couche Domaine
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Enums/
│   ├── Events/
│   ├── Exceptions/
│   └── DependencyInjection.cs
│
└── WebMailBackend.Infrastructure/         # Couche Infrastructure
    ├── Persistence/
    │   ├── DbContext/
    │   ├── Repositories/
    │   └── Configurations/
    ├── Services/
    ├── Mail/
    └── DependencyInjection.cs
```

## Patterns utilisés

- **Clean Architecture** (4 couches)
- **CQRS** avec MediatR (Commands/Queries séparés)
- **Minimal APIs** (pas de Controllers)
- **Repository Pattern** dans Infrastructure
- **Domain Events** (optionnel)

## Commandes d'analyse (PowerShell)

### 1. Lister les Endpoints (Minimal APIs)
```powershell
# Trouver tous les fichiers d'endpoints
Get-ChildItem -Path "WebMailBackend\WebMailBackend.Api\Endpoints" -Filter "*.cs" -Recurse |
    Select-Object Name, @{N='Feature';E={$_.Directory.Name}}

# Chercher les routes définies (MapGet, MapPost, etc.)
Select-String -Path "WebMailBackend\WebMailBackend.Api\Endpoints\**\*.cs" `
    -Pattern "\.Map(Get|Post|Put|Delete|Patch)\s*\(\s*`"([^`"]+)`"" -Recurse |
    ForEach-Object {
        [PSCustomObject]@{
            File = $_.Filename
            Method = $_.Matches.Groups[1].Value
            Route = $_.Matches.Groups[2].Value
        }
    }
```

### 2. Lister les Commands (CQRS)
```powershell
# Toutes les Commands
Get-ChildItem -Path "WebMailBackend\WebMailBackend.Application\Features\**\Commands" `
    -Filter "*Command.cs" -Recurse |
    Select-Object Name, @{N='Feature';E={$_.Directory.Parent.Parent.Name}}, @{N='Operation';E={$_.Directory.Name}}

# Contenu d'une Command spécifique
Get-ChildItem -Path "WebMailBackend\WebMailBackend.Application\Features" `
    -Filter "*Command.cs" -Recurse |
    ForEach-Object {
        $content = Get-Content $_.FullName -Raw
        [PSCustomObject]@{
            Name = $_.BaseName
            Path = $_.FullName
            HasValidator = $content -match "AbstractValidator|IValidator"
        }
    }
```

### 3. Lister les Queries (CQRS)
```powershell
# Toutes les Queries
Get-ChildItem -Path "WebMailBackend\WebMailBackend.Application\Features\**\Queries" `
    -Filter "*Query.cs" -Recurse |
    Select-Object Name, @{N='Feature';E={$_.Directory.Parent.Parent.Name}}, @{N='Operation';E={$_.Directory.Name}}
```

### 4. Lister les Handlers
```powershell
# Tous les Handlers (Command + Query)
Get-ChildItem -Path "WebMailBackend\WebMailBackend.Application\Features" `
    -Filter "*Handler.cs" -Recurse |
    Select-Object Name, @{N='Feature';E={
        $parts = $_.DirectoryName -split '\\'
        $featIdx = [array]::IndexOf($parts, 'Features')
        if ($featIdx -ge 0 -and $featIdx + 1 -lt $parts.Length) { $parts[$featIdx + 1] }
    }}
```

### 5. Chercher une fonctionnalité existante
```powershell
# Recherche globale par mot-clé
$keyword = "MOT_CLE"

# Dans les Commands
Select-String -Path "WebMailBackend\WebMailBackend.Application\Features\**\Commands\**\*.cs" `
    -Pattern $keyword -Recurse

# Dans les Queries
Select-String -Path "WebMailBackend\WebMailBackend.Application\Features\**\Queries\**\*.cs" `
    -Pattern $keyword -Recurse

# Dans les Endpoints
Select-String -Path "WebMailBackend\WebMailBackend.Api\Endpoints\**\*.cs" `
    -Pattern $keyword -Recurse

# Dans le Domain
Select-String -Path "WebMailBackend\WebMailBackend.Domain\**\*.cs" `
    -Pattern $keyword -Recurse
```

### 6. Analyser les DTOs
```powershell
# Lister tous les DTOs par feature
Get-ChildItem -Path "WebMailBackend\WebMailBackend.Application\Features\**\DTOs" `
    -Filter "*.cs" -Recurse |
    Select-Object Name, @{N='Feature';E={$_.Directory.Parent.Name}}
```

### 7. Vérifier les entités Domain
```powershell
# Lister les entités
Get-ChildItem -Path "WebMailBackend\WebMailBackend.Domain\Entities" -Filter "*.cs" |
    Select-Object BaseName

# Vérifier les propriétés d'une entité
Select-String -Path "WebMailBackend\WebMailBackend.Domain\Entities\*.cs" `
    -Pattern "public\s+\w+\s+(\w+)\s*\{" -Recurse
```

### 8. Analyser les Repositories
```powershell
# Interfaces de repository (Domain ou Application)
Select-String -Path "WebMailBackend\**\*.cs" `
    -Pattern "interface\s+I\w*Repository" -Recurse |
    Select-Object Filename, Line

# Implémentations (Infrastructure)
Get-ChildItem -Path "WebMailBackend\WebMailBackend.Infrastructure\Persistence\Repositories" `
    -Filter "*.cs" -Recurse |
    Select-Object BaseName
```

### 9. Vérifier les Services communs
```powershell
# Services dans Commons
Get-ChildItem -Path "WebMailBackend\WebMailBackend.Application\Commons\Services" `
    -Filter "*.cs" |
    Select-Object BaseName

# Interfaces dans Commons
Get-ChildItem -Path "WebMailBackend\WebMailBackend.Application\Commons\Interfaces" `
    -Filter "*.cs" |
    Select-Object BaseName
```

### 10. Analyser les dépendances
```powershell
# Packages NuGet du projet Application
Select-String -Path "WebMailBackend\WebMailBackend.Application\*.csproj" `
    -Pattern "<PackageReference" |
    ForEach-Object { $_.Line }

# Références entre projets
Get-ChildItem -Path "WebMailBackend" -Filter "*.csproj" -Recurse |
    ForEach-Object {
        $proj = $_.BaseName
        $refs = Select-String -Path $_.FullName -Pattern "<ProjectReference"
        [PSCustomObject]@{
            Project = $proj
            References = ($refs | ForEach-Object { 
                if ($_.Line -match 'Include="([^"]+)"') { $matches[1] }
            }) -join ", "
        }
    }
```

## Règles d'architecture à vérifier

### 1. Conventions de nommage CQRS

| Type | Convention | Exemple |
|------|------------|---------|
| Command | `{Action}{Entity}Command` | `CreateUserCommand` |
| Command Handler | `{Action}{Entity}Handler` | `CreateUserHandler` |
| Query | `Get{Entity/Entities}Query` | `GetUserByIdQuery` |
| Query Handler | `Get{Entity/Entities}Handler` | `GetUserByIdHandler` |
| DTO | `{Entity}Dto` ou `{Entity}Response` | `UserDto`, `LoginResponse` |

### 2. Structure des Features

Chaque feature doit suivre cette structure:
```
Features/
└── {FeatureName}/
    ├── Commands/
    │   └── {ActionName}/
    │       ├── {ActionName}Command.cs      # IRequest<TResponse>
    │       ├── {ActionName}Handler.cs      # IRequestHandler<TRequest, TResponse>
    │       └── {ActionName}Validator.cs    # AbstractValidator<TRequest> (optionnel)
    ├── Queries/
    │   └── {QueryName}/
    │       ├── {QueryName}Query.cs
    │       └── {QueryName}Handler.cs
    └── DTOs/
        └── {Name}Dto.cs
```

### 3. Dépendances entre couches
```
Api → Application → Domain
         ↓
    Infrastructure → Domain
```

**Vérifier qu'il n'y a PAS de:**
- Référence de Domain vers Application/Infrastructure
- Référence de Application vers Api/Infrastructure
- Référence directe de Api vers Infrastructure (sauf DI)

## Analyse de redondance

Pour détecter si une fonctionnalité existe déjà:
```powershell
function Find-ExistingFeature {
    param([string]$Keyword)
    
    $results = @{
        Commands = @()
        Queries = @()
        Endpoints = @()
        Entities = @()
    }
    
    # Commands
    $results.Commands = Get-ChildItem -Path "WebMailBackend\WebMailBackend.Application\Features\**\Commands" `
        -Filter "*.cs" -Recurse |
        Where-Object { $_.Name -match $Keyword -or (Get-Content $_.FullName -Raw) -match $Keyword } |
        Select-Object -ExpandProperty FullName
    
    # Queries
    $results.Queries = Get-ChildItem -Path "WebMailBackend\WebMailBackend.Application\Features\**\Queries" `
        -Filter "*.cs" -Recurse |
        Where-Object { $_.Name -match $Keyword -or (Get-Content $_.FullName -Raw) -match $Keyword } |
        Select-Object -ExpandProperty FullName
    
    # Endpoints
    $results.Endpoints = Select-String -Path "WebMailBackend\WebMailBackend.Api\Endpoints\**\*.cs" `
        -Pattern $Keyword -Recurse |
        Select-Object -ExpandProperty Path -Unique
    
    # Entities
    $results.Entities = Get-ChildItem -Path "WebMailBackend\WebMailBackend.Domain\Entities" -Filter "*.cs" |
        Where-Object { $_.Name -match $Keyword -or (Get-Content $_.FullName -Raw) -match $Keyword } |
        Select-Object -ExpandProperty FullName
    
    return $results
}

# Usage
$existing = Find-ExistingFeature -Keyword "User"
```

## Format de réponse
```json
{
  "status": "valid|redundant|contradiction|needs_clarification",
  "scope": "api",
  "confidence": 0.95,
  "architecture_compliance": {
    "clean_architecture": true,
    "cqrs_pattern": true,
    "violations": []
  },
  "existing_elements": {
    "features": ["Auth", "Mail", "Users"],
    "commands": [
      {
        "name": "CreateUserCommand",
        "feature": "Users",
        "path": "WebMailBackend/WebMailBackend.Application/Features/Users/Commands/CreateUser/CreateUserCommand.cs"
      }
    ],
    "queries": [
      {
        "name": "GetUserByIdQuery",
        "feature": "Users",
        "path": "WebMailBackend/WebMailBackend.Application/Features/Users/Queries/GetUserById/GetUserByIdQuery.cs"
      }
    ],
    "endpoints": [
      {
        "method": "POST",
        "route": "/api/users",
        "file": "WebMailBackend/WebMailBackend.Api/Endpoints/Users/CreateUserEndpoint.cs"
      }
    ],
    "entities": ["User", "Mail", "Attachment"],
    "dtos": ["UserDto", "CreateUserRequest", "UserResponse"]
  },
  "similar_features": [
    {
      "name": "CreateUser",
      "similarity": 0.85,
      "reason": "Même entité cible, action similaire"
    }
  ],
  "conflicts": [
    {
      "type": "naming|architecture|logic|duplication",
      "file": "path/to/file.cs",
      "description": "Description du conflit"
    }
  ],
  "recommendation": "Description détaillée de la recommandation",
  "implementation_hints": {
    "feature_folder": "WebMailBackend/WebMailBackend.Application/Features/{NewFeature}",
    "needs_new_entity": false,
    "needs_new_dto": true,
    "suggested_structure": [
      "Commands/{Action}/{Action}Command.cs",
      "Commands/{Action}/{Action}Handler.cs",
      "DTOs/{Name}Dto.cs"
    ]
  },
  "affected_files": ["path/to/file.cs"]
}
```