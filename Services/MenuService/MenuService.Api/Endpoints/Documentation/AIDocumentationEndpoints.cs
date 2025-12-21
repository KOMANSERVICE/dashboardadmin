using Carter;
using MenuService.Api.Endpoints.Documentation.Models;
using MenuService.Application.Features.Menus.Commands.ActiveMenu;
using MenuService.Application.Features.Menus.Commands.CreateMenu;
using MenuService.Application.Features.Menus.Commands.InactiveMenu;
using MenuService.Application.Features.Menus.Commands.UpdateMenu;
using MenuService.Application.Features.Menus.Queries.GetAllActifMenu;
using MenuService.Application.Features.Menus.Queries.GetAllMenu;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MenuService.Api.Endpoints.Documentation;

public class AIDocumentationEndpoints : ICarterModule
{
    private const string ServiceName = "MenuService";
    private const string ServiceVersion = "1.0.0";
    private const string ServiceDescription = "Service de gestion des menus avec hiérarchie et gestion d'état";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/docs/ai")
            .WithTags("AI Documentation")
            .WithDescription("Documentation exploitable par AI")
            .WithOpenApi();

        // Common endpoints (same as MagasinService)
        group.MapGet("/manifest", GetManifest)
            .WithName("GetAIManifest")
            .WithSummary("Récupère le manifest de documentation AI")
            .Produces<AIManifest>(StatusCodes.Status200OK)
            .CacheOutput(policy => policy.Expire(TimeSpan.FromHours(1)));

        group.MapGet("/", GetFullDocumentation)
            .WithName("GetAIDocumentation")
            .WithSummary("Récupère la documentation complète")
            .Produces<AIDocumentation>(StatusCodes.Status200OK)
            .CacheOutput(policy => policy.Expire(TimeSpan.FromHours(1)));

        group.MapGet("/endpoints/{name}", GetEndpointDetails)
            .WithName("GetAIEndpointDetails")
            .WithSummary("Récupère les détails d'un endpoint")
            .Produces<EndpointDocumentation>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/models/{name}", GetModelSchema)
            .WithName("GetAIModelSchema")
            .WithSummary("Récupère le schéma d'un modèle")
            .Produces<ModelSchema>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/features", GetFeatures)
            .WithName("GetAIFeatures")
            .WithSummary("Récupère la liste des features CQRS")
            .Produces<FeaturesDocumentation>(StatusCodes.Status200OK);

        group.MapGet("/versions", GetVersions)
            .WithName("GetAIVersions")
            .WithSummary("Récupère les versions disponibles")
            .Produces<VersionsDocumentation>(StatusCodes.Status200OK);

        group.MapGet("/search", SearchDocumentation)
            .WithName("SearchAIDocumentation")
            .WithSummary("Recherche dans la documentation")
            .Produces<SearchResults>(StatusCodes.Status200OK);

        group.MapGet("/openapi-mapping", GetOpenApiMapping)
            .WithName("GetOpenApiMapping")
            .WithSummary("Récupère le mapping avec OpenAPI")
            .Produces<OpenApiMapping>(StatusCodes.Status200OK);

        group.MapGet("/health", GetHealth)
            .WithName("GetAIDocumentationHealth")
            .WithSummary("Vérifie la santé du service de documentation")
            .Produces<HealthStatus>(StatusCodes.Status200OK);

        // MenuService specific endpoints
        group.MapGet("/security", GetSecurityDocumentation)
            .WithName("GetSecurityDocumentation")
            .WithSummary("Documentation des aspects sécurité")
            .Produces<SecurityDocumentation>(StatusCodes.Status200OK);

        group.MapGet("/grpc", GetGrpcDocumentation)
            .WithName("GetGrpcDocumentation")
            .WithSummary("Documentation du service gRPC")
            .Produces<GrpcDocumentation>(StatusCodes.Status200OK);

        group.MapGet("/features/hierarchy", GetHierarchyDocumentation)
            .WithName("GetHierarchyDocumentation")
            .WithSummary("Documentation de la gestion de hiérarchie")
            .Produces<HierarchyDocumentation>(StatusCodes.Status200OK);

        group.MapGet("/features/multi-app", GetMultiAppDocumentation)
            .WithName("GetMultiAppDocumentation")
            .WithSummary("Documentation du support multi-applications")
            .Produces<MultiAppDocumentation>(StatusCodes.Status200OK);

        group.MapGet("/features/state-management", GetStateManagementDocumentation)
            .WithName("GetStateManagementDocumentation")
            .WithSummary("Documentation de la gestion d'état")
            .Produces<StateManagementDocumentation>(StatusCodes.Status200OK);

        group.MapGet("/examples", GetExamples)
            .WithName("GetExamples")
            .WithSummary("Exemples d'utilisation")
            .Produces<ExamplesDocumentation>(StatusCodes.Status200OK);

