# scripts/Start-DashBoardAdminAgent.ps1
# Agent autonome pour DashBoardAdmin

param(
    [string]$Owner = "KOMANSERVICE",
    [string]$Repo = "dashboardadmin",
    [int]$ProjectNumber = 5,
    [int]$PollingInterval = 60,
    
    # Configuration du repo des packages IDR (pour les issues de composants/bugs)
    [string]$Owner_package = "KOMANSERVICE",
    [string]$Repo_package = "IDR.Library",
    [int]$ProjectNumber_package = 4,
    

    # Modes de fonctionnement
    [switch]$AnalysisOnly,
    [switch]$CoderOnly,
    
    # Choix du modele Claude
    [ValidateSet("claude-sonnet-4-20250514", "claude-opus-4-20250514", "claude-opus-4-5-20251101", "claude-3-5-sonnet-20241022")]
    [string]$Model = "claude-opus-4-5-20251101",
    
    # Options avancees
    [switch]$DryRun,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

# ============================================
# CONFIGURATION
# ============================================

# Variables projet principal
$env:GITHUB_OWNER = $Owner
$env:GITHUB_REPO = $Repo
$env:PROJECT_NUMBER = $ProjectNumber

# Variables repo packages IDR
$env:GITHUB_OWNER_PACKAGE = $Owner_package
$env:GITHUB_REPO_PACKAGE = $Repo_package
$env:PROJECT_NUMBER_PACKAGE = $ProjectNumber_package

$env:CLAUDE_MODEL = $Model

# Colonnes du Project Board
$Columns = @{
    Analyse = "Analyse"
    Todo = "Todo"
    AnalyseBlock = "AnalyseBlock"
    InProgress = "In Progress"
    Review = "In Review"
    ATester = "A Tester"
    Done = "Done"
}

# ============================================
# AFFICHAGE INITIAL
# ============================================

Write-Host ""
Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host "           DASHBOARDADMIN - AGENT AUTONOME                        " -ForegroundColor Cyan
Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host "  Solution: DashBoardAdmin                                        " -ForegroundColor Gray
Write-Host "  - BackendAdmin (Clean Vertical Slice)                           " -ForegroundColor Gray
Write-Host "  - FrontendAdmin (MAUI Blazor Hybrid)                            " -ForegroundColor Gray
Write-Host "  - Microservices (MagasinService, MenuService, etc.)             " -ForegroundColor Gray
Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  [PROJET PRINCIPAL]" -ForegroundColor White
Write-Host "  Owner: $Owner" -ForegroundColor White
Write-Host "  Repo: $Repo" -ForegroundColor White
Write-Host "  Project: #$ProjectNumber" -ForegroundColor White
Write-Host ""
Write-Host "  [REPO PACKAGES IDR]" -ForegroundColor Yellow
Write-Host "  Owner: $Owner_package" -ForegroundColor Yellow
Write-Host "  Repo: $Repo_package" -ForegroundColor Yellow
Write-Host "  Project: #$ProjectNumber_package" -ForegroundColor Yellow
Write-Host ""
Write-Host "  Model: $Model" -ForegroundColor White
Write-Host "  Polling: toutes les $PollingInterval secondes" -ForegroundColor White
Write-Host "==================================================================" -ForegroundColor Cyan

if ($AnalysisOnly) {
    Write-Host "  Mode: ANALYSE UNIQUEMENT" -ForegroundColor Yellow
}
elseif ($CoderOnly) {
    Write-Host "  Mode: CODEUR UNIQUEMENT" -ForegroundColor Yellow
}
else {
    Write-Host "  Mode: COMPLET (Analyse + Codeur)" -ForegroundColor Green
}

if ($DryRun) {
    Write-Host "  DRY RUN - Aucune modification" -ForegroundColor Magenta
}

Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host ""

# ============================================
# VERIFICATION DES PREREQUIS
# ============================================

Write-Host "[PREREQ] Verification des prerequis..." -ForegroundColor Cyan

$allPrereqsMet = $true

# Verifier gh
try {
    $null = gh --version 2>&1
    Write-Host "   [OK] gh (GitHub CLI)" -ForegroundColor Green
}
catch {
    Write-Host "   [X] gh (GitHub CLI) non trouve" -ForegroundColor Red
    $allPrereqsMet = $false
}

# Verifier claude
try {
    $null = claude --version 2>&1
    Write-Host "   [OK] claude (Claude CLI)" -ForegroundColor Green
}
catch {
    Write-Host "   [X] claude (Claude CLI) non trouve" -ForegroundColor Red
    $allPrereqsMet = $false
}

# Verifier dotnet
try {
    $null = dotnet --version 2>&1
    Write-Host "   [OK] dotnet" -ForegroundColor Green
}
catch {
    Write-Host "   [X] dotnet non trouve" -ForegroundColor Red
    $allPrereqsMet = $false
}

# Verifier git
try {
    $null = git --version 2>&1
    Write-Host "   [OK] git" -ForegroundColor Green
}
catch {
    Write-Host "   [X] git non trouve" -ForegroundColor Red
    $allPrereqsMet = $false
}

if (-not $allPrereqsMet) {
    Write-Host ""
    Write-Host "[ERREUR] Certains prerequis manquent." -ForegroundColor Red
    exit 1
}

# Verifier l'authentification GitHub
Write-Host ""
Write-Host "[AUTH] Verification de l'authentification GitHub..." -ForegroundColor Cyan
$authStatus = gh auth status 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERREUR] Non authentifie sur GitHub. Executez 'gh auth login'" -ForegroundColor Red
    exit 1
}
Write-Host "   [OK] Authentifie sur GitHub" -ForegroundColor Green

