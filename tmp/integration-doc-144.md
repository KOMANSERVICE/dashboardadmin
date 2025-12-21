# Integration: Bug FrontendAdmin - Ordonner les menus #141

## Service: DashBoardAdmin

**Issue:** #144 - fix(#144): Restore ResponsiveDataTable design and fix drag-drop for menu ordering
**PR:** #145
**Date:** 2025-12-19

---

## Resume des modifications

Cette fonctionnalite ajoute la possibilite de reordonner les menus dans le FrontendAdmin via un systeme de drag-and-drop. Les modifications incluent:

- Ajout du champ `SortOrder` a l'entite `Menu`
- Nouvel endpoint `PATCH /menu/reorder` pour persister l'ordre des menus
- Interface drag-and-drop dans le composant `ListeMenu.razor`

---

## Architecture

```
FrontendAdmin (Blazor)
    |
    v
BackendAdmin.Api (BFF / Gateway)
    |
    v
MenuService.Api (Microservice)
    |
    v
MenuService.Infrastructure (PostgreSQL)
```

---

## Endpoints

### MenuService.Api (Microservice)

| Methode | Route | Description | Scope requis | Request Body | Response |
|---------|-------|-------------|--------------|--------------|----------|
| PATCH | `/menu/reorder` | Reordonne plusieurs menus en mettant a jour leur SortOrder | `menu:write` | `ReorderMenusRequest` | `BaseResponse<ReorderMenusResponse>` |

### BackendAdmin.Api (BFF)

| Methode | Route | Description | Auth | Request Body | Response |
|---------|-------|-------------|------|--------------|----------|
| PATCH | `/menu/{appAdminReference}/reorder` | Reordonne les menus d'une application | Bearer Token | `ReorderMenusApiRequest` | `BaseResponse<ReorderMenusApiResponse>` |

---

## DTOs

### MenuService (Microservice)

#### ReorderMenusRequest
```csharp
public record ReorderMenusRequest(
    string AppAdminReference,
    List<MenuSortOrderItem> Items
);
```

#### MenuSortOrderItem
```csharp
public record MenuSortOrderItem(
    string Reference,    // Reference unique du menu
    int SortOrder        // Nouvel ordre de tri (0-indexed)
);
```

#### ReorderMenusResponse
```csharp
public record ReorderMenusResponse(
    bool Success,        // Indique si l'operation a reussi
    int UpdatedCount     // Nombre de menus mis a jour
);
```

### BackendAdmin (BFF)

#### ReorderMenusApiRequest
```csharp
public record ReorderMenusApiRequest(
    List<MenuSortOrderItem> Items
);
```

#### ReorderMenusApiResponse
```csharp
public record ReorderMenusApiResponse(
    bool Success,
    int UpdatedCount
);
```

#### MenuInfoDTO (mise a jour)
```csharp
public record MenuInfoDTO
{
    public string Name { get; set; }
    public string Reference { get; set; }
    public string UrlFront { get; set; }
    public string Icon { get; set; }
    public string AppAdminReference { get; set; }
    public bool IsActif { get; set; }
    public string? Group { get; set; }
    public int SortOrder { get; set; }  // NOUVEAU
}
```

### FrontendAdmin (Client Blazor)

#### ReorderMenusRequest
```csharp
public record ReorderMenusRequest(
    List<MenuSortOrderItem> Items
);
```

#### MenuSortOrderItem
```csharp
public record MenuSortOrderItem(
    string Reference,
    int SortOrder
);
```

#### ReorderMenusResponse
```csharp
public record ReorderMenusResponse(
    bool Success,
    int UpdatedCount
);
```

---

## Exemples de Payloads JSON

### Request PATCH /menu/{appAdminReference}/reorder (BackendAdmin)

```json
{
    "items": [
        {
            "reference": "menu-dashboard",
            "sortOrder": 0
        },
        {
            "reference": "menu-users",
            "sortOrder": 1
        },
        {
            "reference": "menu-settings",
            "sortOrder": 2
        },
        {
            "reference": "menu-reports",
            "sortOrder": 3
        }
    ]
}
```

