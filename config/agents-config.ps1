# Configuration des Agents DashBoardAdmin
# config/agents-config.ps1

# ============================================
# CONFIGURATION GITHUB
# ============================================

$Config = @{
    GitHub = @{
        Owner = "VOTRE-ORG"
        Repo = "DashBoardAdmin"
        ProjectNumber = 1
    }
    
    # ============================================
    # CONFIGURATION DU MODÈLE CLAUDE
    # ============================================
    # Modèles disponibles:
    # - claude-sonnet-4-20250514     (Recommandé - équilibre performance/coût)
    # - claude-opus-4-20250514       (Plus puissant - pour tâches complexes)
    # - claude-3-5-sonnet-20241022   (Version précédente)
    
    Claude = @{
        Model = "claude-sonnet-4-20250514"
        
        # Options par type de tâche
        ModelByTask = @{
            Analysis = "claude-sonnet-4-20250514"      # Analyse des issues
            Coding = "claude-sonnet-4-20250514"        # Implémentation du code
            Documentation = "claude-sonnet-4-20250514" # Génération de documentation
            NewService = "claude-opus-4-20250514"      # Création de nouveaux microservices (plus complexe)
        }
    }
    
    # ============================================
    # COLONNES DU PROJECT BOARD
    # ============================================
    
    Columns = @{
        Analyse = "Analyse"
        Todo = "Todo"
        AnalyseBlock = "AnalyseBlock"
        InProgress = "In Progress"
        Review = "In Review"
        ATester = "A Tester"
        Done = "Done"
    }
    
    # ============================================
    # INTERVALLES DE POLLING
    # ============================================
    
    Polling = @{
        DefaultInterval = 60        # Secondes entre chaque vérification
        MinInterval = 30            # Minimum autorisé
        MaxInterval = 300           # Maximum autorisé
    }
    
    # ============================================
    # LIBRAIRIES INTERNES (TOUJOURS À JOUR)
    # ============================================
    
    InternalLibraries = @{
        "IDR.Library.BuildingBlocks" = @{
            Name = "IDR.Library.BuildingBlocks"
            NuGetPath = "$env:USERPROFILE\.nuget\packages\idr.library.buildingblocks"
            DocsFolder = "contentFiles\any\any\agent-docs"
            AlwaysUpdate = $true
        }
        "IDR.Library.Blazor" = @{
            Name = "IDR.Library.Blazor"
            NuGetPath = "$env:USERPROFILE\.nuget\packages\idr.library.blazor"
            DocsFolder = "contentFiles\any\any\agent-docs"
            AlwaysUpdate = $true
        }
    }
    
    # Documentation des librairies (chemin automatique depuis NuGet)
    LibraryDocs = @{
        Manifest = "idr-library-manifest.json"
        AutoLoad = $true
        BasePath = "$env:USERPROFILE\.nuget\packages"
    }
    
    # ============================================
    # ARCHITECTURE DES PROJETS
    # ============================================
    
    Projects = @{
        BackendAdmin = @{
            Type = "CleanVerticalSlice"
            Path = "BackendAdmin"
            Layers = @("Api", "Application", "Domain", "Infrastructure")
        }
        FrontendAdmin = @{
            Type = "BlazorHybrid"
            Path = "FrontendAdmin"
            Projects = @("FrontendAdmin", "FrontendAdmin.Shared", "FrontendAdmin.Web", "FrontendAdmin.Web.Client")
        }
        Services = @{
            Type = "Microservices"
            Path = "Services"
            Template = "CleanVerticalSlice"
            Existing = @("MagasinService", "MenuService")
            Future = @("AbonnementService", "FacturationService", "TresorerieService")
        }
    }
    
    # ============================================
    # RÈGLES DE VALIDATION
    # ============================================
    
    ValidationRules = @{
        # Ne jamais modifier ces packages sans demande explicite
        ProtectedPackages = $true
        
        # Toujours comprendre le code avant modification
        RequireCodeUnderstanding = $true
        
        # Bloquer si contradiction détectée
        BlockOnContradiction = $true
        
        # Ne pas inventer si information manquante
        BlockOnMissingInfo = $true
        
        # Documentation API obligatoire pour microservices
        RequireApiDocumentation = $true
    }
    
    # ============================================
    # TEMPLATES DE TESTS
    # ============================================
    
    Testing = @{
        Framework = "xUnit"
        BDD = "Xunit.Gherkin.Quick"
        Assertions = "FluentAssertions"
        Mocking = "Moq"
        ApiTesting = "Microsoft.AspNetCore.Mvc.Testing"
        BlazorTesting = "bUnit"
    }
    
    # ============================================
    # LOGS ET DEBUG
    # ============================================
    
    Logging = @{
        Verbose = $false
        LogFile = ".claude/agent-logs.txt"
        MaxLogSize = 10MB
    }
}