# ============================================
# LECTURE AUTOMATIQUE DOCUMENTATION IDR LIBRARY
# ============================================

Write-Host ""
Write-Host "[IDR] Lecture automatique de la documentation IDR Library..." -ForegroundColor Cyan

$script:IDRDocsContent = ""

function Read-IDRPackageDocs {
    param([string]$PackageName)
    
    $packageNameLower = $PackageName.ToLower()
    $basePath = Join-Path $env:USERPROFILE ".nuget\packages\$packageNameLower"
    
    if (-not (Test-Path $basePath)) {
        return @{ Success = $false; Error = "Package non installe"; Content = "" }
    }
    
    $versions = Get-ChildItem -Path $basePath -Directory -ErrorAction SilentlyContinue | 
        Where-Object { $_.Name -match '^\d+\.\d+' } |
        Sort-Object { 
            $v = $_.Name -replace '-.*$', ''
            try { [Version]$v } catch { [Version]"0.0.0" }
        } -Descending
    
    if ($versions.Count -eq 0) {
        return @{ Success = $false; Error = "Aucune version trouvee"; Content = "" }
    }
    
    $latestVersion = $versions[0]
    $docsPath = Join-Path $latestVersion.FullName "contentFiles\any\any\agent-docs"
    
    if (-not (Test-Path $docsPath)) {
        return @{ Success = $false; Error = "Dossier agent-docs non trouve"; Version = $latestVersion.Name; Content = "" }
    }
    
    $docFiles = Get-ChildItem -Path $docsPath -File -Recurse -ErrorAction SilentlyContinue
    $allContent = @()
    
    foreach ($file in $docFiles) {
        $content = Get-Content -Path $file.FullName -Raw -ErrorAction SilentlyContinue
        if ($content) {
            $allContent += "=== $($file.Name) ===`n$content`n"
        }
    }
    
    return @{
        Success = $true
        Version = $latestVersion.Name
        Path = $docsPath
        Files = @($docFiles.Name)
        Content = ($allContent -join "`n")
    }
}

# Lire IDR.Library.BuildingBlocks
$bbDocs = Read-IDRPackageDocs -PackageName "IDR.Library.BuildingBlocks"
if ($bbDocs.Success) {
    Write-Host "   [OK] IDR.Library.BuildingBlocks v$($bbDocs.Version)" -ForegroundColor Green
    if ($bbDocs.Files) {
        Write-Host "       Fichiers: $($bbDocs.Files -join ', ')" -ForegroundColor DarkGray
    }
    $script:IDRDocsContent += "`n### IDR.Library.BuildingBlocks v$($bbDocs.Version)`n$($bbDocs.Content)`n"
}
else {
    Write-Host "   [!] IDR.Library.BuildingBlocks: $($bbDocs.Error)" -ForegroundColor Yellow
}