        group.MapGet("/migration", GetMigrationGuide)
            .WithName("GetMigrationGuide")
            .WithSummary("Guide de migration")
            .Produces<MigrationGuide>(StatusCodes.Status200OK);

        group.MapGet("/performance", GetPerformanceHints)
            .WithName("GetPerformanceHints")
            .WithSummary("Conseils de performance")
            .Produces<PerformanceHints>(StatusCodes.Status200OK);

        group.MapGet("/integration", GetIntegrationPatterns)
            .WithName("GetIntegrationPatterns")
            .WithSummary("Patterns d'intégration")
            .Produces<IntegrationPatterns>(StatusCodes.Status200OK);

        group.MapGet("/troubleshooting", GetTroubleshooting)
            .WithName("GetTroubleshooting")
            .WithSummary("Guide de dépannage")
            .Produces<TroubleshootingGuide>(StatusCodes.Status200OK);
    }

    private static Ok<AIManifest> GetManifest()
    {
        var manifest = new AIManifest
        {
            Name = ServiceName,
            Version = ServiceVersion,
            Description = ServiceDescription,
            Endpoints = new[]
            {
                "/api/menu/{appAdminReference} (GET) - Récupère tous les menus d'une application",
                "/api/menu/{appAdminReference}/actif (GET) - Récupère uniquement les menus actifs",
                "/api/menu (POST) - Crée un nouveau menu",
                "/api/menu (PUT) - Met à jour un menu existant",
                "/api/menu/active (PATCH) - Active un menu",
                "/api/menu/inactive (PATCH) - Désactive un menu",
                "/api/docs/ai/manifest (GET) - Manifest de documentation",
                "/api/docs/ai (GET) - Documentation complète",
                "/api/docs/ai/endpoints/{name} (GET) - Détails d'un endpoint",
                "/api/docs/ai/models/{name} (GET) - Schéma d'un modèle",
                "/api/docs/ai/features (GET) - Features CQRS",
                "/api/docs/ai/security (GET) - Documentation sécurité",
                "/api/docs/ai/grpc (GET) - Documentation gRPC",
                "/api/docs/ai/features/hierarchy (GET) - Gestion de hiérarchie",
                "/api/docs/ai/features/multi-app (GET) - Support multi-applications",
                "/api/docs/ai/features/state-management (GET) - Gestion d'état",
                "/api/docs/ai/examples (GET) - Exemples d'utilisation",
                "/api/docs/ai/migration (GET) - Guide de migration",
                "/api/docs/ai/performance (GET) - Conseils de performance",
                "/api/docs/ai/integration (GET) - Patterns d'intégration",
                "/api/docs/ai/troubleshooting (GET) - Guide de dépannage"
            },
            Documentation = "/api/docs/ai"
        };

        return TypedResults.Ok(manifest);
    }

    private static Ok<AIDocumentation> GetFullDocumentation()
    {
        var documentation = new AIDocumentation
        {
            Service = new ServiceInfo
            {
                Name = ServiceName,
                Version = ServiceVersion,
                Description = ServiceDescription,
                BaseUrl = "/api",
                Protocol = "HTTPS, gRPC",
                ContentType = "application/json",
                Features = new[]
                {
                    "Hiérarchie de menus",
                    "Gestion d'état (actif/inactif)",
                    "Support multi-applications",
                    "Interface duale REST + gRPC"
                }
            },
            Architecture = new ArchitectureInfo
            {
                Pattern = "Clean Architecture with Vertical Slice",
                Layers = new[]
                {
                    "API (Carter endpoints + gRPC service)",
                    "Application (CQRS handlers with FluentValidation)",
                    "Domain (Entities and Models)",
                    "Infrastructure (EF Core, PostgreSQL, Vault)"
                },
                Patterns = new[]
                {
                    "CQRS (Command Query Responsibility Segregation)",
                    "Repository Pattern with Generic Implementation",
                    "Unit of Work",
                    "State Management Pattern",
                    "Multi-tenancy by Application Reference"
                }
            },
            Endpoints = GetEndpointsList(),
            Models = GetModelsList(),
            Features = GetFeaturesList(),
            Authentication = new AuthenticationInfo
            {
                Type = "Bearer JWT",
                Required = true,
                HeaderName = "Authorization",
                HeaderFormat = "Bearer {token}",
                AdditionalInfo = "CORS configuration via Vault"
            },
            Errors = GetErrorCodes()
        };

        return TypedResults.Ok(documentation);
    }

    private static Results<Ok<EndpointDocumentation>, NotFound<ProblemDetails>> GetEndpointDetails(string name)
    {
        var endpoints = GetEndpointsDictionary();

        if (!endpoints.TryGetValue(name.ToLower(), out var endpoint))
        {
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Documentation not found",
                Detail = $"Documentation not found for endpoint: {name}"
            };
            return TypedResults.NotFound(problem);
        }

        return TypedResults.Ok(endpoint);
    }

    private static Results<Ok<ModelSchema>, NotFound<ProblemDetails>> GetModelSchema(string name)
    {
        var schemas = GetModelSchemas();

        if (!schemas.TryGetValue(name.ToLower(), out var schema))
        {
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Model not found",
                Detail = $"Schema not found for model: {name}"
            };
            return TypedResults.NotFound(problem);
        }

        return TypedResults.Ok(schema);
    }

    private static Ok<FeaturesDocumentation> GetFeatures()
    {
        var features = new FeaturesDocumentation
        {
            Commands = new[]
            {
                new FeatureInfo
                {
                    Type = "Command",
                    Name = "CreateMenuCommand",
                    Description = "Création d'un nouveau menu",
                    Parameters = new[] { "MenuDTO" },
                    Response = "CreateMenuResult",
                    Validation = new[]
                    {
                        "Name: Required, MaxLength(100)",
                        "Reference: Required, MaxLength(50), LowercaseAlphabetic",
                        "UrlFront: Required, MaxLength(200)",
                        "Icon: Optional, MaxLength(100)",
                        "AppAdminReference: Required, MaxLength(50)"
                    }
                },
                new FeatureInfo
                {
                    Type = "Command",
                    Name = "UpdateMenuCommand",
                    Description = "Mise à jour d'un menu existant",
                    Parameters = new[] { "MenuDTO" },
                    Response = "UpdateMenuResult"
                },
                new FeatureInfo
                {
                    Type = "Command",
                    Name = "ActiveMenuCommand",
                    Description = "Activation d'un menu",
                    Parameters = new[] { "MenuStateDto" },
                    Response = "ActiveMenuResult"
                },
                new FeatureInfo
                {
                    Type = "Command",
                    Name = "InactiveMenuCommand",
                    Description = "Désactivation d'un menu",
                    Parameters = new[] { "MenuStateDto" },
                    Response = "InactiveMenuResult"
                }
            },
            Queries = new[]
            {
                new FeatureInfo
                {
                    Type = "Query",
                    Name = "GetAllMenuQuery",
                    Description = "Récupération de tous les menus d'une application",
                    Parameters = new[] { "AppAdminReference" },
                    Response = "List<MenuStateDto>"
                },
                new FeatureInfo
                {
                    Type = "Query",
                    Name = "GetAllActifMenuQuery",
                    Description = "Récupération des menus actifs uniquement",
                    Parameters = new[] { "AppAdminReference" },
                    Response = "List<MenuStateDto>"
                }
            }
        };

        return TypedResults.Ok(features);
    }

    private static Ok<SecurityDocumentation> GetSecurityDocumentation()
    {
        var security = new SecurityDocumentation
        {
            Authentication = new SecurityAuthentication
            {
                Type = "JWT Bearer",
                TokenEndpoint = "/api/auth/token",
                RefreshEndpoint = "/api/auth/refresh",
                TokenLifetime = "15 minutes",
                RefreshTokenLifetime = "7 days"
            },
            Cors = new CorsConfiguration
            {
                Provider = "ISecureSecretProvider via Vault",
                ConfigPath = "secret/cors/menuservice",
                AllowedOrigins = new[] { "Configured in Vault" },
                AllowedMethods = new[] { "GET", "POST", "PUT", "PATCH", "DELETE" },
                AllowedHeaders = new[] { "Authorization", "Content-Type" }
            },
            Permissions = new[]
            {
                new PermissionRequirement
                {
                    Endpoint = "All endpoints",
                    Permission = "Bearer token required",
                    Scope = "menu:read, menu:write"
                }
            },
            VaultIntegration = new VaultInfo
            {
                Provider = "HashiCorp Vault",
                SecretsPath = "secret/data/menuservice",
                ConfiguredSecrets = new[] { "DatabaseConnection", "JwtSecret", "CorsSettings" }
            }
        };

        return TypedResults.Ok(security);
    }

    private static Ok<GrpcDocumentation> GetGrpcDocumentation()
    {
        var grpc = new GrpcDocumentation
        {
            ServiceName = "MenuProtoService",
            ProtoFile = "/protos/menu.proto",
            Endpoints = new[]
            {
                new GrpcEndpoint
                {
                    Name = "GetAllMenu",
                    Request = "GetAllMenuRequest",
                    Response = "MenuListReply",
                    Description = "Récupère tous les menus"
                },
                new GrpcEndpoint
                {
                    Name = "GetAllByAppMenu",
                    Request = "GetAllByAppMenuRequest",
                    Response = "MenuListReply",
                    Description = "Récupère les menus par application"
                },
                new GrpcEndpoint
                {
                    Name = "CreateMenu",
                    Request = "CreateMenuRequest",
                    Response = "MenuReply",
                    Description = "Crée un nouveau menu"
                },
                new GrpcEndpoint
                {
                    Name = "UpdateMenu",
                    Request = "UpdateMenuRequest",
                    Response = "MenuReply",
                    Description = "Met à jour un menu"
                },
                new GrpcEndpoint
                {
                    Name = "ActiveMenu",
                    Request = "ActiveMenuRequest",
                    Response = "Empty",
                    Description = "Active un menu"
                },
                new GrpcEndpoint
                {
                    Name = "InactiveMenu",
                    Request = "InactiveMenuRequest",
                    Response = "Empty",
                    Description = "Désactive un menu"
                }
            }
        };

        return TypedResults.Ok(grpc);
    }

    private static Ok<HierarchyDocumentation> GetHierarchyDocumentation()
    {
        var hierarchy = new HierarchyDocumentation
        {
            Description = "Le MenuService supporte une hiérarchie de menus avec menus parents et enfants",
            Features = new[]
            {
                "Menus parents et sous-menus",
                "Ordre d'affichage configurable",
                "Navigation récursive",
                "Validation de cycles"
            },
            Schema = new HierarchySchema
            {
                ParentField = "parentMenuId",
                OrderField = "ordre",
                MaxDepth = "Non limité (attention aux performances)",
                CycleDetection = "Validation lors de la création/mise à jour"
            },
            Examples = new[]
            {
                new HierarchyExample
                {
                    Name = "Menu à deux niveaux",
                    Structure = @"
- Dashboard (ordre: 1)
- Gestion (ordre: 2)
  - Utilisateurs (ordre: 1, parentMenuId: [Gestion.Id])
  - Rôles (ordre: 2, parentMenuId: [Gestion.Id])
- Rapports (ordre: 3)"
                }
            }
        };

        return TypedResults.Ok(hierarchy);
    }

    private static Ok<MultiAppDocumentation> GetMultiAppDocumentation()
    {
        var multiApp = new MultiAppDocumentation
        {
            Description = "Support natif pour multiple applications avec isolation des données",
            KeyField = "AppAdminReference",
            Features = new[]
            {
                "Isolation complète des menus par application",
                "Filtrage automatique par AppAdminReference",
                "Support de références croisées entre applications",
                "Configuration spécifique par application"
            },
            QueryPattern = "Tous les endpoints de requête filtrent par AppAdminReference",
            Examples = new[]
            {
                new MultiAppExample
                {
                    Application = "APP001",
                    Description = "Application de gestion",
                    MenuCount = 15
                },
                new MultiAppExample
                {
                    Application = "APP002",
                    Description = "Application client",
                    MenuCount = 8
                }
            }
        };

        return TypedResults.Ok(multiApp);
    }

    private static Ok<StateManagementDocumentation> GetStateManagementDocumentation()
    {
        var stateManagement = new StateManagementDocumentation
        {
            Description = "Gestion d'état binaire actif/inactif pour les menus",
            StateField = "IsActif",
            Endpoints = new[]
            {
                new StateEndpoint
                {
                    Name = "ActiveMenu",
                    Method = "PATCH",
                    Path = "/menu/active",
                    Description = "Active un menu",
                    Effect = "IsActif = true"
                },
                new StateEndpoint
                {
                    Name = "InactiveMenu",
                    Method = "PATCH",
                    Path = "/menu/inactive",
                    Description = "Désactive un menu",
                    Effect = "IsActif = false"
                }
            },
            QueryBehavior = "GetAllActifMenu filtre automatiquement les menus inactifs",
            UseCases = new[]
            {
                "Maintenance temporaire de sections",
                "Déploiement progressif de features",
                "Gestion des accès saisonniers"
            }
        };

        return TypedResults.Ok(stateManagement);
    }

    private static Ok<ExamplesDocumentation> GetExamples()
    {
        var examples = new ExamplesDocumentation
        {
            UseCases = new[]
            {
                new UseCase
                {
                    Name = "Création d'une structure de menu complète",
                    Description = "Créer une hiérarchie de menus pour une nouvelle application",
                    Steps = new[]
                    {
                        "1. Créer les menus parents (Dashboard, Gestion, etc.)",
                        "2. Créer les sous-menus avec parentMenuId",
                        "3. Activer tous les menus nécessaires",
                        "4. Vérifier avec GetAllActifMenu"
                    },
                    Example = @"
// 1. Créer menu parent
POST /api/menu
{
  ""menu"": {
    ""name"": ""Gestion"",
    ""reference"": ""gestion"",
    ""urlFront"": ""/gestion"",
    ""icon"": ""settings"",
    ""appAdminReference"": ""APP001""
  }
}

// 2. Créer sous-menu
POST /api/menu
{
  ""menu"": {
    ""name"": ""Utilisateurs"",
    ""reference"": ""utilisateurs"",
    ""urlFront"": ""/gestion/utilisateurs"",
    ""icon"": ""users"",
    ""appAdminReference"": ""APP001"",
    ""parentMenuId"": ""[ID_MENU_GESTION]""
  }
}"
                }
            }
        };

        return TypedResults.Ok(examples);
    }

    private static Ok<MigrationGuide> GetMigrationGuide()
    {
        var migration = new MigrationGuide
        {
            CurrentVersion = "1.0.0",
            BreakingChanges = new[]
            {
                new BreakingChange
                {
                    Version = "1.0.0",
                    Description = "Version initiale",
                    Changes = new[] { "N/A" },
                    MigrationSteps = new[] { "N/A" }
                }
            },
            CompatibilityMatrix = new[]
            {
                new VersionCompatibility
                {
                    ClientVersion = "1.x",
                    ServerVersions = new[] { "1.0.0" },
                    Compatible = true
                }
            }
        };

        return TypedResults.Ok(migration);
    }

    private static Ok<PerformanceHints> GetPerformanceHints()
    {
        var performance = new PerformanceHints
        {
            Caching = new CachingStrategy
            {
                Recommended = true,
                CacheDuration = "5-15 minutes pour les menus",
                CacheKey = "menu:{appAdminReference}:{isActif}",
                InvalidationTriggers = new[] { "Create", "Update", "Active", "Inactive" }
            },
            Pagination = new PaginationStrategy
            {
                Supported = false,
                Reason = "Les menus sont généralement peu nombreux (<100)",
                Alternative = "Filtrage côté client recommandé"
            },
            QueryOptimization = new[]
            {
                "Index sur AppAdminReference",
                "Index composite sur (AppAdminReference, IsActif)",
                "Eager loading pour parentMenu si nécessaire"
            }
        };

        return TypedResults.Ok(performance);
    }

    private static Ok<IntegrationPatterns> GetIntegrationPatterns()
    {
        var integration = new IntegrationPatterns
        {
            Patterns = new[]
            {
                new IntegrationPattern
                {
                    Name = "Event-Driven",
                    Description = "Publication d'événements lors des changements de menu",
                    Implementation = "À implémenter avec RabbitMQ/Kafka",
                    Events = new[] { "MenuCreated", "MenuUpdated", "MenuActivated", "MenuDeactivated" }
                },
                new IntegrationPattern
                {
                    Name = "Synchronous REST",
                    Description = "Intégration directe via API REST",
                    Implementation = "Disponible via endpoints Carter",
                    BestPractices = new[] { "Utiliser circuit breaker", "Implémenter retry avec backoff" }
                },
                new IntegrationPattern
                {
                    Name = "Asynchronous gRPC",
                    Description = "Communication haute performance via gRPC",
                    Implementation = "Service gRPC disponible",
                    UseCases = new[] { "Synchronisation en masse", "Communication inter-services" }
                }
            }
        };

        return TypedResults.Ok(integration);
    }

    private static Ok<TroubleshootingGuide> GetTroubleshooting()
    {
        var troubleshooting = new TroubleshootingGuide
        {
            CommonIssues = new[]
            {
                new TroubleshootingIssue
                {
                    Problem = "Menu non visible après création",
                    PossibleCauses = new[] { "Menu inactif", "Mauvais AppAdminReference", "Cache non invalidé" },
                    Solutions = new[] { "Vérifier IsActif", "Vérifier AppAdminReference", "Invalider le cache" }
                },
                new TroubleshootingIssue
                {
                    Problem = "Erreur de validation lors de la création",
                    PossibleCauses = new[] { "Reference non lowercase", "Champs requis manquants", "Reference déjà existante" },
                    Solutions = new[] { "Vérifier format reference", "Vérifier tous les champs requis", "Utiliser une reference unique" }
                }
            }
        };

        return TypedResults.Ok(troubleshooting);
    }

    private static Ok<VersionsDocumentation> GetVersions()
    {
        var versions = new VersionsDocumentation
        {
            Current = "v1",
            Available = new[]
            {
                new VersionInfo
                {
                    Version = "v1",
                    Status = "current",
                    ReleaseDate = "2025-01-01",
                    Deprecated = false,
                    Features = new[] { "CRUD complet", "Gestion d'état", "Support hiérarchie", "Multi-applications" }
                }
            }
        };

        return TypedResults.Ok(versions);
    }

    private static Ok<SearchResults> SearchDocumentation([FromQuery] string? q)
    {
        var results = new SearchResults
        {
            Query = q ?? string.Empty,
            Results = new List<SearchResult>()
        };

        if (string.IsNullOrWhiteSpace(q))
        {
            return TypedResults.Ok(results);
        }

        var searchTerm = q.ToLower();

        // Search in endpoints
        foreach (var endpoint in GetEndpointsDictionary())
        {
            if (endpoint.Key.Contains(searchTerm) ||
                endpoint.Value.Path.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                endpoint.Value.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            {
                results.Results.Add(new SearchResult
                {
                    Type = "endpoint",
                    Name = endpoint.Value.Name,
                    Description = endpoint.Value.Description,
                    Url = $"/api/docs/ai/endpoints/{endpoint.Key}"
                });
            }
        }

        // Search in models
        foreach (var model in GetModelSchemas())
        {
            if (model.Key.Contains(searchTerm) ||
                model.Value.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            {
                results.Results.Add(new SearchResult
                {
                    Type = "model",
                    Name = model.Value.Name,
                    Description = model.Value.Description,
                    Url = $"/api/docs/ai/models/{model.Key}"
                });
            }
        }

        // Search in features
        if ("state".Contains(searchTerm) || "actif".Contains(searchTerm) || "active".Contains(searchTerm))
        {
            results.Results.Add(new SearchResult
            {
                Type = "feature",
                Name = "State Management",
                Description = "Gestion de l'état actif/inactif des menus",
                Url = "/api/docs/ai/features/state-management"
            });
        }

        if ("hierarchy".Contains(searchTerm) || "parent".Contains(searchTerm))
        {
            results.Results.Add(new SearchResult
            {
                Type = "feature",
                Name = "Hierarchy",
                Description = "Gestion de la hiérarchie des menus",
                Url = "/api/docs/ai/features/hierarchy"
            });
        }

        return TypedResults.Ok(results);
    }

    private static Ok<OpenApiMapping> GetOpenApiMapping()
    {
        var mapping = new OpenApiMapping
        {
            Mappings = new Dictionary<string, string>
            {
                { "endpoints", "paths" },
                { "models", "components/schemas" },
                { "authentication", "components/securitySchemes" },
                { "errors", "components/responses" }
            },
            OpenApiUrl = "/swagger/v1/swagger.json"
        };

        return TypedResults.Ok(mapping);
    }

    private static Ok<HealthStatus> GetHealth()
    {
        var health = new HealthStatus
        {
            Status = "healthy",
            DocumentationComplete = true,
            LastUpdated = DateTime.UtcNow,
            Version = ServiceVersion,
            EndpointCount = 6, // Excluding documentation endpoints
            ModelCount = 3,
            AdditionalChecks = new Dictionary<string, bool>
            {
                { "gRPC Service", true },
                { "Vault Connection", true },
                { "Database Connection", true }
            }
        };

        return TypedResults.Ok(health);
    }

    private static List<EndpointSummary> GetEndpointsList()
    {
        return new List<EndpointSummary>
        {
            new() { Name = "GetAllMenu", Method = "GET", Path = "/api/menu/{appAdminReference}", Description = "Récupère tous les menus d'une application" },
            new() { Name = "GetAllActifMenu", Method = "GET", Path = "/api/menu/{appAdminReference}/actif", Description = "Récupère uniquement les menus actifs" },
            new() { Name = "CreateMenu", Method = "POST", Path = "/api/menu", Description = "Crée un nouveau menu" },
            new() { Name = "UpdateMenu", Method = "PUT", Path = "/api/menu", Description = "Met à jour un menu existant" },
            new() { Name = "ActiveMenu", Method = "PATCH", Path = "/api/menu/active", Description = "Active un menu" },
            new() { Name = "InactiveMenu", Method = "PATCH", Path = "/api/menu/inactive", Description = "Désactive un menu" }
        };
    }

    private static List<ModelSummary> GetModelsList()
    {
        return new List<ModelSummary>
        {
            new() { Name = "MenuDTO", Type = "object", Description = "Données de base d'un menu" },
            new() { Name = "MenuStateDto", Type = "object", Description = "Menu avec état actif/inactif" },
            new() { Name = "Menu", Type = "entity", Description = "Entité de domaine Menu" }
        };
    }

    private static List<FeatureInfo> GetFeaturesList()
    {
        var features = new List<FeatureInfo>();
        features.AddRange(GetFeatures().Value.Commands);
        features.AddRange(GetFeatures().Value.Queries);
        return features;
    }

    private static List<ErrorCode> GetErrorCodes()
    {
        return new List<ErrorCode>
        {
            new() { Code = 400, Name = "BadRequest", Description = "Requête invalide ou validation échouée" },
            new() { Code = 401, Name = "Unauthorized", Description = "Authentification requise" },
            new() { Code = 404, Name = "NotFound", Description = "Menu non trouvé" },
            new() { Code = 409, Name = "Conflict", Description = "Conflit (ex: reference déjà existante)" },
            new() { Code = 500, Name = "InternalServerError", Description = "Erreur interne du serveur" }
        };
    }

    private static Dictionary<string, EndpointDocumentation> GetEndpointsDictionary()
    {
        return new Dictionary<string, EndpointDocumentation>
        {
            ["getallmenu"] = new EndpointDocumentation
            {
                Name = "GetAllMenu",
                Method = "GET",
                Path = "/api/menu/{appAdminReference}",
                Description = "Récupère tous les menus (actifs et inactifs) d'une application",
                Parameters = new[]
                {
                    new ApiParameterInfo { Name = "appAdminReference", Type = "string", Required = true, In = "path", Description = "Référence de l'application" }
                },
                RequestExample = null,
                ResponseExample = @"[
  {
    ""id"": ""123e4567-e89b-12d3-a456-426614174000"",
    ""name"": ""Dashboard"",
    ""reference"": ""dashboard"",
    ""urlFront"": ""/dashboard"",
    ""icon"": ""home"",
    ""isActif"": true,
    ""appAdminReference"": ""APP001""
  },
  {
    ""id"": ""223e4567-e89b-12d3-a456-426614174000"",
    ""name"": ""Gestion"",
    ""reference"": ""gestion"",
    ""urlFront"": ""/gestion"",
    ""icon"": ""settings"",
    ""isActif"": true,
    ""appAdminReference"": ""APP001"",
    ""parentMenuId"": null
  }
]",
                ErrorResponses = new[]
                {
                    new ErrorResponse { Code = 400, Description = "AppAdminReference invalide" },
                    new ErrorResponse { Code = 401, Description = "Non autorisé" }
                },
                Authorization = new[] { "Bearer token requis" }
            },
            ["getallactifmenu"] = new EndpointDocumentation
            {
                Name = "GetAllActifMenu",
                Method = "GET",
                Path = "/api/menu/{appAdminReference}/actif",
                Description = "Récupère uniquement les menus actifs d'une application",
                Parameters = new[]
                {
                    new ApiParameterInfo { Name = "appAdminReference", Type = "string", Required = true, In = "path", Description = "Référence de l'application" }
                },
                RequestExample = null,
                ResponseExample = @"[
  {
    ""id"": ""123e4567-e89b-12d3-a456-426614174000"",
    ""name"": ""Dashboard"",
    ""reference"": ""dashboard"",
    ""urlFront"": ""/dashboard"",
    ""icon"": ""home"",
    ""isActif"": true,
    ""appAdminReference"": ""APP001""
  }
]",
                ErrorResponses = new[]
                {
                    new ErrorResponse { Code = 400, Description = "AppAdminReference invalide" },
                    new ErrorResponse { Code = 401, Description = "Non autorisé" }
                },
                Authorization = new[] { "Bearer token requis" }
            },
            ["createmenu"] = new EndpointDocumentation
            {
                Name = "CreateMenu",
                Method = "POST",
                Path = "/api/menu",
                Description = "Crée un nouveau menu",
                Parameters = Array.Empty<ApiParameterInfo>(),
                RequestExample = @"{
  ""menu"": {
    ""name"": ""Nouveau Menu"",
    ""reference"": ""nouveaumenu"",
    ""urlFront"": ""/nouveau-menu"",
    ""icon"": ""folder"",
    ""appAdminReference"": ""APP001""
  }
}",
                ResponseExample = @"{
  ""data"": {
    ""id"": ""323e4567-e89b-12d3-a456-426614174000""
  },
  ""message"": ""Menu créé avec succès"",
  ""isSuccess"": true,
  ""statusCode"": 201
}",
                ErrorResponses = new[]
                {
                    new ErrorResponse { Code = 400, Description = "Validation échouée" },
                    new ErrorResponse { Code = 401, Description = "Non autorisé" },
                    new ErrorResponse { Code = 409, Description = "Reference déjà existante" }
                },
                Authorization = new[] { "Bearer token requis" }
            },
            ["updatemenu"] = new EndpointDocumentation
            {
                Name = "UpdateMenu",
                Method = "PUT",
                Path = "/api/menu",
                Description = "Met à jour un menu existant",
                Parameters = Array.Empty<ApiParameterInfo>(),
                RequestExample = @"{
  ""menu"": {
    ""id"": ""123e4567-e89b-12d3-a456-426614174000"",
    ""name"": ""Dashboard Modifié"",
    ""reference"": ""dashboard"",
    ""urlFront"": ""/dashboard-v2"",
    ""icon"": ""dashboard"",
    ""appAdminReference"": ""APP001""
  }
}",
                ResponseExample = @"{
  ""data"": {
    ""id"": ""123e4567-e89b-12d3-a456-426614174000""
  },
  ""message"": ""Menu mis à jour avec succès"",
  ""isSuccess"": true,
  ""statusCode"": 200
}",
                ErrorResponses = new[]
                {
                    new ErrorResponse { Code = 400, Description = "Validation échouée" },
                    new ErrorResponse { Code = 401, Description = "Non autorisé" },
                    new ErrorResponse { Code = 404, Description = "Menu non trouvé" }
                },
                Authorization = new[] { "Bearer token requis" }
            },
            ["activemenu"] = new EndpointDocumentation
            {
                Name = "ActiveMenu",
                Method = "PATCH",
                Path = "/api/menu/active",
                Description = "Active un menu (IsActif = true)",
                Parameters = Array.Empty<ApiParameterInfo>(),
                RequestExample = @"{
  ""menu"": {
    ""id"": ""123e4567-e89b-12d3-a456-426614174000"",
    ""name"": ""Dashboard"",
    ""reference"": ""dashboard"",
    ""urlFront"": ""/dashboard"",
    ""icon"": ""home"",
    ""isActif"": false,
    ""appAdminReference"": ""APP001""
  }
}",
                ResponseExample = @"{
  ""message"": ""Menu activé avec succès"",
  ""isSuccess"": true,
  ""statusCode"": 200
}",
                ErrorResponses = new[]
                {
                    new ErrorResponse { Code = 400, Description = "Données invalides" },
                    new ErrorResponse { Code = 401, Description = "Non autorisé" },
                    new ErrorResponse { Code = 404, Description = "Menu non trouvé" }
                },
                Authorization = new[] { "Bearer token requis" }
            },
            ["inactivemenu"] = new EndpointDocumentation
            {
                Name = "InactiveMenu",
                Method = "PATCH",
                Path = "/api/menu/inactive",
                Description = "Désactive un menu (IsActif = false)",
                Parameters = Array.Empty<ApiParameterInfo>(),
                RequestExample = @"{
  ""menu"": {
    ""id"": ""123e4567-e89b-12d3-a456-426614174000"",
    ""name"": ""Dashboard"",
    ""reference"": ""dashboard"",
    ""urlFront"": ""/dashboard"",
    ""icon"": ""home"",
    ""isActif"": true,
    ""appAdminReference"": ""APP001""
  }
}",
                ResponseExample = @"{
  ""message"": ""Menu désactivé avec succès"",
  ""isSuccess"": true,
  ""statusCode"": 200
}",
                ErrorResponses = new[]
                {
                    new ErrorResponse { Code = 400, Description = "Données invalides" },
                    new ErrorResponse { Code = 401, Description = "Non autorisé" },
                    new ErrorResponse { Code = 404, Description = "Menu non trouvé" }
                },
                Authorization = new[] { "Bearer token requis" }
            }
        };
    }

    private static Dictionary<string, ModelSchema> GetModelSchemas()
    {
        return new Dictionary<string, ModelSchema>
        {
            ["menudto"] = new ModelSchema
            {
                Name = "MenuDTO",
                Type = "object",
                Description = "Données de transfert pour la création/mise à jour de menu",
                Properties = new[]
                {
                    new ApiPropertyInfo { Name = "name", Type = "string", Required = true, Description = "Nom du menu (max 100 caractères)" },
                    new ApiPropertyInfo { Name = "reference", Type = "string", Required = true, Description = "Référence unique en minuscules (max 50 caractères, alphabétique)" },
                    new ApiPropertyInfo { Name = "urlFront", Type = "string", Required = true, Description = "URL de navigation frontend (max 200 caractères)" },
                    new ApiPropertyInfo { Name = "icon", Type = "string", Required = false, Description = "Icône du menu (max 100 caractères)" },
                    new ApiPropertyInfo { Name = "appAdminReference", Type = "string", Required = true, Description = "Référence de l'application (max 50 caractères)" }
                }
            },
            ["menustatedto"] = new ModelSchema
            {
                Name = "MenuStateDto",
                Type = "object",
                Description = "Menu avec informations d'état (hérite de MenuDTO)",
                Properties = new[]
                {
                    new ApiPropertyInfo { Name = "id", Type = "string (guid)", Required = true, Description = "Identifiant unique du menu" },
                    new ApiPropertyInfo { Name = "name", Type = "string", Required = true, Description = "Nom du menu" },
                    new ApiPropertyInfo { Name = "reference", Type = "string", Required = true, Description = "Référence unique" },
                    new ApiPropertyInfo { Name = "urlFront", Type = "string", Required = true, Description = "URL de navigation" },
                    new ApiPropertyInfo { Name = "icon", Type = "string", Required = false, Description = "Icône du menu" },
                    new ApiPropertyInfo { Name = "isActif", Type = "boolean", Required = true, Description = "État actif/inactif du menu" },
                    new ApiPropertyInfo { Name = "appAdminReference", Type = "string", Required = true, Description = "Référence de l'application" }
                }
            },
            ["menu"] = new ModelSchema
            {
                Name = "Menu",
                Type = "entity",
                Description = "Entité de domaine représentant un menu dans la base de données",
                Properties = new[]
                {
                    new ApiPropertyInfo { Name = "Id", Type = "Guid", Required = true, Description = "Clé primaire" },
                    new ApiPropertyInfo { Name = "Name", Type = "string", Required = true, Description = "Nom du menu (mappé sur cf1)" },
                    new ApiPropertyInfo { Name = "Reference", Type = "string", Required = true, Description = "Référence unique (mappé sur cf2)" },
                    new ApiPropertyInfo { Name = "UrlFront", Type = "string", Required = true, Description = "URL frontend (mappé sur cf3)" },
                    new ApiPropertyInfo { Name = "Icon", Type = "string", Required = false, Description = "Icône (mappé sur cf4)" },
                    new ApiPropertyInfo { Name = "IsActif", Type = "bool", Required = true, Description = "État actif (mappé sur cf5)" },
                    new ApiPropertyInfo { Name = "AppAdminReference", Type = "string", Required = true, Description = "Application (mappé sur cf6)" }
                }
            }
        };
    }
}