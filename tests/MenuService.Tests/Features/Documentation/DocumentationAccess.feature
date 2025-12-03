@documentation @api @swagger
Feature: Accès à la documentation API de MenuService
    En tant que développeur
    Je veux accéder à la documentation API du service
    Afin de comprendre et utiliser les endpoints disponibles

Background:
    Given le service MenuService est démarré
    And la configuration OpenAPI est activée
    And le CORS est configuré

@smoke @critical
Scenario: Accès à la documentation Swagger UI
    When je navigue vers "/"
    Then je suis redirigé vers "/swagger"
    And la page Swagger UI est affichée
    And le titre contient "MenuService API"

@smoke
Scenario: Vérification des endpoints documentés
    Given je récupère la documentation OpenAPI
    Then les endpoints suivants sont documentés:
        | Method | Path                   | Description            |
        | GET    | /menu                  | GetAllMenu            |
        | GET    | /menu/actif           | GetAllActifMenu       |
        | POST   | /menu                  | CreateMenu            |
        | PUT    | /menu/{id}            | UpdateMenu            |
        | PATCH  | /menu/{id}/active     | ActiveMenu            |
        | PATCH  | /menu/{id}/inactive   | InactiveMenu          |

@validation
Scenario: Documentation des paramètres de requête
    Given je récupère la documentation OpenAPI
    When j'examine l'endpoint "PATCH /menu/{id}/active"
    Then le paramètre "id" est documenté comme:
        | Property    | Value       |
        | Type        | string      |
        | Format      | uuid        |
        | Required    | true        |
        | Description | ID du menu  |

@validation
Scenario: Documentation des modèles de données
    Given je récupère la documentation OpenAPI
    Then les modèles suivants sont documentés:
        | Model                  | Description                        |
        | MenuDTO               | Représente un menu de l'application|
        | CreateMenuRequest     | Données pour créer un menu         |
        | UpdateMenuRequest     | Données pour modifier un menu      |
        | MenuListResponse      | Liste des menus                    |

@security
Scenario: Documentation de l'authentification et autorisation
    Given je récupère la documentation OpenAPI
    Then les endpoints sont documentés comme nécessitant une authentification
    And le schéma de sécurité "Bearer" est défini
    And les rôles requis sont documentés pour chaque endpoint

@integration
Scenario: Documentation des codes de réponse
    Given je récupère la documentation OpenAPI
    When j'examine l'endpoint "POST /menu"
    Then les codes de réponse suivants sont documentés:
        | Code | Description                           |
        | 201  | Menu créé avec succès                 |
        | 400  | Données invalides                     |
        | 401  | Non authentifié                       |
        | 403  | Permissions insuffisantes             |
        | 409  | Menu avec ce nom existe déjà          |

@cors
Scenario: Documentation de la configuration CORS
    Given je récupère la documentation OpenAPI
    Then la documentation mentionne les origines autorisées
    And les méthodes HTTP autorisées sont listées
    And la configuration des en-têtes est documentée