# Lire IDR.Library.Blazor
$blazorDocs = Read-IDRPackageDocs -PackageName "IDR.Library.Blazor"
if ($blazorDocs.Success) {
    Write-Host "   [OK] IDR.Library.Blazor v$($blazorDocs.Version)" -ForegroundColor Green
    if ($blazorDocs.Files) {
        Write-Host "       Fichiers: $($blazorDocs.Files -join ', ')" -ForegroundColor DarkGray
    }
    $script:IDRDocsContent += "`n### IDR.Library.Blazor v$($blazorDocs.Version)`n$($blazorDocs.Content)`n"
}
else {
    Write-Host "   [!] IDR.Library.Blazor: $($blazorDocs.Error)" -ForegroundColor Yellow
}

# ============================================
# FONCTIONS POUR GESTION DES PACKAGES IDR
# ============================================

function New-PackageIssue {
    param(
        [string]$Title,
        [string]$Body,
        [string]$IssueType = "enhancement"
    )
    
    $issueFile = Join-Path $env:TEMP "package-issue-$([Guid]::NewGuid().ToString('N').Substring(0,8)).md"
    $Body | Out-File $issueFile -Encoding utf8
    
    try {
        $result = gh issue create `
            --repo "$Owner_package/$Repo_package" `
            --title $Title `
            --body-file $issueFile `
            --label $IssueType
        
        Write-Host "   [ISSUE] Creee: $result" -ForegroundColor Green
        
        if ($ProjectNumber_package -gt 0) {
            gh project item-add $ProjectNumber_package --owner $Owner_package --url $result 2>$null
        }
        
        return $result
    }
    catch {
        Write-Host "   [ERREUR] Creation issue: $_" -ForegroundColor Red
        return $null
    }
    finally {
        Remove-Item $issueFile -ErrorAction SilentlyContinue
    }
}

# ============================================
# NAVIGATION VERS LE PROJET
# ============================================

$scriptDir = $PSScriptRoot
if ($scriptDir) {
    $projectRoot = Split-Path $scriptDir -Parent
    if (Test-Path $projectRoot) {
        Set-Location $projectRoot
        Write-Host ""
        Write-Host "[DIR] Repertoire de travail: $projectRoot" -ForegroundColor Cyan
    }
}

# ============================================
# GESTION DES ISSUES TRAITEES
# ============================================

$claudeDir = ".claude"
if (-not (Test-Path $claudeDir)) {
    New-Item -ItemType Directory -Path $claudeDir -Force | Out-Null
}

$processedAnalysisFile = Join-Path $claudeDir "processed-analysis.json"
$processedCoderFile = Join-Path $claudeDir "processed-coder.json"

function Get-ProcessedIssues {
    param([string]$FilePath)
    
    if (Test-Path $FilePath) {
        try {
            $content = Get-Content $FilePath -Raw
            if ($content) {
                return @(ConvertFrom-Json $content)
            }
        }
        catch { }
    }
    return @()
}

function Add-ProcessedIssue {
    param(
        [string]$FilePath,
        [int]$IssueNumber
    )
    
    $processed = @(Get-ProcessedIssues -FilePath $FilePath)
    if ($IssueNumber -notin $processed) {
        $processed += $IssueNumber
        $processed | ConvertTo-Json | Out-File $FilePath -Encoding utf8
    }
}

if (-not (Test-Path $processedAnalysisFile)) {
    "[]" | Out-File $processedAnalysisFile -Encoding utf8
}
if (-not (Test-Path $processedCoderFile)) {
    "[]" | Out-File $processedCoderFile -Encoding utf8
}

# ============================================
# RECUPERATION DES ISSUES
# ============================================

function Get-IssuesInColumn {
    param([string]$ColumnName)
    
    try {
        $result = gh project item-list $ProjectNumber --owner $Owner --format json 2>$null
        
        if ([string]::IsNullOrWhiteSpace($result)) {
            return @()
        }
        
        $items = $result | ConvertFrom-Json
        
        if (-not $items -or -not $items.items) {
            return @()
        }
        
        $issues = @()
        foreach ($item in $items.items) {
            if ($item.status -eq $ColumnName -and $item.content.type -eq "Issue") {
                $issues += [PSCustomObject]@{
                    ItemId = $item.id
                    IssueNumber = $item.content.number
                    Title = $item.content.title
                }
            }
        }
        
        return $issues
    }
    catch {
        Write-Host "   [WARN] Erreur recuperation issues: $_" -ForegroundColor Yellow
        return @()
    }
}

