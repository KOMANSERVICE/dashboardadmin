namespace MenuService.Api.Endpoints.Documentation.Models;

// Base models (same as MagasinService)
public record AIManifest
{
    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string[] Endpoints { get; init; } = Array.Empty<string>();
    public string Documentation { get; init; } = string.Empty;
}

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

public record ServiceInfo
{
    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = string.Empty;
    public string Protocol { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public string[] Features { get; init; } = Array.Empty<string>();
}

public record ArchitectureInfo
{
    public string Pattern { get; init; } = string.Empty;
    public string[] Layers { get; init; } = Array.Empty<string>();
    public string[] Patterns { get; init; } = Array.Empty<string>();
}

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
    public string In { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public record ErrorResponse
{
    public int Code { get; init; }
    public string Description { get; init; } = string.Empty;
}

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

public record FeatureInfo
{
    public string Type { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string[] Parameters { get; init; } = Array.Empty<string>();
    public string Response { get; init; } = string.Empty;
    public string[] Validation { get; init; } = Array.Empty<string>();
}

public record FeaturesDocumentation
{
    public FeatureInfo[] Commands { get; init; } = Array.Empty<FeatureInfo>();
    public FeatureInfo[] Queries { get; init; } = Array.Empty<FeatureInfo>();
}

public record AuthenticationInfo
{
    public string Type { get; init; } = string.Empty;
    public bool Required { get; init; }
    public string HeaderName { get; init; } = string.Empty;
    public string HeaderFormat { get; init; } = string.Empty;
    public string AdditionalInfo { get; init; } = string.Empty;
}

public record ErrorCode
{
    public int Code { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

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
    public string[] Features { get; init; } = Array.Empty<string>();
}

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

public record OpenApiMapping
{
    public Dictionary<string, string> Mappings { get; init; } = new();
    public string OpenApiUrl { get; init; } = string.Empty;
}

public record HealthStatus
{
    public string Status { get; init; } = string.Empty;
    public bool DocumentationComplete { get; init; }
    public DateTime LastUpdated { get; init; }
    public string Version { get; init; } = string.Empty;
    public int EndpointCount { get; init; }
    public int ModelCount { get; init; }
    public Dictionary<string, bool> AdditionalChecks { get; init; } = new();
}

// MenuService specific models
public record SecurityDocumentation
{
    public SecurityAuthentication Authentication { get; init; } = new();
    public CorsConfiguration Cors { get; init; } = new();
    public PermissionRequirement[] Permissions { get; init; } = Array.Empty<PermissionRequirement>();
    public VaultInfo VaultIntegration { get; init; } = new();
}

public record SecurityAuthentication
{
    public string Type { get; init; } = string.Empty;
    public string TokenEndpoint { get; init; } = string.Empty;
    public string RefreshEndpoint { get; init; } = string.Empty;
    public string TokenLifetime { get; init; } = string.Empty;
    public string RefreshTokenLifetime { get; init; } = string.Empty;
}

public record CorsConfiguration
{
    public string Provider { get; init; } = string.Empty;
    public string ConfigPath { get; init; } = string.Empty;
    public string[] AllowedOrigins { get; init; } = Array.Empty<string>();
    public string[] AllowedMethods { get; init; } = Array.Empty<string>();
    public string[] AllowedHeaders { get; init; } = Array.Empty<string>();
}

public record PermissionRequirement
{
    public string Endpoint { get; init; } = string.Empty;
    public string Permission { get; init; } = string.Empty;
    public string Scope { get; init; } = string.Empty;
}

public record VaultInfo
{
    public string Provider { get; init; } = string.Empty;
    public string SecretsPath { get; init; } = string.Empty;
    public string[] ConfiguredSecrets { get; init; } = Array.Empty<string>();
}

public record GrpcDocumentation
{
    public string ServiceName { get; init; } = string.Empty;
    public string ProtoFile { get; init; } = string.Empty;
    public GrpcEndpoint[] Endpoints { get; init; } = Array.Empty<GrpcEndpoint>();
}

public record GrpcEndpoint
{
    public string Name { get; init; } = string.Empty;
    public string Request { get; init; } = string.Empty;
    public string Response { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public record HierarchyDocumentation
{
    public string Description { get; init; } = string.Empty;
    public string[] Features { get; init; } = Array.Empty<string>();
    public HierarchySchema Schema { get; init; } = new();
    public HierarchyExample[] Examples { get; init; } = Array.Empty<HierarchyExample>();
}

public record HierarchySchema
{
    public string ParentField { get; init; } = string.Empty;
    public string OrderField { get; init; } = string.Empty;
    public string MaxDepth { get; init; } = string.Empty;
    public string CycleDetection { get; init; } = string.Empty;
}

public record HierarchyExample
{
    public string Name { get; init; } = string.Empty;
    public string Structure { get; init; } = string.Empty;
}

public record MultiAppDocumentation
{
    public string Description { get; init; } = string.Empty;
    public string KeyField { get; init; } = string.Empty;
    public string[] Features { get; init; } = Array.Empty<string>();
    public string QueryPattern { get; init; } = string.Empty;
    public MultiAppExample[] Examples { get; init; } = Array.Empty<MultiAppExample>();
}

public record MultiAppExample
{
    public string Application { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int MenuCount { get; init; }
}

public record StateManagementDocumentation
{
    public string Description { get; init; } = string.Empty;
    public string StateField { get; init; } = string.Empty;
    public StateEndpoint[] Endpoints { get; init; } = Array.Empty<StateEndpoint>();
    public string QueryBehavior { get; init; } = string.Empty;
    public string[] UseCases { get; init; } = Array.Empty<string>();
}

public record StateEndpoint
{
    public string Name { get; init; } = string.Empty;
    public string Method { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Effect { get; init; } = string.Empty;
}

public record ExamplesDocumentation
{
    public UseCase[] UseCases { get; init; } = Array.Empty<UseCase>();
}

public record UseCase
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string[] Steps { get; init; } = Array.Empty<string>();
    public string Example { get; init; } = string.Empty;
}

public record MigrationGuide
{
    public string CurrentVersion { get; init; } = string.Empty;
    public BreakingChange[] BreakingChanges { get; init; } = Array.Empty<BreakingChange>();
    public VersionCompatibility[] CompatibilityMatrix { get; init; } = Array.Empty<VersionCompatibility>();
}

public record BreakingChange
{
    public string Version { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string[] Changes { get; init; } = Array.Empty<string>();
    public string[] MigrationSteps { get; init; } = Array.Empty<string>();
}

public record VersionCompatibility
{
    public string ClientVersion { get; init; } = string.Empty;
    public string[] ServerVersions { get; init; } = Array.Empty<string>();
    public bool Compatible { get; init; }
}

public record PerformanceHints
{
    public CachingStrategy Caching { get; init; } = new();
    public PaginationStrategy Pagination { get; init; } = new();
    public string[] QueryOptimization { get; init; } = Array.Empty<string>();
}

public record CachingStrategy
{
    public bool Recommended { get; init; }
    public string CacheDuration { get; init; } = string.Empty;
    public string CacheKey { get; init; } = string.Empty;
    public string[] InvalidationTriggers { get; init; } = Array.Empty<string>();
}

public record PaginationStrategy
{
    public bool Supported { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string Alternative { get; init; } = string.Empty;
}

public record IntegrationPatterns
{
    public IntegrationPattern[] Patterns { get; init; } = Array.Empty<IntegrationPattern>();
}

public record IntegrationPattern
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Implementation { get; init; } = string.Empty;
    public string[] Events { get; init; } = Array.Empty<string>();
    public string[] BestPractices { get; init; } = Array.Empty<string>();
    public string[] UseCases { get; init; } = Array.Empty<string>();
}

public record TroubleshootingGuide
{
    public TroubleshootingIssue[] CommonIssues { get; init; } = Array.Empty<TroubleshootingIssue>();
}

public record TroubleshootingIssue
{
    public string Problem { get; init; } = string.Empty;
    public string[] PossibleCauses { get; init; } = Array.Empty<string>();
    public string[] Solutions { get; init; } = Array.Empty<string>();
}