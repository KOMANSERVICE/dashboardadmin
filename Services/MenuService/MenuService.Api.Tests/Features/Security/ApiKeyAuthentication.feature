Feature: API Key Security for MenuService
  As a service administrator
  I want to secure the MenuService API with API Keys
  So that only authorized applications can access menu endpoints

  Background:
    Given the MenuService is running
    And IDR.Library.BuildingBlocks v3.1.4 is installed with API Key Authentication support
    And a valid API Key has been created with appropriate scopes

  # ============================================
  # CONFIGURATION SCENARIOS
  # ============================================

  @configuration
  Scenario: API Key Authentication middleware is configured
    Given the DependencyInjection.cs file in MenuService.Api
    When the application starts
    Then the following services should be registered:
      | Service                 | Description                    |
      | IApiKeyService          | API Key management service     |
      | IApiKeyFactory          | Factory for creating ApiKey    |
      | IServiceContextService  | Service context accessor       |
    And the middleware pipeline should include:
      | Order | Middleware                    |
      | 1     | UseAuthentication             |
      | 2     | UseApiKeyAuthentication       |
      | 3     | UseServiceContext             |
      | 4     | UseAuthorization              |

  @configuration @domain
  Scenario: ApiKey entity is created in Domain layer
    Given the MenuService.Domain project
    When I check the Models folder
    Then an ApiKey entity should exist implementing IApiKey interface
    And it should have the following properties:
      | Property        | Type       | Description                          |
      | Id              | Guid       | Primary key                          |
      | ApiKeyHash      | string     | SHA-256 hash of the API key          |
      | ApplicationId   | string     | Identifier of the calling app        |
      | ApplicationName | string     | Name of the calling application      |
      | Scopes          | string     | Comma-separated list of scopes       |
      | CreatedAt       | DateTime   | Creation timestamp                   |
      | ExpiresAt       | DateTime?  | Optional expiration date             |
      | IsRevoked       | bool       | Revocation status                    |
      | RevokedReason   | string?    | Reason for revocation                |
      | RevokedAt       | DateTime?  | Revocation timestamp                 |
      | LastUsedAt      | DateTime?  | Last usage timestamp                 |

  @configuration @infrastructure
  Scenario: ApiKeyFactory is implemented in Infrastructure layer
    Given the MenuService.Infrastructure project
    When I check the Security folder
    Then an ApiKeyFactory class should exist implementing IApiKeyFactory
    And it should create ApiKey instances from the provided parameters

  @configuration @database
  Scenario: Database migration is created for ApiKey table
    Given the MenuService.Infrastructure project
    When a migration is generated
    Then a new table "ApiKeys" should be created with all ApiKey properties
    And the migration should be safe for production (no data loss)

  # ============================================
  # SCOPE PROTECTION SCENARIOS
  # ============================================

  @security @scopes
  Scenario Outline: Endpoints are protected with appropriate scopes
    Given the endpoint "<Endpoint>" with method "<Method>"
    When the endpoint is configured
    Then it should require the scope "<RequiredScope>"

    Examples:
      | Method | Endpoint                           | RequiredScope    |
      | GET    | /menu/{appAdminReference}          | menu:read        |
      | GET    | /menu/{appAdminReference}/actif    | menu:read        |
      | POST   | /menu                              | menu:write       |
      | PUT    | /menu                              | menu:write       |
      | PATCH  | /menu/active                       | menu:admin       |
      | PATCH  | /menu/inactive                     | menu:admin       |

  # ============================================
  # AUTHENTICATION SCENARIOS
  # ============================================

  @security @authentication
  Scenario: Request with valid API Key is authenticated
    Given an API Key "valid-test-key-64chars" with scopes "menu:read,menu:write"
    And the API Key is not expired and not revoked
    When I send a GET request to "/menu/test-app" with header "X-API-Key: valid-test-key-64chars"
    Then the response status should be 200
    And the ServiceContextService should return:
      | Property        | Value           |
      | ApplicationId   | test-app-id     |
      | ApplicationName | Test App        |
      | IsAuthenticated | true            |

  @security @authentication
  Scenario: Request without API Key header returns 401
    Given no API Key header is provided
    When I send a GET request to "/menu/test-app"
    Then the response status should be 401
    And the response body should contain "API Key is required"

  @security @authentication
  Scenario: Request with invalid API Key returns 401
    Given an invalid API Key "invalid-key-that-does-not-exist"
    When I send a GET request to "/menu/test-app" with header "X-API-Key: invalid-key-that-does-not-exist"
    Then the response status should be 401
    And the response body should contain "Invalid API Key"

  @security @authentication
  Scenario: Request with expired API Key returns 401
    Given an API Key that has expired
    When I send a GET request to "/menu/test-app" with header "X-API-Key: expired-key"
    Then the response status should be 401
    And the response body should contain "API Key has expired"

  @security @authentication
  Scenario: Request with revoked API Key returns 401
    Given an API Key that has been revoked
    When I send a GET request to "/menu/test-app" with header "X-API-Key: revoked-key"
    Then the response status should be 401
    And the response body should contain "API Key has been revoked"

  # ============================================
  # AUTHORIZATION SCENARIOS
  # ============================================

  @security @authorization
  Scenario: Request with insufficient scopes returns 403
    Given an API Key with only scope "menu:read"
    When I send a POST request to "/menu" with header "X-API-Key: read-only-key"
    Then the response status should be 403
    And the response body should contain "Insufficient scopes"

  @security @authorization
  Scenario: Request with correct scope is authorized
    Given an API Key with scopes "menu:read,menu:write"
    When I send a POST request to "/menu" with valid menu data and header "X-API-Key: write-key"
    Then the response status should be 201
    And the menu should be created

  @security @authorization
  Scenario: RequireScope with ScopeMatchMode.Any accepts any matching scope
    Given an API Key with scope "menu:admin"
    And an endpoint requiring ScopeMatchMode.Any with scopes "menu:write" and "menu:admin"
    When I send a request to that endpoint
    Then the request should be authorized

  @security @authorization
  Scenario: RequireScope with ScopeMatchMode.All requires all scopes
    Given an API Key with only scope "menu:write"
    And an endpoint requiring ScopeMatchMode.All with scopes "menu:write" and "menu:admin"
    When I send a request to that endpoint
    Then the response status should be 403

  # ============================================
  # LAST USED TRACKING
  # ============================================

  @tracking
  Scenario: LastUsedAt is updated automatically on each request
    Given a valid API Key created at "2024-01-01"
    And LastUsedAt is null
    When I send a request with this API Key
    Then the LastUsedAt field should be updated to the current timestamp