# ============================================
# AGENT D'ANALYSE
# ============================================

function Invoke-AnalysisAgent {
    param(
        [int]$IssueNumber,
        [string]$Title
    )
    
    $issueJson = gh issue view $IssueNumber --repo "$Owner/$Repo" --json number,title,body,labels
    
    $promptFile = Join-Path $env:TEMP "analysis-prompt-$IssueNumber.txt"
    
    $promptContent = @"
Tu es l'agent orchestrateur d'analyse pour le projet DashBoardAdmin.

## Issue a analyser
$issueJson

## Documentation IDR Library (LECTURE AUTOMATIQUE)
$script:IDRDocsContent

## Configuration Repo Packages IDR
- Owner: $Owner_package
- Repo: $Repo_package  
- Project: $ProjectNumber_package

## Instructions
1. Lis les fichiers d'agents:
   - .claude/agents/orchestrator.md
   - .claude/agents/backendadmin-analyzer.md
   - .claude/agents/frontendadmin-analyzer.md
   - .claude/agents/microservice-analyzer.md
   - .claude/agents/package-manager.md (NOUVEAU - gestion composants IDR)
   - .claude/agents/migration-manager.md

2. REGLES CRITIQUES:
   - COMPRENDRE le code existant avant modification
   - NE JAMAIS contredire la logique existante
   - NE JAMAIS inventer si information manquante -> BLOQUER
   - UTILISER les elements de IDR.Library.BuildingBlocks (CQRS, Auth, Validation)
   - UTILISER les composants de IDR.Library.Blazor (IdrForm, IdrInput, etc.)

3. REGLES COMPOSANTS FRONTEND:
   - Si un element se repete 3+ fois -> doit devenir un composant
   - Si composant reutilisable detecte -> creer issue dans $Owner_package/$Repo_package
   - Apres mise a jour IDR.Library.Blazor -> remplacer composants locaux
   
4. REGLES PACKAGES IDR:
   - IDR.Library.BuildingBlocks: utiliser ses elements, creer issue UNIQUEMENT si erreur
   - IDR.Library.Blazor: utiliser ses composants, proposer nouveaux si manquants

5. Analyse:
   a. Determine le scope (BackendAdmin/FrontendAdmin/Microservice)
   b. Pour Frontend: detecter elements repetes -> suggerer composants
   c. Verifie coherence avec packages IDR
   d. Si entites modifiees: identifier migrations

6. Actions:
   - Si VALIDE: Genere Gherkin, commente, deplace vers Todo
   - Si BLOQUEE: Commente raison, deplace vers AnalyseBlock
   - Si COMPOSANT MANQUANT: Creer issue dans repo packages

Variables:
- GITHUB_OWNER: $Owner
- GITHUB_REPO: $Repo
- PROJECT_NUMBER: $ProjectNumber
- GITHUB_OWNER_PACKAGE: $Owner_package
- GITHUB_REPO_PACKAGE: $Repo_package
- PROJECT_NUMBER_PACKAGE: $ProjectNumber_package

Commence l'analyse.
"@

    $promptContent | Out-File $promptFile -Encoding utf8
    
    Write-Host "     [ANALYSE] En cours avec $Model..." -ForegroundColor Yellow
    
    if ($DryRun) {
        Write-Host "     [DRY RUN] Simulation" -ForegroundColor Magenta
        Remove-Item $promptFile -ErrorAction SilentlyContinue
        return $true
    }
    
    try {
        Get-Content $promptFile | claude --model $Model --dangerously-skip-permissions 2>&1 | ForEach-Object {
            if ($Verbose) {
                Write-Host "       $_" -ForegroundColor DarkGray
            }
        }
        Remove-Item $promptFile -ErrorAction SilentlyContinue
        return $true
    }
    catch {
        Write-Host "     [ERREUR] Claude: $_" -ForegroundColor Red
        Remove-Item $promptFile -ErrorAction SilentlyContinue
        return $false
    }
}

