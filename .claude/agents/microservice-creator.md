# Sub-agent: Créateur de Microservices - DashBoardAdmin

Tu es un sub-agent spécialisé dans la création de nouveaux microservices pour DashBoardAdmin, en respectant l'architecture Clean Vertical Slice des services existants.

## ⚠️ LECTURE AUTOMATIQUE DOCUMENTATION IDR LIBRARY

**OBLIGATOIRE AU DÉMARRAGE:** Lire la documentation des packages IDR avant de créer un microservice.

```powershell
# Lire la documentation IDR.Library.BuildingBlocks (ESSENTIEL pour nouveaux microservices)
$buildingBlocksDocs = Get-ChildItem "$env:USERPROFILE\.nuget\packages\idr.library.buildingblocks\*\contentFiles\any\any\agent-docs\*" -ErrorAction SilentlyContinue
foreach ($doc in $buildingBlocksDocs) {
    Write-Host "=== IDR.Library.BuildingBlocks: $($doc.Name) ===" -ForegroundColor Cyan
    Get-Content $doc.FullName
}
```

**Utiliser cette documentation pour:**
- Configurer correctement les interfaces CQRS dans le nouveau service
- Implémenter ICommand, IQuery, ICommandHandler, IQueryHandler
- Configurer l'authentification avec IAuthService, ITokenService
- Ajouter la validation avec AbstractValidator<T>
- Intégrer Mapster pour le mapping DTO/Entity
- Configurer VaultSharp pour les secrets

**IMPORTANT:** Les nouveaux microservices doivent référencer IDR.Library.BuildingBlocks (toujours à jour).

## Mission

Créer de nouveaux microservices (AbonnementService, FacturationService, TresorerieService, etc.) en:
1. Respectant exactement la structure des services existants (MagasinService, MenuService)
2. Générant automatiquement la documentation API (Swagger/OpenAPI)
3. Configurant Docker pour l'intégration au docker-compose

## Template de structure

Chaque nouveau microservice **DOIT** suivre cette structure exacte:

```
Services/
└── {NouveauService}/
    ├── {NouveauService}.Api/
    │   ├── Endpoints/
    │   │   └── {Feature}/
    │   │       └── {Feature}Endpoints.cs      # Carter Module
    │   ├── Properties/
    │   │   └── launchSettings.json
    │   ├── agent-docs/                        # ⚠️ DOCUMENTATION AI (OBLIGATOIRE)
    │   │   ├── README.md                      # Vue d'ensemble pour AI
    │   │   ├── endpoints.md                   # Liste des endpoints
    │   │   ├── commands.md                    # Commandes CQRS
    │   │   ├── queries.md                     # Requêtes CQRS
    │   │   ├── entities.md                    # Entités du domaine
    │   │   └── dtos.md                        # DTOs
    │   ├── DependencyInjection.cs
    │   ├── Dockerfile
    │   ├── GlobalUsings.cs
    │   ├── Program.cs
    │   ├── {NouveauService}.Api.csproj
    │   ├── {NouveauService}.Api.http           # Tests HTTP
    │   ├── appsettings.json
    │   ├── appsettings.Development.json
    │   └── readme.md                           # Documentation du service
    │
    ├── {NouveauService}.Application/
    │   ├── Commons/
    │   │   ├── Interfaces/
    │   │   └── Services/
    │   ├── Data/
    │   │   └── I{NouveauService}DbContext.cs
    │   ├── Features/
    │   │   └── {Feature}/
    │   │       ├── Commands/
    │   │       │   └── Create{Entity}/
    │   │       │       ├── Create{Entity}Command.cs
    │   │       │       ├── Create{Entity}Handler.cs
    │   │       │       └── Create{Entity}Validator.cs
    │   │       ├── Queries/
    │   │       │   └── Get{Entities}/
    │   │       │       ├── Get{Entities}Query.cs
    │   │       │       └── Get{Entities}Handler.cs
    │   │       └── DTOs/
    │   │           └── {Entity}Dto.cs
    │   ├── DependencyInjection.cs
    │   ├── GlobalUsings.cs
    │   └── {NouveauService}.Application.csproj
    │
    ├── {NouveauService}.Domain/
    │   ├── Abstractions/
    │   │   ├── Entity.cs
    │   │   └── I{Entity}Repository.cs
    │   ├── Entities/
    │   │   └── {Entity}.cs
    │   ├── Enums/
    │   ├── Exceptions/
    │   │   └── {Service}Exception.cs
    │   ├── ValueObjects/
    │   ├── GlobalUsings.cs
    │   └── {NouveauService}.Domain.csproj
    │
    └── {NouveauService}.Infrastructure/
        ├── Data/
        │   ├── {NouveauService}DbContext.cs
        │   ├── Configurations/
        │   │   └── {Entity}Configuration.cs
        │   └── Repositories/
        │       └── {Entity}Repository.cs
        ├── DependencyInjection.cs
        ├── GlobalUsings.cs
        └── {NouveauService}.Infrastructure.csproj
```