# ============================================
# FONCTIONS UTILITAIRES
# ============================================

function Get-AgentConfig {
    return $Config
}

function Get-ModelForTask {
    param([string]$TaskType)
    
    if ($Config.Claude.ModelByTask.ContainsKey($TaskType)) {
        return $Config.Claude.ModelByTask[$TaskType]
    }
    return $Config.Claude.Model
}

function Test-IsInternalLibrary {
    param([string]$PackageName)
    
    return $Config.InternalLibraries.ContainsKey($PackageName)
}

function Get-ProjectConfig {
    param([string]$ProjectName)
    
    switch -Wildcard ($ProjectName) {
        "BackendAdmin*" { return $Config.Projects.BackendAdmin }
        "FrontendAdmin*" { return $Config.Projects.FrontendAdmin }
        "*Service" { return $Config.Projects.Services }
        default { return $null }
    }
}

# ============================================
# FONCTIONS DE LECTURE DOCUMENTATION IDR
# ============================================

<#
.SYNOPSIS
    Récupère la version installée d'un package IDR Library
.PARAMETER PackageName
    Nom du package (IDR.Library.BuildingBlocks ou IDR.Library.Blazor)
.EXAMPLE
    Get-IDRLibraryVersion -PackageName "IDR.Library.BuildingBlocks"
#>
function Get-IDRLibraryVersion {
    param(
        [Parameter(Mandatory=$true)]
        [ValidateSet("IDR.Library.BuildingBlocks", "IDR.Library.Blazor")]
        [string]$PackageName
    )
    
    $libConfig = $Config.InternalLibraries[$PackageName]
    if (-not $libConfig) {
        Write-Warning "Package $PackageName non configuré"
        return $null
    }
    
    $nugetPath = $ExecutionContext.InvokeCommand.ExpandString($libConfig.NuGetPath)
    
    if (-not (Test-Path $nugetPath)) {
        Write-Warning "Chemin NuGet non trouvé: $nugetPath"
        return $null
    }
    
    # Récupérer la version la plus récente installée
    $versions = Get-ChildItem -Path $nugetPath -Directory | 
        Where-Object { $_.Name -match '^\d+\.\d+\.\d+' } |
        Sort-Object { [Version]($_.Name -replace '-.*$', '') } -Descending
    
    if ($versions.Count -eq 0) {
        Write-Warning "Aucune version trouvée pour $PackageName"
        return $null
    }
    
    $latestVersion = $versions[0]
    
    return @{
        PackageName = $PackageName
        Version = $latestVersion.Name
        Path = $latestVersion.FullName
        DocsPath = Join-Path $latestVersion.FullName $libConfig.DocsFolder
    }
}

<#
.SYNOPSIS
    Lit automatiquement la documentation d'un package IDR Library
.PARAMETER PackageName
    Nom du package (IDR.Library.BuildingBlocks ou IDR.Library.Blazor)
.EXAMPLE
    Read-IDRLibraryDocs -PackageName "IDR.Library.BuildingBlocks"
