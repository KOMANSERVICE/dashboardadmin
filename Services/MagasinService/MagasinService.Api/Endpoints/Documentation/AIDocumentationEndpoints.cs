using Carter;
using MagasinService.Api.Endpoints.Documentation.Models;
using MagasinService.Application.Features.Magasins.Commands.CreateMagasin;
using MagasinService.Application.Features.Magasins.Commands.UpdateMagasin;
using MagasinService.Application.Features.Magasins.Queries.GetAllMagasin;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MagasinService.Api.Endpoints.Documentation;

public class AIDocumentationEndpoints : ICarterModule
{
    private const string ServiceName = "MagasinService";
    private const string ServiceVersion = "1.0.0";
    private const string ServiceDescription = "Service de gestion des magasins et points de stockage";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/docs/ai")
            .WithTags("AI Documentation")
            .WithDescription("Documentation exploitable par AI")
            .WithOpenApi();

        // Manifest endpoint
        group.MapGet("/manifest", GetManifest)
            .WithName("GetAIManifest")
            .WithSummary("Récupère le manifest de documentation AI")
            .Produces<AIManifest>(StatusCodes.Status200OK)
            .CacheOutput(policy => policy.Expire(TimeSpan.FromHours(1)));

        // Full documentation endpoint
        group.MapGet("/", GetFullDocumentation)
            .WithName("GetAIDocumentation")
            .WithSummary("Récupère la documentation complète")
            .Produces<AIDocumentation>(StatusCodes.Status200OK)
            .CacheOutput(policy => policy.Expire(TimeSpan.FromHours(1)));

        // Endpoint details
        group.MapGet("/endpoints/{name}", GetEndpointDetails)
            .WithName("GetAIEndpointDetails")
            .WithSummary("Récupère les détails d'un endpoint")
            .Produces<EndpointDocumentation>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // Model schemas
        group.MapGet("/models/{name}", GetModelSchema)
            .WithName("GetAIModelSchema")
            .WithSummary("Récupère le schéma d'un modèle")
            .Produces<ModelSchema>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // Features (CQRS)
        group.MapGet("/features", GetFeatures)
            .WithName("GetAIFeatures")
            .WithSummary("Récupère la liste des features CQRS")
            .Produces<FeaturesDocumentation>(StatusCodes.Status200OK);

        // Versions
        group.MapGet("/versions", GetVersions)
            .WithName("GetAIVersions")
            .WithSummary("Récupère les versions disponibles")
            .Produces<VersionsDocumentation>(StatusCodes.Status200OK);

        // Search
        group.MapGet("/search", SearchDocumentation)
            .WithName("SearchAIDocumentation")
            .WithSummary("Recherche dans la documentation")
            .Produces<SearchResults>(StatusCodes.Status200OK);

        // OpenAPI mapping
        group.MapGet("/openapi-mapping", GetOpenApiMapping)
            .WithName("GetOpenApiMapping")
            .WithSummary("Récupère le mapping avec OpenAPI")
            .Produces<OpenApiMapping>(StatusCodes.Status200OK);

