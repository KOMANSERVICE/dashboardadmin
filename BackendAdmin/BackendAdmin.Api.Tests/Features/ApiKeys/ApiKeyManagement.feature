# language: fr
@issue-32 @backend @frontend @api-key @security
Fonctionnalité: Interface de gestion des API Keys
  En tant qu administrateur de FrontendAdmin/BackendAdmin
  Je veux avoir un ecran pour generer et gerer les API Keys
  Afin de securiser l acces aux microservices avec IDR.Library.BuildingBlocks

  Contexte:
    Etant donne un administrateur authentifie avec le role "DashbordAdmin"
    Et l application "MonApp" avec la reference "app-123"

  # ==========================================
  # BACKEND - Domain Layer
  # ==========================================

  @domain @entity
  Scenario: Creation de l entite ApiKey dans le Domain
    Etant donne le package IDR.Library.BuildingBlocks v3.1.4
    Quand je cree l entite ApiKey dans BackendAdmin.Domain
    Alors l entite doit implementer l interface "IApiKey"
    Et l entite doit avoir les proprietes:
      | Propriete       | Type      | Description                      |
      | Id              | Guid      | Identifiant unique               |
      | ApiKeyHash      | string    | Hash SHA-256 de la cle           |
      | ApplicationId   | string    | Reference de l application       |
      | ApplicationName | string    | Nom de l application             |
      | Scopes          | string    | Scopes separes par virgules      |
      | CreatedAt       | DateTime  | Date de creation                 |
      | ExpiresAt       | DateTime? | Date d expiration optionnelle    |
      | IsRevoked       | bool      | Indicateur de revocation         |
      | RevokedReason   | string?   | Raison de revocation             |
      | RevokedAt       | DateTime? | Date de revocation               |
      | LastUsedAt      | DateTime? | Derniere utilisation             |
    Et la table doit etre nommee "TA00002"

  # ==========================================
  # BACKEND - Infrastructure Layer
  # ==========================================

  @infrastructure @factory
  Scenario: Implementation de IApiKeyFactory
    Etant donne l interface IApiKeyFactory de IDR.Library.BuildingBlocks
    Quand je cree la classe ApiKeyFactory dans BackendAdmin.Infrastructure
    Alors la factory doit implementer IApiKeyFactory
    Et la methode CreateApiKey doit retourner une instance d ApiKey

  @infrastructure @configuration
  Scenario: Configuration EF Core pour ApiKey
    Quand je cree ApiKeyConfiguration dans BackendAdmin.Infrastructure
    Alors la configuration doit:
      | Regle                                    |
      | Definir la cle primaire sur Id           |
      | Creer un index unique sur ApiKeyHash     |
      | Creer un index sur ApplicationId         |
      | Limiter ApiKeyHash a 64 caracteres       |
      | Limiter Scopes a 1000 caracteres         |

  @infrastructure @migration
  Scenario: Migration EF Core pour la table ApiKey
    Quand je genere la migration "AddApiKeyEntity"
    Alors la migration doit creer la table "TA00002"
    Et la migration doit etre sure pour la production

  @infrastructure @dbcontext
  Scenario: Ajout du DbSet ApiKeys au contexte
    Quand je modifie ApplicationDbContext
    Alors il doit exposer "DbSet ApiKeys"

  # ==========================================
  # BACKEND - Application Layer (CQRS)
  # ==========================================

  @application @command
  Scenario: Command CreateApiKey
    Etant donne la commande CreateApiKeyCommand avec:
      | Champ           | Type     |
      | ApplicationId   | string   |
      | ApplicationName | string   |
      | Scopes          | string[] |
      | ExpiresAt       | DateTime?|
    Quand le handler CreateApiKeyHandler traite la commande
    Alors il doit utiliser IApiKeyService.CreateApiKeyAsync
    Et retourner CreateApiKeyResult avec:
      | Champ       | Description                                |
      | ApiKey      | Cle en clair (64 chars) - UNE SEULE FOIS   |
      | ApiKeyHash  | Hash SHA-256 pour reference                |
      | CreatedAt   | Date de creation                           |
      | ExpiresAt   | Date d expiration                          |

  @application @command
  Scenario: Command RevokeApiKey
    Etant donne la commande RevokeApiKeyCommand avec:
      | Champ         | Type   |
      | ApiKeyHash    | string |
      | RevokedReason | string |
    Quand le handler RevokeApiKeyHandler traite la commande
    Alors il doit utiliser IApiKeyService.RevokeApiKeyAsync
    Et la cle doit etre marquee IsRevoked=true

  @application @command
  Scenario: Command RotateApiKey
    Etant donne la commande RotateApiKeyCommand avec:
      | Champ           | Type |
      | ApiKeyHash      | string |
      | GracePeriodDays | int    |
    Quand le handler RotateApiKeyHandler traite la commande
    Alors il doit utiliser IApiKeyService.RotateApiKeyAsync
    Et retourner la nouvelle cle (ancienne reste valide pendant la periode de grace)

  @application @query
  Scenario: Query GetApiKeysByApplication
    Etant donne la query GetApiKeysByApplicationQuery avec ApplicationId "app-123"
    Quand le handler GetApiKeysByApplicationHandler traite la query
    Alors il doit utiliser IApiKeyService.GetApiKeysByApplicationAsync
    Et retourner la liste des ApiKeyInfo (SANS les cles en clair)

  # ==========================================
  # BACKEND - API Layer (Endpoints)
  # ==========================================

  @api @endpoint @post
  Scenario: Endpoint POST /apikey - Creer une API Key
    Etant donne l administrateur authentifie via JWT
    Quand j envoie une requete POST vers "/apikey" avec:
      """json
      {
        "applicationId": "app-123",
        "applicationName": "MonApp",
        "scopes": ["menu:read", "menu:write"],
        "expiresAt": "2025-12-31T23:59:59Z"
      }
      """
    Alors la reponse doit avoir le statut 201 Created
    Et la reponse doit contenir la cle API en clair (64 caracteres)
    Et un avertissement doit indiquer que la cle ne sera plus affichee

  @api @endpoint @get
  Scenario: Endpoint GET /apikey/{applicationId} - Lister les API Keys
    Etant donne l administrateur authentifie via JWT
    Et l application "app-123" possede 3 API Keys
    Quand j envoie une requete GET vers "/apikey/app-123"
    Alors la reponse doit avoir le statut 200 OK
    Et la reponse doit contenir 3 ApiKeyInfo
    Et les reponses NE doivent PAS contenir les cles en clair

  @api @endpoint @delete
  Scenario: Endpoint DELETE /apikey/{hash} - Revoquer une API Key
    Etant donne l administrateur authentifie via JWT
    Et une API Key avec le hash "abc123..."
    Quand j envoie une requete DELETE vers "/apikey/abc123" avec:
      """json
      {
        "reason": "Cle compromise"
      }
      """
    Alors la reponse doit avoir le statut 200 OK
    Et la cle doit etre revoquee

  @api @endpoint @post
  Scenario: Endpoint POST /apikey/{hash}/rotate - Rotation d API Key
    Etant donne l administrateur authentifie via JWT
    Et une API Key avec le hash "abc123..."
    Quand j envoie une requete POST vers "/apikey/abc123/rotate" avec:
      """json
      {
        "gracePeriodDays": 7
      }
      """
    Alors la reponse doit avoir le statut 200 OK
    Et une nouvelle cle doit etre generee
    Et l ancienne cle reste valide pendant 7 jours

  @api @security
  Scenario: Les endpoints ApiKey requierent l authentification JWT
    Etant donne aucun token JWT
    Quand j envoie une requete GET vers "/apikey/app-123"
    Alors la reponse doit avoir le statut 401 Unauthorized

  # ==========================================
  # FRONTEND - Pages et Composants
  # ==========================================

  @frontend @page
  Scenario: Page de liste des API Keys
    Etant donne l administrateur sur la page "/apikeys"
    Alors je dois voir une table avec les colonnes:
      | Colonne         | Description                    |
      | Nom             | Nom de l application           |
      | Scopes          | Liste des scopes               |
      | Creee le        | Date de creation               |
      | Expire le       | Date d expiration              |
      | Derniere usage  | Date de derniere utilisation   |
      | Statut          | Active/Revoquee                |
      | Actions         | Revoquer, Rotation             |
    Et un bouton "Creer une API Key"

  @frontend @page
  Scenario: Page de creation d API Key
    Etant donne l administrateur sur la page "/apikeys/create"
    Alors je dois voir un formulaire avec:
      | Champ           | Type        | Requis |
      | Application     | Select      | Oui    |
      | Nom             | TextInput   | Oui    |
      | Scopes          | MultiSelect | Oui    |
      | Date expiration | DatePicker  | Non    |
    Et un bouton "Generer la cle"

  @frontend @component
  Scenario: Affichage de la cle generee (une seule fois)
    Etant donne une nouvelle API Key generee
    Alors je dois voir une modale avec:
      | Element                                              |
      | La cle API complete (64 caracteres)                  |
      | Un bouton "Copier"                                   |
      | Un avertissement "Cette cle ne sera plus affichee"   |
      | Un bouton "J ai copie la cle" pour fermer            |

  @frontend @component
  Scenario: Confirmation de revocation
    Etant donne une API Key active dans la liste
    Quand je clique sur "Revoquer"
    Alors une modale de confirmation doit s afficher
    Et je dois pouvoir saisir une raison de revocation

  # ==========================================
  # INTEGRATION IDR.Library.BuildingBlocks
  # ==========================================

  @integration @dependency-injection
  Scenario: Enregistrement des services API Key
    Quand je configure DependencyInjection dans BackendAdmin.Infrastructure
    Alors les services suivants doivent etre enregistres:
      | Service        | Implementation |
      | IApiKeyFactory | ApiKeyFactory  |
    Et services.AddApiKeyAuthentication() doit etre appele
