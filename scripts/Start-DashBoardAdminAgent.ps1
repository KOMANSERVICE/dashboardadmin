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
    [ValidateSet("claude-sonnet-4-20250514", "claude-opus-4-20250514", "claude-3-5-sonnet-20241022")]
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

# Colonnes du Project Board (noms canoniques)
# Note: La comparaison sera CASE-INSENSITIVE
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
# FONCTIONS DE GESTION DES COLONNES (CASE-INSENSITIVE)
# ============================================

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

function Get-IssuesInColumnCaseInsensitive {
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
            # Comparaison CASE-INSENSITIVE
            if ((Compare-ColumnName -Actual $item.status -Expected $ColumnName) -and $item.content.type -eq "Issue") {
                $issues += [PSCustomObject]@{
                    ItemId = $item.id
                    IssueNumber = $item.content.number
                    Title = $item.content.title
                    Status = $item.status
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

function Move-IssueToColumn {
    param(
        [Parameter(Mandatory)]
        [int]$IssueNumber,
        
        [Parameter(Mandatory)]
        [string]$TargetColumn
    )
    
    try {
        Write-Host "   [MOVE] Deplacement issue #$IssueNumber vers '$TargetColumn'..." -ForegroundColor Cyan
        
        # Recuperer l'ID de l'item dans le project
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
            Write-Host "   [WARN] Issue #$IssueNumber non trouvee dans le project" -ForegroundColor Yellow
            return $false
        }
        
        # Verifier si deja dans la bonne colonne (case-insensitive)
        if (Compare-ColumnName -Actual $item.status -Expected $TargetColumn) {
            Write-Host "   [OK] Issue #$IssueNumber deja dans '$TargetColumn'" -ForegroundColor Green
            return $true
        }
        
        Write-Host "   [INFO] Colonne actuelle: '$($item.status)'" -ForegroundColor DarkGray
        
        # Essayer d'abord avec organization (plus courant pour les entreprises)
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
                Write-Host "   [INFO] Project trouve (organization)" -ForegroundColor DarkGray
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
                    Write-Host "   [INFO] Project trouve (user)" -ForegroundColor DarkGray
                }
            }
        }
        
        if (-not $projectId) {
            Write-Host "   [ERROR] Project non trouve pour $Owner" -ForegroundColor Red
            return $false
        }
        
        # Obtenir le field ID pour Status et ses options
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
            Write-Host "   [ERROR] Champ 'Status' non trouve dans le project" -ForegroundColor Red
            return $false
        }
        
        $statusFieldId = $fieldData.data.node.field.id
        $options = $fieldData.data.node.field.options
        
        Write-Host "   [INFO] Options disponibles: $($options.name -join ', ')" -ForegroundColor DarkGray
        
        # Trouver l'option cible (case-insensitive)
        $targetOption = $options | Where-Object { 
            Compare-ColumnName -Actual $_.name -Expected $TargetColumn 
        } | Select-Object -First 1
        
        if (-not $targetOption) {
            Write-Host "   [ERROR] Colonne '$TargetColumn' non trouvee parmi: $($options.name -join ', ')" -ForegroundColor Red
            return $false
        }
        
        Write-Host "   [INFO] Option trouvee: $($targetOption.name) (ID: $($targetOption.id))" -ForegroundColor DarkGray
        
        # Executer la mutation pour deplacer l'item
        $mutationResult = gh api graphql `
            -f query='mutation($projectId: ID!, $itemId: ID!, $fieldId: ID!, $optionId: String!) { updateProjectV2ItemFieldValue(input: { projectId: $projectId, itemId: $itemId, fieldId: $fieldId, value: { singleSelectOptionId: $optionId } }) { projectV2Item { id } } }' `
            -f projectId="$projectId" `
            -f itemId="$($item.id)" `
            -f fieldId="$statusFieldId" `
            -f optionId="$($targetOption.id)" 2>&1
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "   [ERROR] Echec de la mutation: $mutationResult" -ForegroundColor Red
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

function Get-CurrentIssueColumn {
    param([int]$IssueNumber)
    
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
# RECUPERATION DES ISSUES (CASE-INSENSITIVE)
# ============================================

# Note: Get-IssuesInColumnCaseInsensitive est definie plus haut

function Get-IssuesInColumn {
    param([string]$ColumnName)
    
    # Utiliser la version case-insensitive
    return Get-IssuesInColumnCaseInsensitive -ColumnName $ColumnName
}

# Fonction pour verifier si une issue est dans une colonne specifique
function Test-IssueInColumn {
    param(
        [int]$IssueNumber,
        [string]$ColumnName
    )
    
    $currentColumn = Get-CurrentIssueColumn -IssueNumber $IssueNumber
    if ($currentColumn) {
        return Compare-ColumnName -Actual $currentColumn -Expected $ColumnName
    }
    return $false
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
    
    # Construire le prompt sans code PowerShell complexe dans le here-string
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

## DEPLACEMENT DES CARTES - OBLIGATOIRE

**REGLE ABSOLUE:** Tu DOIS TOUJOURS deplacer l'issue a la fin de l'analyse.

### Deplacements:
- Si analyse VALIDE -> deplacer vers "Todo"
- Si analyse BLOQUEE -> deplacer vers "AnalyseBlock"

### Commande simple pour deplacer:
Utilise gh project item-edit ou la commande gh api graphql pour deplacer l'issue.
La comparaison de colonne est CASE-INSENSITIVE (a tester = A Tester).

## Instructions
1. Lis les fichiers d'agents:
   - .claude/agents/orchestrator.md
   - .claude/agents/backendadmin-analyzer.md
   - .claude/agents/frontendadmin-analyzer.md
   - .claude/agents/microservice-analyzer.md
   - .claude/agents/package-manager.md (gestion composants IDR)
   - .claude/agents/migration-manager.md

2. REGLES CRITIQUES:
   - COMPRENDRE le code existant avant modification
   - NE JAMAIS contredire la logique existante
   - NE JAMAIS inventer si information manquante -> BLOQUER
   - UTILISER les elements de IDR.Library.BuildingBlocks (CQRS, Auth, Validation)
   - UTILISER les composants de IDR.Library.Blazor (IdrForm, IdrInput, etc.)

3. REGLES COMPOSANTS FRONTEND:
   - Si un element se repete 3+ fois -> doit devenir un composant
   - Si composant reutilisable detecte -> creer issue dans le repo packages IDR
   - Apres mise a jour IDR.Library.Blazor -> remplacer composants locaux
   
4. REGLES PACKAGES IDR:
   - IDR.Library.BuildingBlocks: utiliser ses elements, creer issue UNIQUEMENT si erreur
   - IDR.Library.Blazor: utiliser ses composants, proposer nouveaux si manquants

5. Analyse:
   a. Determine le scope (BackendAdmin/FrontendAdmin/Microservice)
   b. Pour Frontend: detecter elements repetes -> suggerer composants
   c. Verifie coherence avec packages IDR
   d. Si entites modifiees: identifier migrations

6. Actions OBLIGATOIRES a la fin:
   - Si VALIDE: Genere Gherkin, commente, **DEPLACER vers Todo**
   - Si BLOQUEE: Commente raison, **DEPLACER vers AnalyseBlock**
   - Si COMPOSANT MANQUANT: Creer issue dans repo packages

7. **NE JAMAIS terminer sans avoir deplace l'issue!**

Variables:
- GITHUB_OWNER: $Owner
- GITHUB_REPO: $Repo
- PROJECT_NUMBER: $ProjectNumber
- GITHUB_OWNER_PACKAGE: $Owner_package
- GITHUB_REPO_PACKAGE: $Repo_package
- PROJECT_NUMBER_PACKAGE: $ProjectNumber_package

Commence l'analyse et N'OUBLIE PAS de deplacer l'issue a la fin.
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
1. Lis le fichier .claude/agents/coder.md pour le workflow complet

2. REGLES CRITIQUES:
   - LIRE et COMPRENDRE le code existant AVANT de modifier
   - NE JAMAIS contredire la logique existante
   - UTILISER IDR.Library.BuildingBlocks pour CQRS, Auth, Validation
   - UTILISER IDR.Library.Blazor pour composants UI
   - NE JAMAIS fermer l'issue (le testeur la fermera)

## WORKFLOW COMPLET A SUIVRE

### PHASE 0: PREPARATION (AVANT TOUTE ACTION)
1. Verifier s'il y a des modifications en cours: git status
2. Si modifications -> commit et push d'abord:
   git add .
   git commit -m "WIP: sauvegarde avant issue #$IssueNumber"
   git push
3. Retourner sur main et recuperer la derniere version:
   git checkout main
   git pull origin main

### PHASE 1: CREATION DE BRANCHE (TOUJOURS DEPUIS MAIN)
4. S'assurer d'etre sur main: git checkout main && git pull origin main
5. Creer la branche depuis main: git checkout -b feature/$IssueNumber-description

### PHASE 2: DEVELOPPEMENT
6. Lire l'analyse et les specs Gherkin dans les commentaires
7. Implementer le code
8. Si entites modifiees -> migration EF Core
9. Mettre a jour agent-docs/ si microservice modifie

### PHASE 3: TESTS (OBLIGATOIRE AVANT PR)
10. Executer TOUS les tests: dotnet test
11. SI TESTS ECHOUENT -> CORRIGER ET REESSAYER
    - Ne PAS continuer si des tests echouent
    - Corriger le code ou les tests
    - Relancer jusqu'a ce que TOUS les tests passent
12. Verifier la compilation: dotnet build

### PHASE 4: COMMIT ET PUSH (SEULEMENT SI TESTS OK)
13. Commit: git add . && git commit -m "feat(#$IssueNumber): description"
14. Push: git push -u origin feature/$IssueNumber-description

### PHASE 5: PULL REQUEST
15. DEPLACER l'issue vers "In Review"
16. Creer la PR: gh pr create --title "feat(#$IssueNumber): ..." --body "Closes #$IssueNumber"

### PHASE 6: MERGE ET FINALISATION
17. Merger la PR: gh pr merge --squash --delete-branch
18. Retourner sur main: git checkout main && git pull origin main
19. Supprimer la branche locale: git branch -d feature/$IssueNumber-description
20. Nettoyer: git fetch --prune
21. DEPLACER l'issue vers "A Tester"
22. NE PAS FERMER L'ISSUE - Ajouter un commentaire de confirmation

## DEPLACEMENTS DES CARTES - OBLIGATOIRE

| Etape | Action | Colonne |
|-------|--------|---------|
| Debut dev | Issue recue | In Progress (deja fait) |
| PR creee | Deplacer | In Review |
| Merge OK | Deplacer | A Tester |
| JAMAIS | NE PAS fermer | - |

## COMMANDES GH POUR DEPLACER
Utiliser gh api graphql avec les variables pour deplacer les issues.
La comparaison est CASE-INSENSITIVE (a tester = A Tester).

Variables:
- GITHUB_OWNER: $Owner
- GITHUB_REPO: $Repo
- PROJECT_NUMBER: $ProjectNumber

Commence l'implementation en suivant EXACTEMENT ce workflow.
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
# AGENT POUR TERMINER LE MERGE (issues en "In Review")
# ============================================

function Invoke-FinishMergeAgent {
    param(
        [int]$IssueNumber,
        [string]$Title
    )
    
    $issueJson = gh issue view $IssueNumber --repo "$Owner/$Repo" --json number,title,body,labels,comments
    
    $promptFile = Join-Path $env:TEMP "merge-prompt-$IssueNumber.txt"
    
    $promptContent = @"
Tu es l'agent de finalisation pour le projet DashBoardAdmin.

## Issue a finaliser
$issueJson

## Contexte
Cette issue est en colonne "In Review", ce qui signifie qu'une PR a ete creee.
Tu dois terminer le processus de merge.

## Instructions

1. **Verifier l'etat de la PR:**
   ``````powershell
   # Lister les PRs liees a cette issue
   gh pr list --repo "$Owner/$Repo" --search "in:title #$IssueNumber OR in:body #$IssueNumber"
   ``````

2. **Si PR existe et approuvee:**
   ``````powershell
   # Merger la PR
   gh pr merge PR_NUMBER --repo "$Owner/$Repo" --squash --delete-branch
   ``````

3. **Si PR deja mergee:**
   - Verifier que la branche est supprimee
   - Deplacer vers "A Tester"

4. **Supprimer la branche (OBLIGATOIRE):**
   ``````powershell
   git checkout main
   git pull origin main
   git branch -d feature/$IssueNumber-xxx        # Local
   git push origin --delete feature/$IssueNumber-xxx  # Remote
   git fetch --prune
   ``````

5. **Deplacer vers "A Tester" (OBLIGATOIRE):**
   - L'issue doit etre deplacee vers "A Tester"
   - NE PAS fermer l'issue (le testeur la fermera)

6. **Ajouter un commentaire:**
   ``````powershell
   gh issue comment $IssueNumber --repo "$Owner/$Repo" --body "✅ PR mergee, branche supprimee. Issue prete pour test."
   ``````

## DEPLACEMENT OBLIGATOIRE
A la fin, tu DOIS deplacer l'issue vers "A Tester".
NE JAMAIS fermer l'issue.

Variables:
- GITHUB_OWNER: $Owner
- GITHUB_REPO: $Repo
- PROJECT_NUMBER: $ProjectNumber

Termine le merge et deplace l'issue vers "A Tester".
"@

    $promptContent | Out-File $promptFile -Encoding utf8
    
    Write-Host "     [MERGE] Finalisation avec $Model..." -ForegroundColor Yellow
    
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
# BOUCLE PRINCIPALE AVEC PRIORITE DE TRAITEMENT
# ============================================

Write-Host ""
Write-Host "[START] Agents autonomes actifs" -ForegroundColor Green
Write-Host "   Appuyez sur Ctrl+C pour arreter"
Write-Host ""
Write-Host "[INFO] PRIORITE DE TRAITEMENT:" -ForegroundColor Yellow
Write-Host "   1. Issues 'In Review' -> Terminer le merge" -ForegroundColor Yellow
Write-Host "   2. Issues 'In Progress' -> Terminer le developpement" -ForegroundColor Yellow
Write-Host "   3. Issues 'Analyse' -> Nouvelles analyses" -ForegroundColor Yellow
Write-Host "   4. Issues 'Todo' -> Nouvelles implementations" -ForegroundColor Yellow
Write-Host ""

$iteration = 0

while ($true) {
    $iteration++
    $timestamp = Get-Date -Format "HH:mm:ss"
    
    Write-Host "[$timestamp] ====================================================" -ForegroundColor DarkCyan
    Write-Host "[$timestamp] [CHECK] Iteration #$iteration" -ForegroundColor DarkCyan
    
    try {
        # ============================================
        # PRIORITE 1: ISSUES EN "IN REVIEW" (Terminer le merge)
        # ============================================
        if (-not $AnalysisOnly) {
            Write-Host "[$timestamp] [PRIORITE 1] Verification colonne 'In Review'..." -ForegroundColor Magenta
            
            $reviewIssues = @(Get-IssuesInColumn -ColumnName $Columns.Review)
            
            if ($reviewIssues.Count -gt 0) {
                Write-Host "[$timestamp] [REVIEW] $($reviewIssues.Count) issue(s) en attente de merge" -ForegroundColor Yellow
                
                foreach ($issue in $reviewIssues) {
                    Write-Host "[$timestamp]   -> #$($issue.IssueNumber): $($issue.Title)" -ForegroundColor White
                    Write-Host "[$timestamp]   [ACTION] Terminer le merge et deplacer vers 'A Tester'..." -ForegroundColor Cyan
                    
                    # Appeler l'agent pour terminer le merge
                    $success = Invoke-FinishMergeAgent -IssueNumber $issue.IssueNumber -Title $issue.Title
                    
                    if ($success) {
                        # Deplacer vers "A Tester" - NE PAS FERMER L'ISSUE
                        $moved = Move-IssueToColumn -IssueNumber $issue.IssueNumber -TargetColumn $Columns.ATester
                        if ($moved) {
                            Write-Host "[$timestamp]   [OK] #$($issue.IssueNumber) -> 'A Tester'" -ForegroundColor Green
                        }
                    }
                    else {
                        Write-Host "[$timestamp]   [WARN] Merge non termine pour #$($issue.IssueNumber)" -ForegroundColor Yellow
                    }
                }
                
                # Continuer a la prochaine iteration apres avoir traite les reviews
                Write-Host "[$timestamp] [WAIT] Prochaine verification dans $PollingInterval sec..." -ForegroundColor DarkGray
                Start-Sleep -Seconds $PollingInterval
                continue
            }
            else {
                Write-Host "[$timestamp] [REVIEW] Aucune issue en review" -ForegroundColor DarkGray
            }
        }
        
        # ============================================
        # PRIORITE 2: ISSUES EN "IN PROGRESS" (Terminer le developpement)
        # ============================================
        if (-not $AnalysisOnly) {
            Write-Host "[$timestamp] [PRIORITE 2] Verification colonne 'In Progress'..." -ForegroundColor Magenta
            
            $inProgressIssues = @(Get-IssuesInColumn -ColumnName $Columns.InProgress)
            
            if ($inProgressIssues.Count -gt 0) {
                Write-Host "[$timestamp] [IN PROGRESS] $($inProgressIssues.Count) issue(s) en cours de developpement" -ForegroundColor Yellow
                
                $issue = $inProgressIssues[0]  # Traiter une seule issue a la fois
                Write-Host "[$timestamp]   -> #$($issue.IssueNumber): $($issue.Title)" -ForegroundColor White
                Write-Host "[$timestamp]   [ACTION] Continuer/Terminer le developpement..." -ForegroundColor Cyan
                
                # Appeler l'agent codeur pour continuer
                $success = Invoke-CoderAgent -IssueNumber $issue.IssueNumber -Title $issue.Title
                
                if ($success) {
                    # Verifier si l'issue est toujours en In Progress ou a ete deplacee
                    $currentColumn = Get-CurrentIssueColumn -IssueNumber $issue.IssueNumber
                    Write-Host "[$timestamp]   [INFO] Colonne actuelle: $currentColumn" -ForegroundColor DarkGray
                    
                    # Si toujours en In Progress apres le traitement, forcer le deplacement vers In Review
                    if (Compare-ColumnName -Actual $currentColumn -Expected $Columns.InProgress) {
                        Write-Host "[$timestamp]   [WARN] Issue toujours en 'In Progress', verification requise" -ForegroundColor Yellow
                    }
                }
                else {
                    Write-Host "[$timestamp]   [ERREUR] Echec du developpement pour #$($issue.IssueNumber)" -ForegroundColor Red
                }
                
                # Continuer a la prochaine iteration
                Write-Host "[$timestamp] [WAIT] Prochaine verification dans $PollingInterval sec..." -ForegroundColor DarkGray
                Start-Sleep -Seconds $PollingInterval
                continue
            }
            else {
                Write-Host "[$timestamp] [IN PROGRESS] Aucune issue en cours" -ForegroundColor DarkGray
            }
        }
        
        # ============================================
        # PRIORITE 3: AGENT D'ANALYSE (Nouvelles analyses)
        # ============================================
        if (-not $CoderOnly) {
            Write-Host "[$timestamp] [PRIORITE 3] Verification colonne 'Analyse'..." -ForegroundColor Blue
            
            $analysisIssues = @(Get-IssuesInColumn -ColumnName $Columns.Analyse)
            $processedAnalysis = @(Get-ProcessedIssues -FilePath $processedAnalysisFile)
            $newAnalysisIssues = @($analysisIssues | Where-Object { $_.IssueNumber -notin $processedAnalysis })
            
            if ($newAnalysisIssues.Count -eq 0) {
                Write-Host "[$timestamp] [ANALYSE] Aucune nouvelle issue a analyser" -ForegroundColor DarkGray
            }
            else {
                Write-Host "[$timestamp] [ANALYSE] $($newAnalysisIssues.Count) issue(s) a analyser" -ForegroundColor Green
                
                foreach ($issue in $newAnalysisIssues) {
                    Write-Host "[$timestamp]   -> #$($issue.IssueNumber): $($issue.Title)" -ForegroundColor White
                    
                    $success = Invoke-AnalysisAgent -IssueNumber $issue.IssueNumber -Title $issue.Title
                    
                    if ($success) {
                        Add-ProcessedIssue -FilePath $processedAnalysisFile -IssueNumber $issue.IssueNumber
                        
                        # Verifier le deplacement (doit etre fait par l'agent)
                        $currentColumn = Get-CurrentIssueColumn -IssueNumber $issue.IssueNumber
                        
                        # Si toujours dans Analyse, forcer le deplacement vers Todo ou AnalyseBlock
                        if (Compare-ColumnName -Actual $currentColumn -Expected $Columns.Analyse) {
                            Write-Host "[$timestamp]   [WARN] Issue pas deplacee par l'agent, deplacement force vers 'Todo'" -ForegroundColor Yellow
                            Move-IssueToColumn -IssueNumber $issue.IssueNumber -TargetColumn $Columns.Todo
                        }
                        
                        Write-Host "[$timestamp]   [OK] #$($issue.IssueNumber) analysee" -ForegroundColor Green
                    }
                    else {
                        Write-Host "[$timestamp]   [ERREUR] #$($issue.IssueNumber) - Deplacement vers 'AnalyseBlock'" -ForegroundColor Red
                        Move-IssueToColumn -IssueNumber $issue.IssueNumber -TargetColumn $Columns.AnalyseBlock
                    }
                }
            }
        }
        
        # ============================================
        # PRIORITE 4: AGENT CODEUR (Nouvelles implementations)
        # ============================================
        if (-not $AnalysisOnly) {
            Write-Host "[$timestamp] [PRIORITE 4] Verification colonne 'Todo'..." -ForegroundColor Magenta
            
            $todoIssues = @(Get-IssuesInColumn -ColumnName $Columns.Todo)
            $processedCoder = @(Get-ProcessedIssues -FilePath $processedCoderFile)
            $newTodoIssues = @($todoIssues | Where-Object { $_.IssueNumber -notin $processedCoder })
            
            if ($newTodoIssues.Count -eq 0) {
                Write-Host "[$timestamp] [CODER] Aucune nouvelle issue a implementer" -ForegroundColor DarkGray
            }
            else {
                Write-Host "[$timestamp] [CODER] $($newTodoIssues.Count) issue(s) a implementer" -ForegroundColor Green
                
                $issue = $newTodoIssues[0]  # Traiter une seule issue a la fois
                Write-Host "[$timestamp]   -> #$($issue.IssueNumber): $($issue.Title)" -ForegroundColor White
                
                # DEPLACER IMMEDIATEMENT vers "In Progress" AVANT de commencer
                Write-Host "[$timestamp]   [MOVE] Deplacement vers 'In Progress'..." -ForegroundColor Cyan
                $moved = Move-IssueToColumn -IssueNumber $issue.IssueNumber -TargetColumn $Columns.InProgress
                
                if (-not $moved) {
                    Write-Host "[$timestamp]   [ERREUR] Impossible de deplacer vers 'In Progress'" -ForegroundColor Red
                    continue
                }
                
                # Lancer le developpement
                $success = Invoke-CoderAgent -IssueNumber $issue.IssueNumber -Title $issue.Title
                
                if ($success) {
                    Add-ProcessedIssue -FilePath $processedCoderFile -IssueNumber $issue.IssueNumber
                    Write-Host "[$timestamp]   [OK] #$($issue.IssueNumber) implementation lancee" -ForegroundColor Green
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


