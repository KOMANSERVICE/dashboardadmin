@api @documentation @ai
Feature: Documentation exploitable par AI pour MenuService
    En tant qu'agent AI ou système externe
    Je veux accéder à une documentation structurée du MenuService
    Afin de comprendre et utiliser automatiquement l'API

Background:
    Given le service MenuService est démarré
    And la documentation AI est activée

@smoke @happy-path
Scenario: Récupération du manifest de documentation AI
    When je fais une requête GET sur "/api/docs/ai/manifest"
    Then la réponse a le code 200
    And la réponse contient un manifest JSON avec:
        | Champ | Description |
        | name | "MenuService" |
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
        | models | Schémas des DTOs et modèles |
        | features | Features CQRS disponibles |
        | authentication | Méthodes d'authentification avec CORS |
        | errors | Codes d'erreur possibles |

@endpoint-details
Scenario: Documentation détaillée d'un endpoint de menu
    When je fais une requête GET sur "/api/docs/ai/endpoints/GetAllMenu"
    Then la réponse a le code 200
    And la documentation de l'endpoint contient:
        | Champ | Description |
        | method | "GET" |
        | path | "/menu/{appAdminReference}" |
        | parameters | Liste des paramètres avec types |
        | requestExample | Exemple de requête |
        | responseExample | Exemple de réponse avec MenuStateDto |
        | errorResponses | Réponses d'erreur possibles |
        | authorization | Besoins d'autorisation |

@models-schema
Scenario: Récupération des schémas de modèles Menu
    When je fais une requête GET sur "/api/docs/ai/models/MenuStateDto"
    Then la réponse a le code 200
    And le schéma contient:
        | Propriété | Type | Required | Description |
        | id | "string (guid)" | true | Identifiant unique |
        | nom | "string" | true | Nom du menu |
        | description | "string" | false | Description |
        | route | "string" | true | Route de navigation |
        | icon | "string" | false | Icône du menu |
        | ordre | "integer" | true | Ordre d'affichage |
        | isActive | "boolean" | true | Statut actif |
        | applicationId | "string (guid)" | true | ID de l'application |
        | parentMenuId | "string (guid)" | false | ID du menu parent |

@state-management-documentation
Scenario: Documentation des endpoints de gestion d'état
    When je fais une requête GET sur "/api/docs/ai/features/state-management"
    Then la réponse a le code 200
    And la documentation contient:
        | Endpoint | Method | Path | Description |
        | ActiveMenu | PATCH | /menu/{id}/active | Active un menu |
        | InactiveMenu | PATCH | /menu/{id}/inactive | Désactive un menu |

@cqrs-documentation
Scenario: Documentation des commandes et queries CQRS
    When je fais une requête GET sur "/api/docs/ai/features"
    Then la réponse a le code 200
    And la liste des features contient:
        | Type | Name | Description |
        | Command | CreateMenuCommand | Création d'un menu |
        | Command | UpdateMenuCommand | Mise à jour d'un menu |
        | Command | ActiveMenuCommand | Activation d'un menu |
        | Command | InactiveMenuCommand | Désactivation d'un menu |
        | Query | GetAllMenuQuery | Récupération de tous les menus |
        | Query | GetAllActifMenuQuery | Récupération des menus actifs |

@security-documentation
Scenario: Documentation des aspects sécurité et CORS
    When je fais une requête GET sur "/api/docs/ai/security"
    Then la réponse a le code 200
    And la documentation de sécurité contient:
        | Aspect | Description |
        | authentication | JWT Bearer token requis |
        | cors | Configuration CORS via Vault |
        | permissions | Permissions requises par endpoint |
        | vault | Intégration avec ISecureSecretProvider |

@hierarchy-documentation
Scenario: Documentation de la hiérarchie des menus
    When je fais une requête GET sur "/api/docs/ai/features/hierarchy"
    Then la réponse a le code 200
    And la documentation explique:
        | Concept | Description |
        | parentMenuId | Création de sous-menus |
        | ordre | Gestion de l'ordre d'affichage |
        | recursion | Support de hiérarchies multi-niveaux |

@grpc-documentation
Scenario: Documentation de l'interface gRPC
    When je fais une requête GET sur "/api/docs/ai/grpc"
    Then la réponse a le code 200
    And la documentation gRPC contient:
        | Service | Methods | Proto file |
        | MenuGrpcService | Liste des méthodes gRPC | menu.proto |

@multi-app-support
Scenario: Documentation du support multi-applications
    When je fais une requête GET sur "/api/docs/ai/features/multi-app"
    Then la réponse a le code 200
    And la documentation explique:
        | Feature | Description |
        | applicationId | Isolation des menus par application |
        | appAdminReference | Référence unique par application |
        | filtering | Filtrage automatique par application |

@examples-collection
Scenario: Collection d'exemples d'utilisation
    When je fais une requête GET sur "/api/docs/ai/examples"
    Then la réponse a le code 200
    And les exemples incluent:
        | Use Case | Description |
        | create-menu | Création d'un menu simple |
        | create-submenu | Création d'un sous-menu |
        | manage-state | Activation/désactivation |
        | bulk-operations | Opérations en masse |

@migration-guide
Scenario: Guide de migration depuis une version antérieure
    When je fais une requête GET sur "/api/docs/ai/migration"
    Then la réponse a le code 200
    And le guide contient:
        | Version | Breaking Changes | Migration Steps |
        | v1 to v2 | Liste des changements | Étapes de migration |

@performance-hints
Scenario: Documentation des bonnes pratiques de performance
    When je fais une requête GET sur "/api/docs/ai/performance"
    Then la réponse a le code 200
    And les recommandations incluent:
        | Aspect | Recommendation |
        | caching | Mise en cache des menus actifs |
        | pagination | Pagination pour grandes listes |
        | filtering | Filtrage côté serveur |

@integration-patterns
Scenario: Patterns d'intégration avec d'autres services
    When je fais une requête GET sur "/api/docs/ai/integration"
    Then la réponse a le code 200
    And les patterns documentés incluent:
        | Pattern | Description | Example |
        | event-driven | Événements de changement de menu | MenuUpdated |
        | sync-api | API synchrone REST | GET /menu |
        | async-grpc | Communication gRPC asynchrone | MenuGrpcService |

@troubleshooting
Scenario: Guide de dépannage intégré
    When je fais une requête GET sur "/api/docs/ai/troubleshooting"
    Then la réponse a le code 200
    And le guide contient:
        | Problem | Possible Causes | Solutions |
        | 404 on menu | Menu non trouvé | Vérifier l'ID |
        | 401 error | Token invalide | Renouveler le token |
        | CORS blocked | Origine non autorisée | Configurer Vault |