# ============================================
# AGENT CODEUR
# ============================================

function Invoke-CoderAgent {
    param(
        [int]$IssueNumber,
        [string]$Title
    )
    
    $issueJson = gh issue view $IssueNumber --repo "$Owner/$Repo" --json number,title,body,labels,comments
    
    $promptFile = Join-Path $env:TEMP "coder-prompt-$IssueNumber.txt"
    
    $promptContent = @"
Tu es l'agent codeur autonome pour le projet DashBoardAdmin.

## Issue a implementer
$issueJson

## Documentation IDR Library (LECTURE AUTOMATIQUE)
$script:IDRDocsContent

## Configuration Repo Packages IDR (pour issues composants/bugs)
- Owner: $Owner_package
- Repo: $Repo_package
- Project: $ProjectNumber_package

## Instructions
1. Lis les fichiers:
   - .claude/agents/coder.md
   - .claude/agents/package-manager.md
   - .claude/agents/migration-manager.md

2. REGLES CRITIQUES:
   - LIRE et COMPRENDRE le code existant AVANT de modifier
   - NE JAMAIS contredire la logique existante
   - UTILISER IDR.Library.BuildingBlocks pour CQRS, Auth, Validation
   - UTILISER IDR.Library.Blazor pour composants UI

3. REGLES COMPOSANTS (FRONTEND):
   - Si element repete 3+ fois -> DOIT devenir composant
   - Verifier si composant existe dans IDR.Library.Blazor
   - Si existe -> utiliser le composant IDR (prefixe Idr*)
   - Si n'existe pas -> creer issue dans ${Owner_package/$Repo_package}:
     gh issue create --repo "$Owner_package/$Repo_package" --title "[Component] Nouveau: NomComposant" --body "..."
   - Apres mise a jour package -> remplacer composants locaux par IDR

4. REGLES PACKAGES IDR:
   - IDR.Library.BuildingBlocks: 
     * TOUJOURS utiliser ICommand, IQuery, ICommandHandler, IQueryHandler
     * TOUJOURS utiliser AbstractValidator<T>
     * En cas d'ERREUR uniquement -> creer issue dans repo packages
   - IDR.Library.Blazor:
     * TOUJOURS utiliser composants Idr* disponibles
     * Si composant manquant -> creer issue pour nouveau composant

5. Workflow:
   a. Lire code existant
   b. Creer branche feature/$IssueNumber-slug
   c. Implementer avec composants IDR
   d. Si element repete -> proposer composant IDR
   e. Si erreur package IDR -> creer issue dans repo packages
   f. Si entites modifiees -> migration EF Core
   g. Tests + Commit + PR + Merge

6. CREATION ISSUE PACKAGE (si necessaire):
   Pour nouveau composant:
   gh issue create --repo "$Owner_package/$Repo_package" --title "[Component] Idr{Nom}" --label "enhancement,component"
   
   Pour bug/erreur:
   gh issue create --repo "$Owner_package/$Repo_package" --title "[Bug] {Description}" --label "bug"

Variables:
- GITHUB_OWNER: $Owner
- GITHUB_REPO: $Repo
- GITHUB_OWNER_PACKAGE: $Owner_package
- GITHUB_REPO_PACKAGE: $Repo_package

Commence l'implementation.
"@

    $promptContent | Out-File $promptFile -Encoding utf8
    
    Write-Host "     [CODER] En cours avec $Model..." -ForegroundColor Magenta
    
    if ($DryRun) {
        Write-Host "     [DRY RUN] Simulation" -ForegroundColor Magenta
        Remove-Item $promptFile -ErrorAction SilentlyContinue
        return $true
    }
    
    try {
        Get-Content $promptFile | claude --model $Model --dangerously-skip-permissions 2>&1 | ForEach-Object {
            if ($Verbose) {
                Write-Host "       $_" -ForegroundColor DarkGray
            }
        }
        Remove-Item $promptFile -ErrorAction SilentlyContinue
        return $true
    }
    catch {
        Write-Host "     [ERREUR] Claude: $_" -ForegroundColor Red
        Remove-Item $promptFile -ErrorAction SilentlyContinue
        return $false
    }
}