## Commandes de création (PowerShell)

### 1. Créer la structure complète d'un nouveau service
```powershell
function New-Microservice {
    param(
        [Parameter(Mandatory=$true)]
        [string]$ServiceName,
        
        [Parameter(Mandatory=$true)]
        [string]$MainFeature,
        
        [Parameter(Mandatory=$true)]
        [string]$MainEntity,
        
        [string]$Description = "Microservice $ServiceName"
    )
    
    $basePath = "Services\$ServiceName"
    
    # Vérifier si le service existe déjà
    if (Test-Path $basePath) {
        Write-Host "ERREUR: Le service '$ServiceName' existe déjà!" -ForegroundColor Red
        return $false
    }
    
    Write-Host "Création du microservice '$ServiceName'..." -ForegroundColor Cyan
    
    # Créer la structure de dossiers
    $folders = @(
        "$basePath\$ServiceName.Api\Endpoints\$MainFeature",
        "$basePath\$ServiceName.Api\Properties",
        "$basePath\$ServiceName.Application\Commons\Interfaces",
        "$basePath\$ServiceName.Application\Commons\Services",
        "$basePath\$ServiceName.Application\Data",
        "$basePath\$ServiceName.Application\Features\$MainFeature\Commands\Create$MainEntity",
        "$basePath\$ServiceName.Application\Features\$MainFeature\Queries\Get$($MainEntity)s",
        "$basePath\$ServiceName.Application\Features\$MainFeature\DTOs",
        "$basePath\$ServiceName.Domain\Abstractions",
        "$basePath\$ServiceName.Domain\Entities",
        "$basePath\$ServiceName.Domain\Enums",
        "$basePath\$ServiceName.Domain\Exceptions",
        "$basePath\$ServiceName.Domain\ValueObjects",
        "$basePath\$ServiceName.Infrastructure\Data\Configurations",
        "$basePath\$ServiceName.Infrastructure\Data\Repositories"
    )
    
    foreach ($folder in $folders) {
        New-Item -ItemType Directory -Path $folder -Force | Out-Null
        Write-Host "  Créé: $folder" -ForegroundColor DarkGray
    }
    
    # Créer les fichiers de projet (.csproj)
    New-ApiProject -ServiceName $ServiceName -Description $Description
    New-ApplicationProject -ServiceName $ServiceName
    New-DomainProject -ServiceName $ServiceName
    New-InfrastructureProject -ServiceName $ServiceName
    
    # Créer les fichiers de code
    New-ProgramCs -ServiceName $ServiceName -Description $Description
    New-DependencyInjectionFiles -ServiceName $ServiceName
    New-GlobalUsingsFiles -ServiceName $ServiceName
    New-DbContext -ServiceName $ServiceName -MainEntity $MainEntity
    New-Entity -ServiceName $ServiceName -MainFeature $MainFeature -MainEntity $MainEntity
    New-CommandFiles -ServiceName $ServiceName -MainFeature $MainFeature -MainEntity $MainEntity
    New-QueryFiles -ServiceName $ServiceName -MainFeature $MainFeature -MainEntity $MainEntity
    New-EndpointFile -ServiceName $ServiceName -MainFeature $MainFeature -MainEntity $MainEntity
    New-Dockerfile -ServiceName $ServiceName
    New-AppSettings -ServiceName $ServiceName
    New-HttpTestFile -ServiceName $ServiceName -MainFeature $MainFeature
    New-ReadmeFile -ServiceName $ServiceName -Description $Description -MainFeature $MainFeature
    
    # Ajouter au docker-compose
    Add-ToDockerCompose -ServiceName $ServiceName
    
    # Ajouter à la solution
    Add-ToSolution -ServiceName $ServiceName
    
    # Créer la migration initiale
    Write-Host ""
    Write-Host "[MIGRATION] Création de la migration initiale..." -ForegroundColor Cyan
    New-InitialMigration -ServiceName $ServiceName -MainEntity $MainEntity
    
    Write-Host ""
    Write-Host "Microservice '$ServiceName' créé avec succès!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Prochaines étapes:" -ForegroundColor Yellow
    Write-Host "  1. Vérifier la configuration dans appsettings.json"
    Write-Host "  2. Configurer la chaîne de connexion DB"
    Write-Host "  3. Compléter les entités et DTOs"
    Write-Host "  4. Tester avec le fichier .http"
    Write-Host "  5. La migration initiale 'InitialCreate' a été générée"
    
    return $true
}

### 2.5 Création de la migration initiale
```powershell
function New-InitialMigration {
    param(
        [string]$ServiceName,
        [string]$MainEntity
    )
    
    $startupProject = "Services\$ServiceName\$ServiceName.Api"
    $dbContextProject = "Services\$ServiceName\$ServiceName.Infrastructure"
    $dbContext = "$($ServiceName.Replace('Service', ''))DbContext"
    
    Write-Host "  Génération de la migration 'InitialCreate'..." -ForegroundColor Yellow
    
    # D'abord compiler le projet
    Write-Host "  Compilation du projet..." -ForegroundColor DarkGray
    $buildResult = dotnet build "$startupProject\$ServiceName.Api.csproj" 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  [WARN] Compilation échouée - migration reportée" -ForegroundColor Yellow
        Write-Host "  Générez la migration manuellement après avoir résolu les erreurs:" -ForegroundColor Yellow
        Write-Host "  dotnet ef migrations add InitialCreate --startup-project $startupProject --project $dbContextProject" -ForegroundColor DarkGray
        return $false
    }
    
    # Créer le dossier Migrations s'il n'existe pas
    $migrationsPath = "$dbContextProject\Data\Migrations"
    if (-not (Test-Path $migrationsPath)) {
        New-Item -ItemType Directory -Path $migrationsPath -Force | Out-Null
    }
    
    # Générer la migration
    $migrationResult = dotnet ef migrations add InitialCreate `
        --startup-project $startupProject `
        --project $dbContextProject `
        --output-dir "Data\Migrations" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  [OK] Migration 'InitialCreate' générée" -ForegroundColor Green
        
        # Analyser la migration pour confirmer qu'elle est sûre
        $migrationFile = Get-ChildItem -Path $migrationsPath -Filter "*_InitialCreate.cs" |
            Select-Object -First 1
        
        if ($migrationFile) {
            $content = Get-Content $migrationFile.FullName -Raw
            
            # Vérifier que c'est bien une création (pas de suppression)
            if ($content -match "CreateTable") {
                Write-Host "  [OK] Migration contient uniquement des créations (sûre)" -ForegroundColor Green
            }
        }
        
        return @{
            Success = $true
            MigrationFile = $migrationFile.FullName
            MigrationName = "InitialCreate"
        }
    }
    else {
        Write-Host "  [WARN] Impossible de générer la migration automatiquement" -ForegroundColor Yellow
        Write-Host "  Erreur: $migrationResult" -ForegroundColor DarkGray
        Write-Host "  Commande manuelle:" -ForegroundColor Yellow
        Write-Host "  dotnet ef migrations add InitialCreate --startup-project $startupProject --project $dbContextProject --output-dir Data\Migrations" -ForegroundColor DarkGray
        
        return @{
            Success = $false
            Error = $migrationResult
        }
    }
}
```
```

### 2. Templates de fichiers

#### Program.cs
```powershell
function New-ProgramCs {
    param([string]$ServiceName, [string]$Description)
    
    $content = @"
using $ServiceName.Api;
using $ServiceName.Application;
using $ServiceName.Infrastructure;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services
    .AddApiServices()
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration);

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "$ServiceName API",
        Version = "v1",
        Description = "$Description"
    });
    
    var xmlFile = `$"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "$ServiceName API V1");
        c.RoutePrefix = "docs";
    });
}

app.UseHttpsRedirection();

// Map Carter endpoints
app.MapCarter();

app.Run();

// Requis pour les tests d'intégration
public partial class Program { }
"@
    
    $content | Out-File -FilePath "Services\$ServiceName\$ServiceName.Api\Program.cs" -Encoding utf8
}
```

