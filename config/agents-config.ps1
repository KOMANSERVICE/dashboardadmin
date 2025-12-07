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
    # - claude-opus-4-5-20251101     (Le plus puissant - recommandé pour dev)
    # - claude-sonnet-4-5-20250514   (Rapide et performant)
    # - claude-sonnet-4-20250514     (Version standard)
    
    Claude = @{
        Model = "claude-opus-4-5-20251101"
        
        # Options par type de tâche
        ModelByTask = @{
            Analysis = "claude-opus-4-5-20251101"      # Analyse des issues
            Coding = "claude-opus-4-5-20251101"        # Implémentation du code
            Documentation = "claude-opus-4-5-20251101" # Génération de documentation
            NewService = "claude-opus-4-5-20251101"    # Création de nouveaux microservices
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
    # PRIORITÉ DE TRAITEMENT DES ISSUES
    # ============================================
    
    ProcessingPriority = @(
        @{ Column = "In Review"; Action = "FinishMerge"; Description = "Terminer le merge" }
        @{ Column = "In Progress"; Action = "ContinueDev"; Description = "Terminer le développement" }
        @{ Column = "Analyse"; Action = "Analyze"; Description = "Analyser l'issue" }
        @{ Column = "Todo"; Action = "StartDev"; Description = "Commencer le développement" }
    )
    
    # ============================================
    # RÈGLES DE DÉPLACEMENT DES CARTES
    # ============================================
    
    ColumnTransitions = @{
        # Après analyse
        AnalyseValid = "Todo"
        AnalyseBlocked = "AnalyseBlock"
        
        # Après développement
        DevStarted = "In Progress"
        PRCreated = "In Review"
        MergeCompleted = "A Tester"
        
        # Le testeur fermera l'issue
        # Done est géré manuellement
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

# ============================================
# FONCTIONS DE DÉPLACEMENT DES CARTES (CASE-INSENSITIVE)
# ============================================

<#
.SYNOPSIS
    Compare deux noms de colonne de manière case-insensitive
.PARAMETER Actual
    Nom de colonne actuel
.PARAMETER Expected
    Nom de colonne attendu
.EXAMPLE
    Compare-ColumnName -Actual "a tester" -Expected "A Tester"  # Retourne $true
#>
function Compare-ColumnName {
    param(
        [string]$Actual,
        [string]$Expected
    )
    
    # Normaliser: trim, lowercase, remplacer espaces multiples
    $normalizedActual = ($Actual -replace '\s+', ' ').Trim().ToLower()
    $normalizedExpected = ($Expected -replace '\s+', ' ').Trim().ToLower()
    
    return $normalizedActual -eq $normalizedExpected
}

<#
.SYNOPSIS
    Déplace une issue vers une colonne spécifique (case-insensitive)
.PARAMETER IssueNumber
    Numéro de l'issue
.PARAMETER TargetColumn
    Nom de la colonne cible
.EXAMPLE
    Move-IssueToColumn -IssueNumber 42 -TargetColumn "A Tester"
#>
function Move-IssueToColumn {
    param(
        [Parameter(Mandatory)]
        [int]$IssueNumber,
        
        [Parameter(Mandatory)]
        [string]$TargetColumn
    )
    
    $Owner = $env:GITHUB_OWNER
    $Repo = $env:GITHUB_REPO
    $ProjectNumber = [int]$env:PROJECT_NUMBER
    
    try {
        Write-Host "[MOVE] Deplacement issue #$IssueNumber vers '$TargetColumn'..." -ForegroundColor Cyan
        
        # Recuperer l'item via gh project item-list (plus fiable)
        $result = gh project item-list $ProjectNumber --owner $Owner --format json 2>$null
        if ([string]::IsNullOrWhiteSpace($result)) {
            Write-Host "   [ERROR] Impossible de lister les items du project" -ForegroundColor Red
            return $false
        }
        
        $items = $result | ConvertFrom-Json
        $item = $items.items | Where-Object { 
            $_.content.type -eq "Issue" -and $_.content.number -eq $IssueNumber 
        } | Select-Object -First 1
        
        if (-not $item) {
            Write-Host "   [ERROR] Issue #$IssueNumber non trouvee dans le project" -ForegroundColor Red
            return $false
        }
        
        # Verifier si deja dans la bonne colonne
        if (Compare-ColumnName -Actual $item.status -Expected $TargetColumn) {
            Write-Host "   [OK] Issue #$IssueNumber deja dans '$TargetColumn'" -ForegroundColor Green
            return $true
        }
        
        # Essayer d'abord avec organization
        $projectId = $null
        
        # Utiliser les variables GraphQL pour eviter les problemes d'echappement
        $orgResult = gh api graphql `
            -f query='query($login: String!, $number: Int!) { organization(login: $login) { projectV2(number: $number) { id } } }' `
            -f login="$Owner" `
            -F number=$ProjectNumber 2>$null
        
        if ($orgResult) {
            $orgData = $orgResult | ConvertFrom-Json
            if ($orgData.data.organization.projectV2) {
                $projectId = $orgData.data.organization.projectV2.id
            }
        }
        
        # Si pas trouve, essayer avec user
        if (-not $projectId) {
            $userResult = gh api graphql `
                -f query='query($login: String!, $number: Int!) { user(login: $login) { projectV2(number: $number) { id } } }' `
                -f login="$Owner" `
                -F number=$ProjectNumber 2>$null
            
            if ($userResult) {
                $userData = $userResult | ConvertFrom-Json
                if ($userData.data.user.projectV2) {
                    $projectId = $userData.data.user.projectV2.id
                }
            }
        }
        
        if (-not $projectId) {
            Write-Host "   [ERROR] Project non trouve pour $Owner" -ForegroundColor Red
            return $false
        }
        
        # Recuperer le field Status et les options
        $fieldResult = gh api graphql `
            -f query='query($nodeId: ID!, $fieldName: String!) { node(id: $nodeId) { ... on ProjectV2 { field(name: $fieldName) { ... on ProjectV2SingleSelectField { id options { id name } } } } } }' `
            -f nodeId="$projectId" `
            -f fieldName="Status" 2>$null
        
        if (-not $fieldResult) {
            Write-Host "   [ERROR] Impossible de recuperer le champ Status" -ForegroundColor Red
            return $false
        }
        
        $fieldData = $fieldResult | ConvertFrom-Json
        if (-not $fieldData.data.node.field) {
            Write-Host "   [ERROR] Champ 'Status' non trouve" -ForegroundColor Red
            return $false
        }
        
        $statusFieldId = $fieldData.data.node.field.id
        
        # Trouver l'option avec comparaison CASE-INSENSITIVE
        $targetOption = $fieldData.data.node.field.options | 
            Where-Object { Compare-ColumnName -Actual $_.name -Expected $TargetColumn } | 
            Select-Object -First 1
        
        if (-not $targetOption) {
            Write-Host "   [ERROR] Colonne '$TargetColumn' non trouvee" -ForegroundColor Red
            Write-Host "   Colonnes disponibles:" -ForegroundColor Yellow
            $fieldData.data.node.field.options | ForEach-Object { Write-Host "      - $($_.name)" }
            return $false
        }
        
        # Deplacer l'item
        $mutationResult = gh api graphql `
            -f query='mutation($projectId: ID!, $itemId: ID!, $fieldId: ID!, $optionId: String!) { updateProjectV2ItemFieldValue(input: { projectId: $projectId, itemId: $itemId, fieldId: $fieldId, value: { singleSelectOptionId: $optionId } }) { projectV2Item { id } } }' `
            -f projectId="$projectId" `
            -f itemId="$($item.id)" `
            -f fieldId="$statusFieldId" `
            -f optionId="$($targetOption.id)" 2>&1
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "   [ERROR] Echec: $mutationResult" -ForegroundColor Red
            return $false
        }
        
        Write-Host "   [OK] Issue #$IssueNumber deplacee vers '$TargetColumn'" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "   [ERROR] Exception: $_" -ForegroundColor Red
        return $false
    }
}

<#
.SYNOPSIS
    Récupère la colonne actuelle d'une issue
.PARAMETER IssueNumber
    Numéro de l'issue
.EXAMPLE
    Get-IssueColumn -IssueNumber 42
#>
function Get-IssueColumn {
    param([int]$IssueNumber)
    
    $Owner = $env:GITHUB_OWNER
    $Repo = $env:GITHUB_REPO
    $ProjectNumber = $env:PROJECT_NUMBER
    
    try {
        $result = gh project item-list $ProjectNumber --owner $Owner --format json 2>$null
        $items = $result | ConvertFrom-Json
        
        $item = $items.items | Where-Object { 
            $_.content.type -eq "Issue" -and $_.content.number -eq $IssueNumber 
        } | Select-Object -First 1
        
        if ($item) {
            return $item.status
        }
        return $null
    }
    catch {
        return $null
    }
}

<#
.SYNOPSIS
    Vérifie si une issue est dans une colonne spécifique (case-insensitive)
.EXAMPLE
    Test-IssueInColumn -IssueNumber 42 -ColumnName "a tester"  # Fonctionne même si la colonne est "A Tester"
#>
function Test-IssueInColumn {
    param(
        [int]$IssueNumber,
        [string]$ColumnName
    )
    
    $currentColumn = Get-IssueColumn -IssueNumber $IssueNumber
    if ($currentColumn) {
        return Compare-ColumnName -Actual $currentColumn -Expected $ColumnName
    }
    return $false
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
    'Show-IDRDocumentation',
    'Compare-ColumnName',
    'Move-IssueToColumn',
    'Get-IssueColumn',
    'Test-IssueInColumn'
)