        // Health check
        group.MapGet("/health", GetHealth)
            .WithName("GetAIDocumentationHealth")
            .WithSummary("Vérifie la santé du service de documentation")
            .Produces<HealthStatus>(StatusCodes.Status200OK);
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
                "/api/magasin/{BoutiqueId} (GET) - Récupère tous les magasins d'une boutique",
                "/api/magasin/{BoutiqueId} (POST) - Crée un nouveau magasin",
                "/api/magasin/{BoutiqueId}/{StockLocationId} (PATCH) - Met à jour un magasin",
                "/api/magasin/{id} (GET) - Récupère un magasin spécifique",
                "/api/stock-movements (POST) - Crée un mouvement de stock",
                "/api/stock-movements (GET) - Récupère les mouvements de stock",
                "/api/stock-slips (POST) - Crée un bordereau de mouvement",
                "/api/docs/ai/manifest (GET) - Récupère le manifest de documentation",
                "/api/docs/ai (GET) - Récupère la documentation complète",
                "/api/docs/ai/endpoints/{name} (GET) - Récupère les détails d'un endpoint",
                "/api/docs/ai/models/{name} (GET) - Récupère le schéma d'un modèle",
                "/api/docs/ai/features (GET) - Récupère les features CQRS",
                "/api/docs/ai/versions (GET) - Récupère les versions",
                "/api/docs/ai/search (GET) - Recherche dans la documentation",
                "/api/docs/ai/openapi-mapping (GET) - Mapping avec OpenAPI",
                "/api/docs/ai/health (GET) - Statut de santé"
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
                Protocol = "HTTPS",
                ContentType = "application/json"
            },
            Architecture = new ArchitectureInfo
            {
                Pattern = "Clean Architecture with Vertical Slice",
                Layers = new[]
                {
                    "API (Carter endpoints)",
                    "Application (CQRS handlers)",
                    "Domain (Entities and Value Objects)",
                    "Infrastructure (EF Core, PostgreSQL)"
                },
                Patterns = new[]
                {
                    "CQRS (Command Query Responsibility Segregation)",
                    "Repository Pattern",
                    "Unit of Work",
                    "Value Objects",
                    "Domain-Driven Design"
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
                HeaderFormat = "Bearer {token}"
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
                    Name = "CreateMagasinCommand",
                    Description = "Création d'un nouveau magasin",
                    Parameters = new[] { "StockLocationDTO", "BoutiqueId" },
                    Response = "CreateMagasinResult"
                },
                new FeatureInfo
                {
                    Type = "Command",
                    Name = "UpdateMagasinCommand",
                    Description = "Mise à jour d'un magasin existant",
                    Parameters = new[] { "StockLocationUpdateDTO", "BoutiqueId", "StockLocationId" },
                    Response = "UpdateMagasinResult"
                }
            },
            Queries = new[]
            {
                new FeatureInfo
                {
                    Type = "Query",
                    Name = "GetAllMagasinQuery",
                    Description = "Récupération de tous les magasins d'une boutique",
                    Parameters = new[] { "BoutiqueId" },
                    Response = "List<StockLocationDTO>"
                },
                new FeatureInfo
                {
                    Type = "Query",
                    Name = "GetOneMagasinQuery",
                    Description = "Récupération d'un magasin spécifique",
                    Parameters = new[] { "BoutiqueId", "StockLocationId" },
                    Response = "StockLocationDTO"
                }
            }
        };

        return TypedResults.Ok(features);
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
                    Deprecated = false
                }
            }
        };

        return TypedResults.Ok(versions);
    }

    private static Ok<SearchResults> SearchDocumentation(string? q)
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
            EndpointCount = 7, // Excluding documentation endpoints
            ModelCount = 6
        };

        return TypedResults.Ok(health);
    }

    private static List<EndpointSummary> GetEndpointsList()
    {
        return new List<EndpointSummary>
        {
            new() { Name = "GetAllMagasin", Method = "GET", Path = "/api/magasin/{BoutiqueId}", Description = "Récupère tous les magasins d'une boutique" },
            new() { Name = "GetOneMagasin", Method = "GET", Path = "/api/magasin/{id}", Description = "Récupère un magasin spécifique" },
            new() { Name = "CreateMagasin", Method = "POST", Path = "/api/magasin/{BoutiqueId}", Description = "Crée un nouveau magasin" },
            new() { Name = "UpdateMagasin", Method = "PATCH", Path = "/api/magasin/{BoutiqueId}/{StockLocationId}", Description = "Met à jour un magasin existant" },
            new() { Name = "CreateStockMovement", Method = "POST", Path = "/api/stock-movements", Description = "Crée un mouvement de stock entre deux magasins" },
            new() { Name = "GetStockMovements", Method = "GET", Path = "/api/stock-movements", Description = "Récupère les mouvements de stock avec filtres" },
            new() { Name = "CreateStockSlip", Method = "POST", Path = "/api/stock-slips", Description = "Crée un bordereau de mouvement avec plusieurs produits" }
        };
    }

    private static List<ModelSummary> GetModelsList()
    {
        return new List<ModelSummary>
        {
            new() { Name = "StockLocationDTO", Type = "object", Description = "Représentation complète d'un magasin" },
            new() { Name = "StockLocationUpdateDTO", Type = "object", Description = "Données de mise à jour d'un magasin" },
            new() { Name = "CreateStockMovementRequest", Type = "object", Description = "Demande de création d'un mouvement de stock" },
            new() { Name = "StockMovementDto", Type = "object", Description = "Représentation d'un mouvement de stock" },
            new() { Name = "CreateStockSlipRequest", Type = "object", Description = "Demande de création d'un bordereau de mouvement" },
            new() { Name = "StockSlipItemRequest", Type = "object", Description = "Article d'un bordereau de mouvement" }
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
            new() { Code = 400, Name = "BadRequest", Description = "Requête invalide ou données manquantes" },
            new() { Code = 401, Name = "Unauthorized", Description = "Authentification requise" },
            new() { Code = 404, Name = "NotFound", Description = "Ressource non trouvée" },
            new() { Code = 409, Name = "Conflict", Description = "Conflit avec l'état actuel" },
            new() { Code = 500, Name = "InternalServerError", Description = "Erreur interne du serveur" }
        };
    }

    private static Dictionary<string, EndpointDocumentation> GetEndpointsDictionary()
    {
        return new Dictionary<string, EndpointDocumentation>
        {
            ["getallmagasin"] = new EndpointDocumentation
            {
                Name = "GetAllMagasin",
                Method = "GET",
                Path = "/api/magasin/{BoutiqueId}",
                Description = "Récupère tous les magasins associés à une boutique",
                Parameters = new[]
                {
                    new ApiParameterInfo { Name = "BoutiqueId", Type = "Guid", Required = true, In = "path", Description = "Identifiant unique de la boutique" }
                },
                RequestExample = null,
                ResponseExample = @"[
  {
    ""id"": ""123e4567-e89b-12d3-a456-426614174000"",
    ""name"": ""Magasin Principal"",
    ""address"": ""123 Rue du Commerce"",
    ""type"": 1
  }
]",
                ErrorResponses = new[]
                {
                    new ErrorResponse { Code = 400, Description = "BoutiqueId invalide" },
                    new ErrorResponse { Code = 401, Description = "Non autorisé" },
                    new ErrorResponse { Code = 404, Description = "Boutique non trouvée" }
                },
                Authorization = new[] { "Bearer token requis" }
            },
            ["createmagasin"] = new EndpointDocumentation
            {
                Name = "CreateMagasin",
                Method = "POST",
                Path = "/api/magasin/{BoutiqueId}",
                Description = "Crée un nouveau magasin pour une boutique",
                Parameters = new[]
                {
                    new ApiParameterInfo { Name = "BoutiqueId", Type = "Guid", Required = true, In = "path", Description = "Identifiant unique de la boutique" }
                },
                RequestExample = @"{
  ""stockLocation"": {
    ""id"": ""00000000-0000-0000-0000-000000000000"",
    ""name"": ""Nouveau Magasin"",
    ""address"": ""456 Avenue du Commerce"",
    ""type"": 1
  }
}",
                ResponseExample = @"{
  ""data"": {
    ""id"": ""123e4567-e89b-12d3-a456-426614174000""
  },
  ""message"": ""Magasin créé avec succès"",
  ""isSuccess"": true,
  ""statusCode"": 201
}",
                ErrorResponses = new[]
                {
                    new ErrorResponse { Code = 400, Description = "Données invalides" },
                    new ErrorResponse { Code = 401, Description = "Non autorisé" },
                    new ErrorResponse { Code = 409, Description = "Un magasin avec ce nom existe déjà" }
                },
                Authorization = new[] { "Bearer token requis" }
            },
            ["updatemagasin"] = new EndpointDocumentation
            {
                Name = "UpdateMagasin",
                Method = "PATCH",
                Path = "/api/magasin/{BoutiqueId}/{StockLocationId}",
                Description = "Met à jour les informations d'un magasin existant",
                Parameters = new[]
                {
                    new ApiParameterInfo { Name = "BoutiqueId", Type = "Guid", Required = true, In = "path", Description = "Identifiant unique de la boutique" },
                    new ApiParameterInfo { Name = "StockLocationId", Type = "Guid", Required = true, In = "path", Description = "Identifiant unique du magasin" }
                },
                RequestExample = @"{
  ""stockLocation"": {
    ""name"": ""Magasin Mis à Jour"",
    ""address"": ""789 Boulevard du Commerce""
  }
}",
                ResponseExample = @"{
  ""data"": {
    ""id"": ""123e4567-e89b-12d3-a456-426614174000""
  },
  ""message"": ""Magasin mis à jour avec succès"",
  ""isSuccess"": true,
  ""statusCode"": 200
}",
                ErrorResponses = new[]
                {
                    new ErrorResponse { Code = 400, Description = "Données invalides" },
                    new ErrorResponse { Code = 401, Description = "Non autorisé" },
                    new ErrorResponse { Code = 404, Description = "Magasin non trouvé" }
                },
                Authorization = new[] { "Bearer token requis" }
            }
        };
    }

    private static Dictionary<string, ModelSchema> GetModelSchemas()
    {
        return new Dictionary<string, ModelSchema>
        {
            ["stocklocationdto"] = new ModelSchema
            {
                Name = "StockLocationDTO",
                Type = "object",
                Description = "Représentation complète d'un point de stockage/magasin",
                Properties = new[]
                {
                    new ApiPropertyInfo { Name = "id", Type = "string (guid)", Required = true, Description = "Identifiant unique du magasin" },
                    new ApiPropertyInfo { Name = "name", Type = "string", Required = true, Description = "Nom du magasin" },
                    new ApiPropertyInfo { Name = "address", Type = "string", Required = false, Description = "Adresse du magasin" },
                    new ApiPropertyInfo { Name = "type", Type = "integer (enum)", Required = true, Description = "Type de magasin: 1=Sale, 2=Store, 3=Site" }
                }
            },
            ["stocklocationupdatedto"] = new ModelSchema
            {
                Name = "StockLocationUpdateDTO",
                Type = "object",
                Description = "Données pour la mise à jour d'un magasin",
                Properties = new[]
                {
                    new ApiPropertyInfo { Name = "name", Type = "string", Required = false, Description = "Nom du magasin" },
                    new ApiPropertyInfo { Name = "address", Type = "string", Required = false, Description = "Adresse du magasin" }
                }
            }
        };
    }
}