### Request PATCH /menu/reorder (MenuService)

```json
{
    "appAdminReference": "app-admin-001",
    "items": [
        {
            "reference": "menu-dashboard",
            "sortOrder": 0
        },
        {
            "reference": "menu-users",
            "sortOrder": 1
        },
        {
            "reference": "menu-settings",
            "sortOrder": 2
        }
    ]
}
```

### Response Success (200 OK)

```json
{
    "success": true,
    "statusCode": 200,
    "message": "Menus reordonnes avec succes",
    "data": {
        "success": true,
        "updatedCount": 3
    }
}
```

### Response Error - Menu not found (404)

```json
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
    "title": "Not Found",
    "status": 404,
    "detail": "Menus not found: menu-invalid-ref"
}
```

### Response Error - Validation (400)

```json
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "Bad Request",
    "status": 400,
    "errors": {
        "AppAdminReference": ["AppAdminReference is required."],
        "Items": ["Items list cannot be empty."]
    }
}
```

---

## Commands/Queries

### MenuService

| Type | Nom | Request | Response | Description |
|------|-----|---------|----------|-------------|
| Command | `ReorderMenusCommand` | `ReorderMenusCommand` | `ReorderMenusResult` | Met a jour le SortOrder de plusieurs menus |

#### ReorderMenusCommand
```csharp
public record ReorderMenusCommand(
    string AppAdminReference,
    List<MenuSortOrderItem> Items
) : ICommand<ReorderMenusResult>;
```

#### ReorderMenusResult
```csharp
public record ReorderMenusResult(
    bool Success,
    int UpdatedCount
);
```

### BackendAdmin

| Type | Nom | Request | Response | Description |
|------|-----|---------|----------|-------------|
| Command | `ReorderMenusCommand` | `ReorderMenusCommand` | `ReorderMenusResult` | Delegue au MenuService via Refit |

---

## Validation (FluentValidation)

### ReorderMenusValidator (MenuService)

```csharp
public class ReorderMenusValidator : AbstractValidator<ReorderMenusCommand>
{
    public ReorderMenusValidator()
    {
        RuleFor(x => x.AppAdminReference)
            .NotEmpty()
            .WithMessage("AppAdminReference is required.");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Items list cannot be empty.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Reference)
                .NotEmpty()
                .WithMessage("Menu reference is required.");

            item.RuleFor(i => i.SortOrder)
                .GreaterThanOrEqualTo(0)
                .WithMessage("SortOrder must be greater than or equal to 0.");
        });
    }
}
```

---

## Entite modifiee

### Menu (MenuService.Domain)

```csharp
[Table("TM00001")]
public class Menu : Entity<Guid>
{
    [Column("cf1")]
    public string Name { get; set; } = default!;

    [Column("cf2")]
    public string Reference { get; set; } = default!;

    [Column("cf3")]
    public string UrlFront { get; set; } = default!;

    [Column("cf4")]
    public string Icon { get; set; } = default!;

    [Column("cf5")]
    public bool IsActif { get; set; }

    [Column("cf6")]
    public string AppAdminReference { get; set; } = default!;

    [Column("cf7")]
    public string? Group { get; set; }

    [Column("cf8")]
    public int SortOrder { get; set; }  // NOUVEAU CHAMP
}
```

---

## Migration de base de donnees

### AddSortOrderToMenu (20251219001717)

```csharp
public partial class AddSortOrderToMenu : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "cf8",
            table: "TM00001",
            type: "integer",
            nullable: false,
            defaultValue: 0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "cf8",
            table: "TM00001");
    }
}
```

---

## Interface HTTP Service (Refit)

### IMenuService (BackendAdmin -> MenuService)

```csharp
public interface IMenuService
{
    [Patch("/menu/reorder")]
    Task<BaseResponse<ReorderMenusResponse>> ReorderMenusAsync(ReorderMenusRequest request);
}
```

### IMenuHttpService (FrontendAdmin -> BackendAdmin)