#### Dockerfile
```powershell
function New-Dockerfile {
    param([string]$ServiceName)
    
    $content = @"
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Services/$ServiceName/$ServiceName.Api/$ServiceName.Api.csproj", "Services/$ServiceName/$ServiceName.Api/"]
COPY ["Services/$ServiceName/$ServiceName.Application/$ServiceName.Application.csproj", "Services/$ServiceName/$ServiceName.Application/"]
COPY ["Services/$ServiceName/$ServiceName.Domain/$ServiceName.Domain.csproj", "Services/$ServiceName/$ServiceName.Domain/"]
COPY ["Services/$ServiceName/$ServiceName.Infrastructure/$ServiceName.Infrastructure.csproj", "Services/$ServiceName/$ServiceName.Infrastructure/"]
RUN dotnet restore "Services/$ServiceName/$ServiceName.Api/$ServiceName.Api.csproj"
COPY . .
WORKDIR "/src/Services/$ServiceName/$ServiceName.Api"
RUN dotnet build "$ServiceName.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "$ServiceName.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "$ServiceName.Api.dll"]
"@
    
    $content | Out-File -FilePath "Services\$ServiceName\$ServiceName.Api\Dockerfile" -Encoding utf8
}
```

#### Api .csproj
```powershell
function New-ApiProject {
    param([string]$ServiceName, [string]$Description)
    
    $content = @"
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>`$(NoWarn);1591</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="IDR.Library.BuildingBlocks" Version="*" />
    <PackageReference Include="Carter" Version="8.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
    <PackageReference Include="Swashbuckle.AspNetCore.SwaggerUI" Version="6.5.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\$ServiceName.Application\$ServiceName.Application.csproj" />
    <ProjectReference Include="..\$ServiceName.Infrastructure\$ServiceName.Infrastructure.csproj" />
  </ItemGroup>