# ============================================
# BOUCLE PRINCIPALE
# ============================================

Write-Host ""
Write-Host "[START] Agents autonomes actifs" -ForegroundColor Green
Write-Host "   Appuyez sur Ctrl+C pour arreter"
Write-Host ""

$iteration = 0

while ($true) {
    $iteration++
    $timestamp = Get-Date -Format "HH:mm:ss"
    
    Write-Host "[$timestamp] ====================================================" -ForegroundColor DarkCyan
    Write-Host "[$timestamp] [CHECK] Iteration #$iteration" -ForegroundColor DarkCyan
    
    try {
        # AGENT ANALYSE
        if (-not $CoderOnly) {
            Write-Host "[$timestamp] [ANALYSE] Verification colonne '$($Columns.Analyse)'..." -ForegroundColor Blue
            
            $analysisIssues = @(Get-IssuesInColumn -ColumnName $Columns.Analyse)
            $processedAnalysis = @(Get-ProcessedIssues -FilePath $processedAnalysisFile)
            $newAnalysisIssues = @($analysisIssues | Where-Object { $_.IssueNumber -notin $processedAnalysis })
            
            if ($newAnalysisIssues.Count -eq 0) {
                Write-Host "[$timestamp] [ANALYSE] Aucune nouvelle issue" -ForegroundColor DarkGray
            }
            else {
                Write-Host "[$timestamp] [ANALYSE] $($newAnalysisIssues.Count) issue(s)" -ForegroundColor Green
                
                foreach ($issue in $newAnalysisIssues) {
                    Write-Host "[$timestamp]   -> #$($issue.IssueNumber): $($issue.Title)" -ForegroundColor White
                    
                    $success = Invoke-AnalysisAgent -IssueNumber $issue.IssueNumber -Title $issue.Title
                    
                    if ($success) {
                        Add-ProcessedIssue -FilePath $processedAnalysisFile -IssueNumber $issue.IssueNumber
                        Write-Host "[$timestamp]   [OK] #$($issue.IssueNumber) analysee" -ForegroundColor Green
                    }
                    else {
                        Write-Host "[$timestamp]   [ERREUR] #$($issue.IssueNumber)" -ForegroundColor Red
                    }
                }
            }
        }
        
        # AGENT CODEUR
        if (-not $AnalysisOnly) {
            Write-Host "[$timestamp] [CODER] Verification colonne '$($Columns.Todo)'..." -ForegroundColor Magenta
            
            $todoIssues = @(Get-IssuesInColumn -ColumnName $Columns.Todo)
            $processedCoder = @(Get-ProcessedIssues -FilePath $processedCoderFile)
            $newTodoIssues = @($todoIssues | Where-Object { $_.IssueNumber -notin $processedCoder })
            
            if ($newTodoIssues.Count -eq 0) {
                Write-Host "[$timestamp] [CODER] Aucune nouvelle issue" -ForegroundColor DarkGray
            }
            else {
                Write-Host "[$timestamp] [CODER] $($newTodoIssues.Count) issue(s)" -ForegroundColor Green
                
                $issue = $newTodoIssues[0]
                Write-Host "[$timestamp]   -> #$($issue.IssueNumber): $($issue.Title)" -ForegroundColor White
                
                $success = Invoke-CoderAgent -IssueNumber $issue.IssueNumber -Title $issue.Title
                
                if ($success) {
                    Add-ProcessedIssue -FilePath $processedCoderFile -IssueNumber $issue.IssueNumber
                    Write-Host "[$timestamp]   [OK] #$($issue.IssueNumber) implementee" -ForegroundColor Green
                }
                else {
                    Write-Host "[$timestamp]   [ERREUR] #$($issue.IssueNumber)" -ForegroundColor Red
                }
            }
        }
    }
    catch {
        Write-Host "[$timestamp] [ERREUR] Exception: $_" -ForegroundColor Red
    }
    
    Write-Host "[$timestamp] [WAIT] Prochaine verification dans $PollingInterval sec..." -ForegroundColor DarkGray
    Start-Sleep -Seconds $PollingInterval
}

