using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using MenuService.Tests.Fixtures;
using Xunit.Gherkin.Quick;
using System.Text.Json;
using System.Xml.Linq;
using YamlDotNet.Serialization;

namespace MenuService.Tests.Features.Documentation;

[FeatureFile("./Features/Documentation/AIDocumentation.feature")]
public sealed class AIDocumentationFeature : Feature, IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;
    private readonly HttpClient _client;
    private HttpResponseMessage? _response;
    private JsonDocument? _jsonResponse;
    private string? _responseContent;

    public AIDocumentationFeature(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    #region Given

    [Given(@"le service MenuService est démarré")]
    public void GivenLeServiceMenuServiceEstDemarre()
    {
        // Le service est déjà démarré via la fixture
        _client.Should().NotBeNull();
    }

    [Given(@"la documentation AI est activée")]
    public void GivenLaDocumentationAIEstActivee()
    {
        // La documentation AI devrait être activée par défaut dans l'environnement de test
        // Cette étape pourrait vérifier la configuration si nécessaire
    }

    #endregion

    #region When

    [When(@"je fais une requête GET sur ""(.*)""")]
    public async Task WhenJeFaisUneRequeteGETSur(string endpoint)
    {
        _response = await _client.GetAsync(endpoint);
        _responseContent = await _response.Content.ReadAsStringAsync();

        if (_response.IsSuccessStatusCode && _response.Content.Headers.ContentType?.MediaType == "application/json")
        {
            _jsonResponse = JsonDocument.Parse(_responseContent);
        }
    }

    #endregion

    #region Then

    [Then(@"la réponse a le code (\d+)")]
    public void ThenLaReponseALeCode(int statusCode)
    {
        ((int)_response!.StatusCode).Should().Be(statusCode);
    }

    [Then(@"la réponse contient un manifest JSON avec:")]
    public void ThenLaReponseContientUnManifestJSONAvec(Gherkin.Ast.DataTable dataTable)
    {
        _jsonResponse.Should().NotBeNull();
        var root = _jsonResponse!.RootElement;

        foreach (var row in dataTable.Rows.Skip(1))
        {
            var field = row.Cells.First().Value;
            var description = row.Cells.Last().Value;

            root.TryGetProperty(field, out var property).Should().BeTrue($"Le champ '{field}' devrait exister");

            switch (field)
            {
                case "name":
                    property.GetString().Should().Be("MenuService");
                    break;
                case "version":
                    property.GetString().Should().NotBeNullOrEmpty();
                    break;
                case "description":
                    property.GetString().Should().NotBeNullOrEmpty();
                    break;
                case "endpoints":
                    property.ValueKind.Should().Be(JsonValueKind.Array);
                    property.GetArrayLength().Should().BeGreaterThan(0);
                    break;
                case "documentation":
                    property.GetString().Should().StartWith("/api/docs/ai");
                    break;
            }
        }
    }

    [Then(@"la documentation contient les sections:")]
    public void ThenLaDocumentationContientLesSections(Gherkin.Ast.DataTable dataTable)
    {
        _jsonResponse.Should().NotBeNull();
        var root = _jsonResponse!.RootElement;

        foreach (var row in dataTable.Rows.Skip(1))
        {
            var section = row.Cells.First().Value;
            root.TryGetProperty(section, out _).Should().BeTrue($"La section '{section}' devrait exister");
        }
    }

    [Then(@"la documentation de l'endpoint contient:")]
    public void ThenLaDocumentationDeLEndpointContient(Gherkin.Ast.DataTable dataTable)
    {
        _jsonResponse.Should().NotBeNull();
        var root = _jsonResponse!.RootElement;

        foreach (var row in dataTable.Rows.Skip(1))
        {
            var field = row.Cells.First().Value;
            root.TryGetProperty(field.ToLowerInvariant(), out var property).Should().BeTrue($"Le champ '{field}' devrait exister");
        }
    }

    [Then(@"le schéma contient:")]
    public void ThenLeSchemaContient(Gherkin.Ast.DataTable dataTable)
    {
        _jsonResponse.Should().NotBeNull();
        var root = _jsonResponse!.RootElement;

        root.TryGetProperty("properties", out var properties).Should().BeTrue();

        foreach (var row in dataTable.Rows.Skip(1))
        {
            var propertyName = row.Cells.ElementAt(0).Value.ToLowerInvariant();
            var type = row.Cells.ElementAt(1).Value;
            var required = row.Cells.ElementAt(2).Value;

            properties.TryGetProperty(propertyName, out var property).Should().BeTrue($"La propriété '{propertyName}' devrait exister");

            if (property.TryGetProperty("type", out var typeProperty))
            {
                typeProperty.GetString().Should().Contain(type.Split(' ')[0].ToLowerInvariant());
            }
        }
    }

    [Then(@"la documentation contient:")]
    public void ThenLaDocumentationContient(Gherkin.Ast.DataTable dataTable)
    {
        _jsonResponse.Should().NotBeNull();
        var root = _jsonResponse!.RootElement;

        foreach (var row in dataTable.Rows.Skip(1))
        {
            var endpoint = row.Cells.ElementAt(0).Value;
            var method = row.Cells.ElementAt(1).Value;
            var path = row.Cells.ElementAt(2).Value;
            var description = row.Cells.ElementAt(3).Value;

            // Vérifier que l'endpoint existe dans la documentation
            root.TryGetProperty("endpoints", out var endpoints).Should().BeTrue();
            endpoints.EnumerateArray().Should().Contain(e =>
                e.TryGetProperty("name", out var n) && n.GetString() == endpoint &&
                e.TryGetProperty("method", out var m) && m.GetString() == method &&
                e.TryGetProperty("path", out var p) && p.GetString() == path,
                $"L'endpoint {method} {path} devrait être documenté");
        }
    }

    [Then(@"la liste des features contient:")]
    public void ThenLaListeDesFeaturesContient(Gherkin.Ast.DataTable dataTable)
    {
        _jsonResponse.Should().NotBeNull();
        var root = _jsonResponse!.RootElement;

        root.TryGetProperty("features", out var features).Should().BeTrue();
        features.ValueKind.Should().Be(JsonValueKind.Array);

        var featuresList = features.EnumerateArray().ToList();
        featuresList.Should().HaveCountGreaterThan(0);

        foreach (var row in dataTable.Rows.Skip(1))
        {
            var type = row.Cells.ElementAt(0).Value;
            var name = row.Cells.ElementAt(1).Value;

            featuresList.Should().Contain(f =>
                f.TryGetProperty("type", out var t) && t.GetString() == type &&
                f.TryGetProperty("name", out var n) && n.GetString() == name,
                $"La feature {type} {name} devrait exister");
        }
    }

    [Then(@"la documentation de sécurité contient:")]
    public void ThenLaDocumentationDeSecuriteContient(Gherkin.Ast.DataTable dataTable)
    {
        _jsonResponse.Should().NotBeNull();
        var root = _jsonResponse!.RootElement;

        foreach (var row in dataTable.Rows.Skip(1))
        {
            var aspect = row.Cells.First().Value;
            var description = row.Cells.Last().Value;

            root.TryGetProperty(aspect.ToLowerInvariant(), out var property).Should().BeTrue($"L'aspect '{aspect}' devrait exister");
        }
    }

    [Then(@"la documentation explique:")]
    public void ThenLaDocumentationExplique(Gherkin.Ast.DataTable dataTable)
    {
        _jsonResponse.Should().NotBeNull();
        var root = _jsonResponse!.RootElement;

        root.TryGetProperty("concepts", out var concepts).Should().BeTrue();

        foreach (var row in dataTable.Rows.Skip(1))
        {
            var concept = row.Cells.First().Value;
            concepts.TryGetProperty(concept, out _).Should().BeTrue($"Le concept '{concept}' devrait être expliqué");
        }
    }

    [Then(@"la documentation gRPC contient:")]
    public void ThenLaDocumentationGRPCContient(Gherkin.Ast.DataTable dataTable)
    {
        _jsonResponse.Should().NotBeNull();
        var root = _jsonResponse!.RootElement;

        root.TryGetProperty("grpc", out var grpc).Should().BeTrue();

        foreach (var row in dataTable.Rows.Skip(1))
        {
            var service = row.Cells.ElementAt(0).Value;
            grpc.TryGetProperty("services", out var services).Should().BeTrue();
            services.EnumerateArray().Should().Contain(s =>
                s.TryGetProperty("name", out var n) && n.GetString() == service,
                $"Le service gRPC '{service}' devrait être documenté");
        }
    }

    [Then(@"les exemples incluent:")]
    public void ThenLesExemplesIncluent(Gherkin.Ast.DataTable dataTable)
    {
        _jsonResponse.Should().NotBeNull();
        var root = _jsonResponse!.RootElement;

        root.TryGetProperty("examples", out var examples).Should().BeTrue();
        examples.ValueKind.Should().Be(JsonValueKind.Array);

        foreach (var row in dataTable.Rows.Skip(1))
        {
            var useCase = row.Cells.First().Value;
            examples.EnumerateArray().Should().Contain(e =>
                e.TryGetProperty("useCase", out var u) && u.GetString() == useCase,
                $"L'exemple '{useCase}' devrait exister");
        }
    }

    [Then(@"le guide contient:")]
    public void ThenLeGuideContient(Gherkin.Ast.DataTable dataTable)
    {
        _jsonResponse.Should().NotBeNull();
        var root = _jsonResponse!.RootElement;

        root.TryGetProperty("guide", out var guide).Should().BeTrue();

        foreach (var row in dataTable.Rows.Skip(1))
        {
            var version = row.Cells.ElementAt(0).Value;
            guide.TryGetProperty(version, out _).Should().BeTrue($"Le guide pour la version '{version}' devrait exister");
        }
    }

    [Then(@"les recommandations incluent:")]
    public void ThenLesRecommandationsIncluent(Gherkin.Ast.DataTable dataTable)
    {
        _jsonResponse.Should().NotBeNull();
        var root = _jsonResponse!.RootElement;

        root.TryGetProperty("recommendations", out var recommendations).Should().BeTrue();

        foreach (var row in dataTable.Rows.Skip(1))
        {
            var aspect = row.Cells.First().Value;
            recommendations.TryGetProperty(aspect.ToLowerInvariant(), out _).Should().BeTrue(
                $"Les recommandations pour '{aspect}' devraient exister");
        }
    }

    [Then(@"les patterns documentés incluent:")]
    public void ThenLesPatternsDocumentesIncluent(Gherkin.Ast.DataTable dataTable)
    {
        _jsonResponse.Should().NotBeNull();
        var root = _jsonResponse!.RootElement;

        root.TryGetProperty("patterns", out var patterns).Should().BeTrue();
        patterns.ValueKind.Should().Be(JsonValueKind.Array);

        foreach (var row in dataTable.Rows.Skip(1))
        {
            var pattern = row.Cells.ElementAt(0).Value;
            patterns.EnumerateArray().Should().Contain(p =>
                p.TryGetProperty("name", out var n) && n.GetString() == pattern,
                $"Le pattern '{pattern}' devrait être documenté");
        }
    }

    #endregion
}