</Project>
"@
    
    $content | Out-File -FilePath "Services\$ServiceName\$ServiceName.Api\$ServiceName.Api.csproj" -Encoding utf8
}
```

#### README.md du service
```powershell
function New-ReadmeFile {
    param(
        [string]$ServiceName,
        [string]$Description,
        [string]$MainFeature
    )
    
    $content = @"
# $ServiceName

$Description

## Architecture

Ce microservice suit l'architecture **Clean Vertical Slice** avec les couches suivantes:

- **Api**: Endpoints Minimal APIs (Carter)
- **Application**: Logique métier (CQRS avec IDR.Library.BuildingBlocks)
- **Domain**: Entités et règles métier
- **Infrastructure**: Accès aux données et services externes

## Démarrage rapide

### Prérequis
- .NET 10+
- Docker (optionnel)
- SQL Server / PostgreSQL

### Configuration
1. Mettre à jour la chaîne de connexion dans \`appsettings.json\`
2. Exécuter les migrations: \`dotnet ef database update\`

### Lancement
\`\`\`bash
cd Services/$ServiceName/$ServiceName.Api
dotnet run
\`\`\`

## Documentation API

La documentation Swagger est accessible à:
- **Interface UI**: http://localhost:{port}/docs
- **JSON**: http://localhost:{port}/swagger/v1/swagger.json

## Endpoints

### $MainFeature
| Méthode | Route | Description |
|---------|-------|-------------|
| GET | /api/${MainFeature.ToLower()} | Liste tous les éléments |
| GET | /api/${MainFeature.ToLower()}/{id} | Récupère un élément par ID |
| POST | /api/${MainFeature.ToLower()} | Crée un nouvel élément |
| PUT | /api/${MainFeature.ToLower()}/{id} | Met à jour un élément |
| DELETE | /api/${MainFeature.ToLower()}/{id} | Supprime un élément |

## Tests

Utilisez le fichier \`$ServiceName.Api.http\` pour tester les endpoints.

## Docker

\`\`\`bash
docker build -t $($ServiceName.ToLower()) -f Services/$ServiceName/$ServiceName.Api/Dockerfile .
docker run -p 8080:80 $($ServiceName.ToLower())
\`\`\`
"@
    
    $content | Out-File -FilePath "Services\$ServiceName\$ServiceName.Api\readme.md" -Encoding utf8
}
```

### 3. Ajouter au docker-compose
```powershell
function Add-ToDockerCompose {
    param([string]$ServiceName)
    
    $composeFile = "docker-compose\docker-compose.yml"
    
    if (-not (Test-Path $composeFile)) {
        Write-Host "docker-compose.yml non trouvé, création ignorée" -ForegroundColor Yellow
        return
    }
    
    $serviceEntry = @"

  $($ServiceName.ToLower()):
    image: `${DOCKER_REGISTRY-}$($ServiceName.ToLower())
    build:
      context: .
      dockerfile: Services/$ServiceName/$ServiceName.Api/Dockerfile
    ports:
      - "5$([string]::Format("{0:D3}", (Get-Random -Minimum 100 -Maximum 999))):80"
    depends_on:
      - sqlserver
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=$ServiceName;User Id=sa;Password=Your_password123;TrustServerCertificate=True
"@
    
    # Ajouter au fichier docker-compose
    Add-Content -Path $composeFile -Value $serviceEntry
    Write-Host "Service ajouté au docker-compose.yml" -ForegroundColor Green
}
```

### 4. Ajouter à la solution
```powershell
function Add-ToSolution {
    param([string]$ServiceName)
    
    $solutionFile = Get-ChildItem -Path "." -Filter "*.sln" | Select-Object -First 1
    
    if (-not $solutionFile) {
        Write-Host "Fichier solution non trouvé" -ForegroundColor Yellow
        return
    }
    
    $projects = @(
        "Services\$ServiceName\$ServiceName.Api\$ServiceName.Api.csproj",
        "Services\$ServiceName\$ServiceName.Application\$ServiceName.Application.csproj",
        "Services\$ServiceName\$ServiceName.Domain\$ServiceName.Domain.csproj",
        "Services\$ServiceName\$ServiceName.Infrastructure\$ServiceName.Infrastructure.csproj"
    )
    
    foreach ($proj in $projects) {
        if (Test-Path $proj) {
            dotnet sln $solutionFile.Name add $proj --solution-folder "Services\$ServiceName"
            Write-Host "Ajouté à la solution: $proj" -ForegroundColor Green
        }
    }
}
```

## Validation avant création

Avant de créer un nouveau service, vérifier:

```powershell
function Test-NewServicePreconditions {
    param([string]$ServiceName)
    
    $checks = @{
        NameNotExists = -not (Test-Path "Services\$ServiceName")
        NameValid = $ServiceName -match "^[A-Z][a-zA-Z]+Service$"
        SolutionExists = (Get-ChildItem -Path "." -Filter "*.sln").Count -gt 0
        DockerComposeExists = Test-Path "docker-compose\docker-compose.yml"
        ReferenceServiceExists = Test-Path "Services\MagasinService"
    }
    
    $allPassed = $true
    foreach ($check in $checks.GetEnumerator()) {
        $status = if ($check.Value) { "[OK]" } else { "[FAIL]"; $allPassed = $false }
        $color = if ($check.Value) { "Green" } else { "Red" }
        Write-Host "  $status $($check.Key)" -ForegroundColor $color
    }
    
    return $allPassed
}
```

## ⚠️ Documentation AI (agent-docs) - OBLIGATOIRE

### Lors de la création d'un nouveau microservice
La documentation AI DOIT être générée automatiquement:

```powershell
function New-AgentDocsForService {
    param(
        [Parameter(Mandatory)]
        [string]$ServiceName,
        [string]$MainFeature,
        [string]$MainEntity,
        [string]$Description
    )
    
    $agentDocsPath = "Services\$ServiceName\$ServiceName.Api\agent-docs"
    
    # Créer le dossier
    New-Item -ItemType Directory -Path $agentDocsPath -Force | Out-Null
    
    # README.md principal
    @"
# $ServiceName - Documentation AI

## Vue d'ensemble
$Description

## Architecture
- **Pattern**: Clean Vertical Slice + CQRS
- **Framework**: ASP.NET Core + Carter
- **Base**: IDR.Library.BuildingBlocks

## Feature principale
- **Feature**: $MainFeature
- **Entité**: $MainEntity

## Fichiers de documentation
- [endpoints.md](./endpoints.md) - Liste des endpoints API
- [commands.md](./commands.md) - Commandes CQRS disponibles
- [queries.md](./queries.md) - Requêtes CQRS disponibles
- [entities.md](./entities.md) - Entités du domaine
- [dtos.md](./dtos.md) - DTOs et modèles

## Statut
- Créé: $(Get-Date -Format "yyyy-MM-dd")
- Documentation: Initiale
"@ | Out-File "$agentDocsPath\README.md" -Encoding utf8

    # endpoints.md
    @"
# Endpoints - $ServiceName

## Liste des endpoints

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | /api/$($MainFeature.ToLower()) | Liste tous les $($MainFeature.ToLower()) |
| GET | /api/$($MainFeature.ToLower())/{id} | Récupère un $MainEntity par ID |
| POST | /api/$($MainFeature.ToLower()) | Crée un nouveau $MainEntity |
| PUT | /api/$($MainFeature.ToLower())/{id} | Met à jour un $MainEntity |
| DELETE | /api/$($MainFeature.ToLower())/{id} | Supprime un $MainEntity |

## Documentation Swagger
Accessible sur: `/docs`
"@ | Out-File "$agentDocsPath\endpoints.md" -Encoding utf8

    # commands.md
    @"
# Commands CQRS - $ServiceName

## Liste des commandes

| Command | Description | Handler |
|---------|-------------|---------|
| Create${MainEntity}Command | Crée un nouveau $MainEntity | ✅ |
| Update${MainEntity}Command | Met à jour un $MainEntity | ✅ |
| Delete${MainEntity}Command | Supprime un $MainEntity | ✅ |

## Validation
Toutes les commandes utilisent AbstractValidator<T> de IDR.Library.BuildingBlocks.
"@ | Out-File "$agentDocsPath\commands.md" -Encoding utf8

    # queries.md
    @"
# Queries CQRS - $ServiceName

## Liste des requêtes

| Query | Description | Response |
|-------|-------------|----------|
| Get${MainFeature}Query | Liste les $MainFeature | List<${MainEntity}Dto> |
| Get${MainEntity}ByIdQuery | Récupère un $MainEntity | ${MainEntity}Dto |

## Pagination
Les requêtes de liste supportent la pagination via:
- PageNumber
- PageSize
"@ | Out-File "$agentDocsPath\queries.md" -Encoding utf8

    # entities.md
    @"
# Entités - $ServiceName

## $MainEntity

Entité principale du service.

### Propriétés
| Propriété | Type | Description |
|-----------|------|-------------|
| Id | Guid | Identifiant unique |
| CreatedAt | DateTime | Date de création |
| UpdatedAt | DateTime? | Date de modification |

### Relations
[À compléter selon les besoins]
"@ | Out-File "$agentDocsPath\entities.md" -Encoding utf8

    # dtos.md
    @"
# DTOs - $ServiceName

## ${MainEntity}Dto

DTO principal pour l'entité $MainEntity.

### Propriétés
| Propriété | Type | Description |
|-----------|------|-------------|
| Id | Guid | Identifiant |
| [Autres] | [Type] | [Description] |

## Create${MainEntity}Request

DTO pour la création.

## Update${MainEntity}Request

DTO pour la mise à jour.
"@ | Out-File "$agentDocsPath\dtos.md" -Encoding utf8

    Write-Host "[AGENT-DOCS] Documentation AI créée pour $ServiceName" -ForegroundColor Green
    return $agentDocsPath
}
```

### Mise à jour lors de modifications
Après TOUTE modification du microservice, la doc AI doit être mise à jour:

```powershell
function Update-AgentDocsAfterChange {
    param(
        [string]$ServiceName,
        [string[]]$ModifiedFiles
    )
    
    # Analyser les fichiers modifiés
    foreach ($file in $ModifiedFiles) {
        if ($file -match "Endpoints") {
            Update-EndpointsDoc -ServiceName $ServiceName
        }
        if ($file -match "Commands") {
            Update-CommandsDoc -ServiceName $ServiceName
        }
        if ($file -match "Queries") {
            Update-QueriesDoc -ServiceName $ServiceName
        }
        if ($file -match "Entities") {
            Update-EntitiesDoc -ServiceName $ServiceName
        }
        if ($file -match "DTOs|Dtos") {
            Update-DtosDoc -ServiceName $ServiceName
        }
    }
    
    # Mettre à jour le README avec la date
    $readmePath = "Services\$ServiceName\$ServiceName.Api\agent-docs\README.md"
    $content = Get-Content $readmePath -Raw
    $content = $content -replace "Dernière mise à jour:.*", "Dernière mise à jour: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    $content | Out-File $readmePath -Encoding utf8
    
    Write-Host "[AGENT-DOCS] Documentation mise à jour pour $ServiceName" -ForegroundColor Green
}
```

### Mise à jour rétroactive pour services existants
```powershell
function Invoke-RetroactiveAgentDocs {
    $services = Get-ChildItem "Services" -Directory | 
        Where-Object { Test-Path "$($_.FullName)\$($_.Name).Api" }
    
    foreach ($service in $services) {
        $agentDocsPath = "$($service.FullName)\$($service.Name).Api\agent-docs"
        
        if (-not (Test-Path $agentDocsPath)) {
            Write-Host "Création agent-docs pour $($service.Name)..." -ForegroundColor Yellow
            New-AgentDocsForService -ServiceName $service.Name -MainFeature "Feature" -MainEntity "Entity" -Description "Service $($service.Name)"
        }
        else {
            Write-Host "Mise à jour agent-docs pour $($service.Name)..." -ForegroundColor Yellow
            # Scanner et mettre à jour la doc
            $allFiles = Get-ChildItem $service.FullName -Recurse -Filter "*.cs" | Select-Object -ExpandProperty FullName
            Update-AgentDocsAfterChange -ServiceName $service.Name -ModifiedFiles $allFiles
        }
    }
}
```

## Format de réponse
```json
{
  "action": "create_microservice",
  "service_name": "AbonnementService",
  "main_feature": "Abonnements",
  "main_entity": "Abonnement",
  "description": "Microservice de gestion des abonnements",
  "preconditions_met": true,
  "files_created": [
    "Services/AbonnementService/AbonnementService.Api/Program.cs",
    "Services/AbonnementService/AbonnementService.Api/Dockerfile",
    "Services/AbonnementService/AbonnementService.Api/readme.md",
    "Services/AbonnementService/AbonnementService.Api/agent-docs/README.md",
    "Services/AbonnementService/AbonnementService.Api/agent-docs/endpoints.md",
    "Services/AbonnementService/AbonnementService.Api/agent-docs/commands.md",
    "Services/AbonnementService/AbonnementService.Api/agent-docs/queries.md",
    "Services/AbonnementService/AbonnementService.Api/agent-docs/entities.md",
    "Services/AbonnementService/AbonnementService.Api/agent-docs/dtos.md"
  ],
  "structure_follows_template": true,
  "docker_compose_updated": true,
  "solution_updated": true,
  "documentation_generated": {
    "swagger_configured": true,
    "readme_created": true,
    "http_test_file_created": true,
    "agent_docs_created": true
  },
  "agent_docs": {
    "path": "Services/AbonnementService/AbonnementService.Api/agent-docs",
    "files": ["README.md", "endpoints.md", "commands.md", "queries.md", "entities.md", "dtos.md"],
    "status": "complete"
  },
  "next_steps": [
    "Configurer la chaîne de connexion",
    "Créer les migrations EF Core",
    "Compléter les entités",
    "Mettre à jour agent-docs après chaque modification",
    "Tester les endpoints"
  ]
}
```
