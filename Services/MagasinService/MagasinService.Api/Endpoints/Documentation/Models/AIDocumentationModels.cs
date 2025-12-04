namespace MagasinService.Api.Endpoints.Documentation.Models;

// Manifest model
public record AIManifest
{
    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string[] Endpoints { get; init; } = Array.Empty<string>();
    public string Documentation { get; init; } = string.Empty;
}

// Full documentation model
public record AIDocumentation
{
    public ServiceInfo Service { get; init; } = new();
    public ArchitectureInfo Architecture { get; init; } = new();
    public List<EndpointSummary> Endpoints { get; init; } = new();
    public List<ModelSummary> Models { get; init; } = new();
    public List<FeatureInfo> Features { get; init; } = new();
    public AuthenticationInfo Authentication { get; init; } = new();
    public List<ErrorCode> Errors { get; init; } = new();
}

// Service information
public record ServiceInfo
{
    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = string.Empty;
    public string Protocol { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
}

// Architecture information
public record ArchitectureInfo
{
    public string Pattern { get; init; } = string.Empty;
    public string[] Layers { get; init; } = Array.Empty<string>();
    public string[] Patterns { get; init; } = Array.Empty<string>();
}

// Endpoint models
public record EndpointSummary
{
    public string Name { get; init; } = string.Empty;
    public string Method { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public record EndpointDocumentation
{
    public string Name { get; init; } = string.Empty;
    public string Method { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public ApiParameterInfo[] Parameters { get; init; } = Array.Empty<ApiParameterInfo>();
    public string? RequestExample { get; init; }
    public string? ResponseExample { get; init; }
    public ErrorResponse[] ErrorResponses { get; init; } = Array.Empty<ErrorResponse>();
    public string[] Authorization { get; init; } = Array.Empty<string>();
}

public record ApiParameterInfo
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public bool Required { get; init; }
    public string In { get; init; } = string.Empty; // path, query, body, header
    public string Description { get; init; } = string.Empty;
}

public record ErrorResponse
{
    public int Code { get; init; }
    public string Description { get; init; } = string.Empty;
}

// Model schema
public record ModelSummary
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public record ModelSchema
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public ApiPropertyInfo[] Properties { get; init; } = Array.Empty<ApiPropertyInfo>();
}

public record ApiPropertyInfo
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public bool Required { get; init; }
    public string Description { get; init; } = string.Empty;
}

// Feature documentation
public record FeatureInfo
{
    public string Type { get; init; } = string.Empty; // Command or Query
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string[] Parameters { get; init; } = Array.Empty<string>();
    public string Response { get; init; } = string.Empty;
}

public record FeaturesDocumentation
{
    public FeatureInfo[] Commands { get; init; } = Array.Empty<FeatureInfo>();
    public FeatureInfo[] Queries { get; init; } = Array.Empty<FeatureInfo>();
}

// Authentication
public record AuthenticationInfo
{
    public string Type { get; init; } = string.Empty;
    public bool Required { get; init; }
    public string HeaderName { get; init; } = string.Empty;
    public string HeaderFormat { get; init; } = string.Empty;
}

// Error codes
public record ErrorCode
{
    public int Code { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

// Versions
public record VersionsDocumentation
{
    public string Current { get; init; } = string.Empty;
    public VersionInfo[] Available { get; init; } = Array.Empty<VersionInfo>();
}

public record VersionInfo
{
    public string Version { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ReleaseDate { get; init; } = string.Empty;
    public bool Deprecated { get; init; }
}

// Search
public record SearchResults
{
    public string Query { get; init; } = string.Empty;
    public List<SearchResult> Results { get; init; } = new();
}

public record SearchResult
{
    public string Type { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
}

// OpenAPI mapping
public record OpenApiMapping
{
    public Dictionary<string, string> Mappings { get; init; } = new();
    public string OpenApiUrl { get; init; } = string.Empty;
}

// Health
public record HealthStatus
{
    public string Status { get; init; } = string.Empty;
    public bool DocumentationComplete { get; init; }
    public DateTime LastUpdated { get; init; }
    public string Version { get; init; } = string.Empty;
    public int EndpointCount { get; init; }
    public int ModelCount { get; init; }
}