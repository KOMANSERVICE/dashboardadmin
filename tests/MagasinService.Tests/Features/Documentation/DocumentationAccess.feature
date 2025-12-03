@documentation @api @swagger
Feature: Accès à la documentation API de MagasinService
    En tant que développeur
    Je veux accéder à la documentation API du service
    Afin de comprendre et utiliser les endpoints disponibles

Background:
    Given le service MagasinService est démarré
    And la configuration OpenAPI est activée

@smoke @critical
Scenario: Accès à la documentation Swagger UI
    When je navigue vers "/openapi/v1.json"
    Then la réponse a le code 200
    And le contenu est un document OpenAPI valide
    And le titre contient "MagasinService API"

@smoke
Scenario: Vérification des endpoints documentés
    Given je récupère la documentation OpenAPI
    Then les endpoints suivants sont documentés:
        | Method | Path                    | Description                |
        | GET    | /magasin/{BoutiqueId}  | GetAllMagasin             |
        | POST   | /magasin               | CreateMagasin             |
        | GET    | /magasin/{id}          | GetOneMagasin             |
        | PUT    | /magasin/{id}          | UpdateMagasin             |

@validation
Scenario: Documentation des paramètres de requête
    Given je récupère la documentation OpenAPI
    When j'examine l'endpoint "GET /magasin/{BoutiqueId}"
    Then le paramètre "BoutiqueId" est documenté comme:
        | Property    | Value       |
        | Type        | string      |
        | Format      | uuid        |
        | Required    | true        |
        | Description | ID de la boutique |

@validation
Scenario: Documentation des modèles de données
    Given je récupère la documentation OpenAPI
    Then les modèles suivants sont documentés:
        | Model                   | Description                          |
        | StockLocationDTO        | Représente un emplacement de stock  |
        | CreateMagasinRequest    | Données pour créer un magasin        |
        | UpdateMagasinRequest    | Données pour modifier un magasin     |
        | GetAllMagasinResponse   | Liste des magasins                   |

@security
Scenario: Documentation de l'authentification
    Given je récupère la documentation OpenAPI
    Then les endpoints sont documentés comme nécessitant une authentification
    And le schéma de sécurité "Bearer" est défini
    And la description mentionne l'utilisation du JWT

@integration
Scenario: Génération automatique depuis les annotations
    Given le code source utilise les attributs OpenAPI
    When le service démarre
    Then la documentation est générée automatiquement
    And les commentaires XML sont inclus dans les descriptions