#>
function Read-IDRLibraryDocs {
    param(
        [Parameter(Mandatory=$true)]
        [ValidateSet("IDR.Library.BuildingBlocks", "IDR.Library.Blazor")]
        [string]$PackageName
    )
    
    $versionInfo = Get-IDRLibraryVersion -PackageName $PackageName
    
    if (-not $versionInfo) {
        return @{
            Success = $false
            Error = "Package $PackageName non trouvé"
        }
    }
    
    $docsPath = $versionInfo.DocsPath
    
    if (-not (Test-Path $docsPath)) {
        return @{
            Success = $false
            Error = "Dossier de documentation non trouvé: $docsPath"
            Version = $versionInfo.Version
        }
    }
    
    # Lire tous les fichiers de documentation
    $docFiles = Get-ChildItem -Path $docsPath -File -Recurse
    $documentation = @{}
    
    foreach ($file in $docFiles) {
        $relativePath = $file.FullName.Replace($docsPath, "").TrimStart("\", "/")
        $content = Get-Content -Path $file.FullName -Raw -ErrorAction SilentlyContinue
        
        if ($content) {
            $documentation[$relativePath] = $content
        }
    }
    
    return @{
        Success = $true
        PackageName = $PackageName
        Version = $versionInfo.Version
        Path = $versionInfo.Path
        DocsPath = $docsPath
        Files = $docFiles.Name
        Documentation = $documentation
    }
}

<#
.SYNOPSIS
    Lit toute la documentation des packages IDR Library installés
.DESCRIPTION
    Lit automatiquement la documentation de IDR.Library.BuildingBlocks et IDR.Library.Blazor
    depuis les packages NuGet installés localement
.EXAMPLE
    $docs = Read-AllIDRLibraryDocs
    $docs.BuildingBlocks.Documentation
#>
function Read-AllIDRLibraryDocs {
    Write-Host ""
    Write-Host "╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║     LECTURE AUTOMATIQUE DOCUMENTATION IDR LIBRARY            ║" -ForegroundColor Cyan
    Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""
    
    $result = @{
        Success = $true
        Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        Libraries = @{}
    }
    
    foreach ($libName in $Config.InternalLibraries.Keys) {
        Write-Host "[IDR] Lecture de $libName..." -ForegroundColor Yellow
        
        $libDocs = Read-IDRLibraryDocs -PackageName $libName
        
        if ($libDocs.Success) {
            Write-Host "   ✅ Version: $($libDocs.Version)" -ForegroundColor Green
            Write-Host "   📁 Fichiers: $($libDocs.Files -join ', ')" -ForegroundColor DarkGray
            $result.Libraries[$libName] = $libDocs
        }
        else {
            Write-Host "   ❌ Erreur: $($libDocs.Error)" -ForegroundColor Red
            $result.Success = $false
            $result.Libraries[$libName] = $libDocs
        }
    }
    
    Write-Host ""
    
    # Créer des alias pour faciliter l'accès
    $result.BuildingBlocks = $result.Libraries["IDR.Library.BuildingBlocks"]
    $result.Blazor = $result.Libraries["IDR.Library.Blazor"]
    
    return $result
}

<#
.SYNOPSIS
    Obtient le chemin de la documentation pour un package spécifique
.PARAMETER PackageName
    Nom du package
.EXAMPLE
    Get-IDRDocsPath -PackageName "IDR.Library.BuildingBlocks"
#>
function Get-IDRDocsPath {
    param(
        [Parameter(Mandatory=$true)]
        [ValidateSet("IDR.Library.BuildingBlocks", "IDR.Library.Blazor")]
        [string]$PackageName
    )
    
    $versionInfo = Get-IDRLibraryVersion -PackageName $PackageName
    
    if ($versionInfo) {
        return $versionInfo.DocsPath
    }
    
    return $null
}

<#
.SYNOPSIS
    Liste tous les fichiers de documentation disponibles
.EXAMPLE
    Get-IDRDocsFiles
#>
function Get-IDRDocsFiles {
    $result = @()
    
    foreach ($libName in $Config.InternalLibraries.Keys) {
        $versionInfo = Get-IDRLibraryVersion -PackageName $libName
        
        if ($versionInfo -and (Test-Path $versionInfo.DocsPath)) {
            $files = Get-ChildItem -Path $versionInfo.DocsPath -File -Recurse
            
            foreach ($file in $files) {
                $result += [PSCustomObject]@{
                    Package = $libName
                    Version = $versionInfo.Version
                    FileName = $file.Name
                    FullPath = $file.FullName
                    Size = $file.Length
                }
            }
        }
    }
    
    return $result
}

<#
.SYNOPSIS
    Commande directe pour lister la documentation (compatible avec le chemin utilisateur)
.EXAMPLE
    Show-IDRDocumentation
#>
function Show-IDRDocumentation {
    Write-Host ""
    Write-Host "Documentation IDR Library disponible:" -ForegroundColor Cyan
    Write-Host ""
    
    # IDR.Library.BuildingBlocks
    Write-Host "[IDR.Library.BuildingBlocks]" -ForegroundColor Yellow
    $bbPath = "$env:USERPROFILE\.nuget\packages\idr.library.buildingblocks\*\contentFiles\any\any\agent-docs\*"
    $bbFiles = Get-Item $bbPath -ErrorAction SilentlyContinue
    if ($bbFiles) {
        $bbFiles | ForEach-Object { Write-Host "   $_" -ForegroundColor White }
    }
    else {
        Write-Host "   (Non installé ou documentation non disponible)" -ForegroundColor DarkGray
    }
    
    Write-Host ""
    
    # IDR.Library.Blazor
    Write-Host "[IDR.Library.Blazor]" -ForegroundColor Yellow
    $blazorPath = "$env:USERPROFILE\.nuget\packages\idr.library.blazor\*\contentFiles\any\any\agent-docs\*"
    $blazorFiles = Get-Item $blazorPath -ErrorAction SilentlyContinue
    if ($blazorFiles) {
        $blazorFiles | ForEach-Object { Write-Host "   $_" -ForegroundColor White }
    }
    else {
        Write-Host "   (Non installé ou documentation non disponible)" -ForegroundColor DarkGray
    }
    
    Write-Host ""
}

# Export
Export-ModuleMember -Variable Config -Function @(
    'Get-AgentConfig',
    'Get-ModelForTask',
    'Test-IsInternalLibrary',
    'Get-ProjectConfig',
    'Get-IDRLibraryVersion',
    'Read-IDRLibraryDocs',
    'Read-AllIDRLibraryDocs',
    'Get-IDRDocsPath',
    'Get-IDRDocsFiles',
    'Show-IDRDocumentation'
)
