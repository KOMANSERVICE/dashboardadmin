@api @documentation @ai
Feature: Documentation exploitable par AI pour MagasinService
    En tant qu'agent AI ou système externe
    Je veux accéder à une documentation structurée du MagasinService
    Afin de comprendre et utiliser automatiquement l'API

Background:
    Given le service MagasinService est démarré
    And la documentation AI est activée

@smoke @happy-path
Scenario: Récupération du manifest de documentation AI
    When je fais une requête GET sur "/api/docs/ai/manifest"
    Then la réponse a le code 200
    And la réponse contient un manifest JSON avec:
        | Champ | Description |
        | name | "MagasinService" |
        | version | Version du service |
        | description | Description du service |
        | endpoints | Liste des endpoints disponibles |
        | documentation | URL de la documentation complète |

@documentation-structure
Scenario: Structure de la documentation AI complète
    When je fais une requête GET sur "/api/docs/ai"
    Then la réponse a le code 200
    And la documentation contient les sections:
        | Section | Description |
        | service | Informations générales du service |
        | architecture | Structure et patterns utilisés |
        | endpoints | Liste détaillée des endpoints |
        | models | Schémas des DTOs et entités |
        | features | Features CQRS disponibles |
        | authentication | Méthodes d'authentification |
        | errors | Codes d'erreur possibles |

@endpoint-details
Scenario: Documentation détaillée d'un endpoint spécifique
    When je fais une requête GET sur "/api/docs/ai/endpoints/GetAllMagasin"
    Then la réponse a le code 200
    And la documentation de l'endpoint contient:
        | Champ | Description |
        | method | "GET" |
        | path | "/magasin/{BoutiqueId}" |
        | parameters | Liste des paramètres avec types |
        | requestExample | Exemple de requête |
        | responseExample | Exemple de réponse |
        | errorResponses | Réponses d'erreur possibles |
        | authorization | Besoins d'autorisation |

@models-schema
Scenario: Récupération des schémas de modèles
    When je fais une requête GET sur "/api/docs/ai/models/StockLocationDTO"
    Then la réponse a le code 200
    And le schéma contient:
        | Propriété | Type | Required | Description |
        | id | "string (guid)" | true | Identifiant unique |
        | nom | "string" | true | Nom du magasin |
        | description | "string" | false | Description |
        | adresse | "string" | false | Adresse |
        | isActive | "boolean" | true | Statut actif |

@cqrs-documentation
Scenario: Documentation des commandes et queries CQRS
    When je fais une requête GET sur "/api/docs/ai/features"
    Then la réponse a le code 200
    And la liste des features contient:
        | Type | Name | Description |
        | Command | CreateMagasinCommand | Création d'un magasin |
        | Command | UpdateMagasinCommand | Mise à jour d'un magasin |
        | Query | GetAllMagasinQuery | Récupération des magasins |
        | Query | GetOneMagasinQuery | Récupération d'un magasin |

@format-options
Scenario Outline: Support de différents formats de documentation
    When je fais une requête GET sur "/api/docs/ai" avec le header "Accept" = "<format>"
    Then la réponse a le code 200
    And le Content-Type de la réponse est "<contentType>"
    And le format de la documentation est valide

Examples:
    | format | contentType |
    | application/json | application/json |
    | application/yaml | application/yaml |
    | application/xml | application/xml |

@versioning
Scenario: Versioning de la documentation AI
    When je fais une requête GET sur "/api/docs/ai/versions"
    Then la réponse a le code 200
    And la liste des versions contient au minimum:
        | Version | Status | ReleaseDate |
        | v1 | current | Date actuelle |

@cache-control
Scenario: Headers de cache appropriés pour la documentation
    When je fais une requête GET sur "/api/docs/ai/manifest"
    Then la réponse a le code 200
    And les headers contiennent:
        | Header | Valeur |
        | Cache-Control | public, max-age=3600 |
        | ETag | Hash du contenu |

@search-capability
Scenario: Recherche dans la documentation AI
    Given la documentation contient plusieurs endpoints
    When je fais une requête GET sur "/api/docs/ai/search?q=magasin"
    Then la réponse a le code 200
    And les résultats contiennent tous les éléments liés à "magasin"

@integration-openapi
Scenario: Compatibilité avec la documentation OpenAPI existante
    When je fais une requête GET sur "/api/docs/ai/openapi-mapping"
    Then la réponse a le code 200
    And le mapping indique la correspondance entre:
        | Documentation AI | OpenAPI |
        | endpoints | paths |
        | models | components/schemas |
        | authentication | components/securitySchemes |

@realtime-updates
Scenario: Documentation mise à jour en temps réel
    Given un nouvel endpoint est ajouté au service
    When je fais une requête GET sur "/api/docs/ai/manifest"
    Then la réponse contient le nouvel endpoint
    And la version de la documentation est incrémentée

@error-handling
Scenario: Gestion des erreurs pour documentation non trouvée
    When je fais une requête GET sur "/api/docs/ai/endpoints/NonExistentEndpoint"
    Then la réponse a le code 404
    And l'erreur contient "Documentation not found for endpoint: NonExistentEndpoint"

@health-check
Scenario: Vérification de la santé du service de documentation
    When je fais une requête GET sur "/api/docs/ai/health"
    Then la réponse a le code 200
    And le statut indique:
        | Champ | Valeur |
        | status | "healthy" |
        | documentationComplete | true |
        | lastUpdated | Timestamp récent |