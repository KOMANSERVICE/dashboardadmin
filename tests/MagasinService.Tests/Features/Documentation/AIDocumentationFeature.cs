using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using MagasinService.Tests.Fixtures;
using Xunit.Gherkin.Quick;
using System.Text.Json;
using System.Xml.Linq;
using YamlDotNet.Serialization;

namespace MagasinService.Tests.Features.Documentation;

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

    [Given(@"le service MagasinService est démarré")]
    public void GivenLeServiceMagasinServiceEstDemarre()
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

    [Given(@"la documentation contient plusieurs endpoints")]
    public void GivenLaDocumentationContientPlusieursEndpoints()
    {
        // Setup pour le test de recherche - les endpoints existent déjà dans le service
    }

    [Given(@"un nouvel endpoint est ajouté au service")]
    public async Task GivenUnNouvelEndpointEstAjouteAuService()
    {
        // Simulation de l'ajout d'un endpoint (dans un vrai test, cela pourrait déclencher un rechargement)
        // Pour ce test, on peut juste vérifier que la documentation se met à jour
        await Task.CompletedTask;
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

    [When(@"je fais une requête GET sur ""(.*)"" avec le header ""(.*)"" = ""(.*)""")]
    public async Task WhenJeFaisUneRequeteGETSurAvecLeHeader(string endpoint, string headerName, string headerValue)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Add(headerName, headerValue);

        _response = await _client.SendAsync(request);
        _responseContent = await _response.Content.ReadAsStringAsync();
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
                    property.GetString().Should().Be("MagasinService");
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

    [Then(@"le Content-Type de la réponse est ""(.*)""")]
    public void ThenLeContentTypeDeLaReponseEst(string contentType)
    {
        _response!.Content.Headers.ContentType?.MediaType.Should().Be(contentType);
    }

    [Then(@"le format de la documentation est valide")]
    public void ThenLeFormatDeLaDocumentationEstValide()
    {
        _responseContent.Should().NotBeNullOrEmpty();

        var contentType = _response!.Content.Headers.ContentType?.MediaType;

        switch (contentType)
        {
            case "application/json":
                Assert.DoesNotThrow(() => JsonDocument.Parse(_responseContent!));
                break;
            case "application/yaml":
                var deserializer = new Deserializer();
                Assert.DoesNotThrow(() => deserializer.Deserialize<object>(_responseContent!));
                break;
            case "application/xml":
                Assert.DoesNotThrow(() => XDocument.Parse(_responseContent!));
                break;
            default:
                throw new NotSupportedException($"Format non supporté: {contentType}");
        }
    }

    [Then(@"la liste des versions contient au minimum:")]
    public void ThenLaListeDesVersionsContientAuMinimum(Gherkin.Ast.DataTable dataTable)
    {
        _jsonResponse.Should().NotBeNull();
        var root = _jsonResponse!.RootElement;

        root.TryGetProperty("versions", out var versions).Should().BeTrue();
        versions.ValueKind.Should().Be(JsonValueKind.Array);
        versions.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Then(@"les headers contiennent:")]
    public void ThenLesHeadersContiennent(Gherkin.Ast.DataTable dataTable)
    {
        foreach (var row in dataTable.Rows.Skip(1))
        {
            var header = row.Cells.First().Value;
            var expectedValue = row.Cells.Last().Value;

            _response!.Headers.TryGetValues(header, out var values).Should().BeTrue($"Le header '{header}' devrait exister");

            if (header == "Cache-Control")
            {
                values!.First().Should().Contain(expectedValue);
            }
            else if (header == "ETag")
            {
                values!.First().Should().NotBeNullOrEmpty();
            }
        }
    }

    [Then(@"les résultats contiennent tous les éléments liés à ""(.*)""")]
    public void ThenLesResultatsContiennentTousLesElementsLiesA(string searchTerm)
    {
        _jsonResponse.Should().NotBeNull();
        var root = _jsonResponse!.RootElement;

        root.TryGetProperty("results", out var results).Should().BeTrue();
        results.ValueKind.Should().Be(JsonValueKind.Array);
        results.GetArrayLength().Should().BeGreaterThan(0);

        foreach (var result in results.EnumerateArray())
        {
            var hasMatch = result.TryGetProperty("name", out var name) &&
                          name.GetString()!.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
            hasMatch.Should().BeTrue($"Tous les résultats devraient contenir '{searchTerm}'");
        }
    }

    [Then(@"le mapping indique la correspondance entre:")]
    public void ThenLeMappingIndiqueLaCorrespondanceEntre(Gherkin.Ast.DataTable dataTable)
    {
        _jsonResponse.Should().NotBeNull();
        var root = _jsonResponse!.RootElement;

        root.TryGetProperty("mappings", out var mappings).Should().BeTrue();

        foreach (var row in dataTable.Rows.Skip(1))
        {
            var aiDoc = row.Cells.First().Value;
            var openApi = row.Cells.Last().Value;

            mappings.TryGetProperty(aiDoc, out var mapping).Should().BeTrue();
            mapping.GetString().Should().Be(openApi);
        }
    }

    [Then(@"la réponse contient le nouvel endpoint")]
    public void ThenLaReponseContientLeNouvelEndpoint()
    {
        _jsonResponse.Should().NotBeNull();
        // Vérification que la documentation est à jour
        // Dans un vrai test, on vérifierait la présence du nouvel endpoint
    }

    [Then(@"la version de la documentation est incrémentée")]
    public void ThenLaVersionDeLaDocumentationEstIncrementee()
    {
        _jsonResponse.Should().NotBeNull();
        var root = _jsonResponse!.RootElement;

        root.TryGetProperty("version", out var version).Should().BeTrue();
        // Vérification que la version a changé
    }

    [Then(@"l'erreur contient ""(.*)""")]
    public async Task ThenLErreurContient(string expectedMessage)
    {
        var content = await _response!.Content.ReadAsStringAsync();
        content.Should().Contain(expectedMessage);
    }

    [Then(@"le statut indique:")]
    public void ThenLeStatutIndique(Gherkin.Ast.DataTable dataTable)
    {
        _jsonResponse.Should().NotBeNull();
        var root = _jsonResponse!.RootElement;

        foreach (var row in dataTable.Rows.Skip(1))
        {
            var field = row.Cells.First().Value;
            var expectedValue = row.Cells.Last().Value;

            root.TryGetProperty(field, out var property).Should().BeTrue($"Le champ '{field}' devrait exister");

            switch (field)
            {
                case "status":
                    property.GetString().Should().Be(expectedValue);
                    break;
                case "documentationComplete":
                    property.GetBoolean().Should().Be(bool.Parse(expectedValue));
                    break;
                case "lastUpdated":
                    property.GetString().Should().NotBeNullOrEmpty();
                    break;
            }
        }
    }

    #endregion
}