```csharp
public interface IMenuHttpService
{
    [Patch("/menu/{appAdminReference}/reorder")]
    Task<BaseResponse<ReorderMenusResponse>> ReorderMenusAsync(
        string appAdminReference,
        ReorderMenusRequest request
    );
}
```

---

## Frontend - Composant Blazor

### ListeMenu.razor

**Route:** `/menu/{AppAdminReference}`

**Fonctionnalites ajoutees:**
- Drag-and-drop natif HTML5 pour reordonner les lignes
- Affichage du numero d'ordre (SortOrder + 1)
- Indicateur visuel pendant le drag
- Sauvegarde automatique apres chaque reordonnancement
- Loader pendant la sauvegarde
- Toast de confirmation/erreur

**Evenements geres:**
- `@ondragstart` - Debut du drag
- `@ondragover` - Survol d'une cible
- `@ondrop` - Depot sur une cible
- `@ondragend` - Fin du drag

---

## Securite

### Scopes requis

| Endpoint | Scope |
|----------|-------|
| `PATCH /menu/reorder` (MenuService) | `menu:write` |
| `PATCH /menu/{appAdminReference}/reorder` (BackendAdmin) | Bearer Token (Authorization) |

---

## Notes d'integration

### Prerequis
1. La migration `AddSortOrderToMenu` doit etre appliquee avant le deploiement
2. Les menus existants auront `SortOrder = 0` par defaut

### Points d'attention
- Les references de menu doivent etre uniques par `AppAdminReference`
- Si un menu reference dans la requete n'existe pas, une erreur 404 est retournee avec les references manquantes
- Le `SortOrder` est 0-indexed (premier element = 0)
- L'ordre est persiste immediatement apres chaque drag-and-drop

### Dependances
- **Mapster** : Mapping entre DTOs
- **MediatR** : Pattern CQRS
- **FluentValidation** : Validation des commandes
- **Refit** : Client HTTP type-safe
- **Entity Framework Core** : ORM

### Configuration requise
Aucune configuration supplementaire requise. L'endpoint utilise l'infrastructure existante.

---

## Fichiers modifies

| Fichier | Type de modification |
|---------|---------------------|
| `MenuService.Domain/Models/Menu.cs` | Ajout propriete `SortOrder` |
| `MenuService.Application/Features/Menus/DTOs/MenuDTO.cs` | Ajout propriete `SortOrder` |
| `MenuService.Application/Features/Menus/Commands/ReorderMenus/*` | Nouveau (Command, Handler, Validator) |
| `MenuService.Api/Endpoints/Menus/ReorderMenus.cs` | Nouveau endpoint |
| `MenuService.Infrastructure/Data/Migrations/*` | Migration `AddSortOrderToMenu` |
| `BackendAdmin.Application/Features/Menus/Commands/ReorderMenus/*` | Nouveau (Command, Handler) |
| `BackendAdmin.Application/Features/Menus/DTOs/MenuDTO.cs` | Ajout DTOs reorder |
| `BackendAdmin.Application/ApiExterne/Menus/IMenuService.cs` | Ajout methode `ReorderMenusAsync` |
| `BackendAdmin.Api/Endpoints/Menus/ReorderMenus.cs` | Nouveau endpoint |
| `FrontendAdmin.Shared/Pages/Menus/ListeMenu.razor` | Ajout drag-and-drop UI |
| `FrontendAdmin.Shared/Pages/Menus/Models/Menu.cs` | Ajout DTOs et `SortOrder` |
| `FrontendAdmin.Shared/Services/Https/IMenuHttpService.cs` | Ajout methode `ReorderMenusAsync` |
| `FrontendAdmin.Shared/wwwroot/css/dragdrop.css` | Styles pour drag-and-drop |

---

## Commits lies

| Hash | Message |
|------|---------|
| `931f0a5` | fix(#144): Restore ResponsiveDataTable design and fix drag-drop for menu ordering (#145) |
| `b681b01` | feat(#141): Add drag-and-drop menu ordering in FrontendAdmin (#143) |
| `778abc7` | feat(#140): Add SortOrder field to Menu for consistent ordering (#142) |
