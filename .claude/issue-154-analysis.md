# Analyse Issue #154 - Gestion des Reseaux Docker Swarm

## Resume

L'issue #154 demande l'implementation de la gestion des reseaux Docker Swarm avec les fonctionnalites suivantes:

| Fonctionnalite | Description |
|----------------|-------------|
| Lister les reseaux | Tous les reseaux (overlay, bridge) |
| Creer un reseau | Nouveau reseau overlay |
| Supprimer un reseau | Retirer un reseau |
| Inspecter un reseau | Conteneurs connectes, config |
| Connecter un conteneur | Ajouter a un reseau |
| Deconnecter un conteneur | Retirer d'un reseau |
| Pruner les reseaux | Supprimer les reseaux inutilises |

## Classification

- **Type**: Full Stack (BackendAdmin + FrontendAdmin)
- **Scope**: Docker Swarm Networks
- **Coherence**: VALIDE - Suit les patterns existants (Services, Nodes, Volumes)

## Analyse Backend

### Structure existante Docker Swarm

L'architecture Docker Swarm est deja bien etablie avec:
- **11 Commands**: CreateService, DeleteService, UpdateService, RestartService, ScaleService, RollbackService, CreateVolume, DeleteVolume, PruneVolumes, BackupVolume, RestoreVolume
- **8 Queries**: GetSwarmServices, GetServiceDetails, GetServiceLogs, GetServiceTasks, GetNodes, GetVolumes, GetVolumeDetails, GetUnusedVolumes
- **19 Endpoints**: Routes API completes pour Services, Nodes, Volumes

### Service Docker existant

`IDockerSwarmService` dans `BackendAdmin.Application/Services/` encapsule les appels Docker.DotNet.

### Structure proposee pour Networks

```
BackendAdmin.Application/Features/Swarm/
├── Commands/
│   ├── CreateNetwork/
│   │   ├── CreateNetworkCommand.cs
│   │   ├── CreateNetworkHandler.cs
│   │   └── CreateNetworkValidator.cs
│   ├── DeleteNetwork/
│   ├── ConnectServiceToNetwork/
│   ├── DisconnectServiceFromNetwork/
│   └── PruneNetworks/
├── Queries/
│   ├── GetNetworks/
│   ├── GetNetworkDetails/
│   └── GetUnusedNetworks/
└── DTOs/
    ├── NetworkDTO.cs
    ├── NetworkDetailsDTO.cs
    ├── CreateNetworkRequest.cs
    └── ConnectServiceRequest.cs

BackendAdmin.Api/Endpoints/Swarm/
├── GetNetworks.cs
├── GetNetworkDetails.cs
├── CreateNetwork.cs
├── DeleteNetwork.cs
├── ConnectServiceToNetwork.cs
├── DisconnectServiceFromNetwork.cs
└── PruneNetworks.cs
```

### Routes API proposees

```
GET     /api/swarm/networks              (GetNetworks)
GET     /api/swarm/networks/{name}       (GetNetworkDetails)
POST    /api/swarm/networks              (CreateNetwork)
DELETE  /api/swarm/networks/{name}       (DeleteNetwork)
POST    /api/swarm/networks/{name}/connect    (ConnectServiceToNetwork)
POST    /api/swarm/networks/{name}/disconnect (DisconnectServiceFromNetwork)
POST    /api/swarm/networks/prune        (PruneNetworks)
```

### DTOs

```csharp
public record NetworkDTO(
    string Id,
    string Name,
    string Driver,
    string Scope,
    bool EnableIPv6,
    int ServiceCount,
    DateTime CreatedAt
);

public record NetworkDetailsDTO(
    string Id,
    string Name,
    string Driver,
    string Scope,
    bool EnableIPv6,
    Dictionary<string, string> Options,
    Dictionary<string, string> Labels,
    string? Subnet,
    string? Gateway,
    List<string> ConnectedServices,
    List<NetworkContainerDTO> Containers,
    DateTime CreatedAt
);

public record NetworkContainerDTO(
    string ContainerId,
    string Name,
    string IPv4Address
);

public record CreateNetworkRequest(
    string Name,
    string Driver = "overlay",
    bool EnableIPv6 = false,
    Dictionary<string, string>? Options = null,
    Dictionary<string, string>? Labels = null,
    string? Subnet = null,
    string? IPRange = null,
    string? Gateway = null
);

public record ConnectServiceRequest(string ServiceName);

public record DisconnectServiceRequest(string ServiceName);

public record PruneNetworksResponse(
    int DeletedCount,
    List<string> DeletedNetworks
);
```

## Analyse Frontend

### Structure existante

La page `SwarmPage.razor` utilise un systeme de tabs pour naviguer entre:
- Tab 0: Services
- Tab 1: Nodes
- Tab 2: Volumes
- **Tab 3: Networks (a ajouter)**

### Composants a creer

```
FrontendAdmin.Shared/Pages/Swarm/Components/
├── NetworkListComponent.razor          # Tableau des reseaux
├── NetworkDetailsComponent.razor       # Modal details avec services connectes
├── CreateNetworkComponent.razor        # Modal creation reseau
├── ConnectServiceComponent.razor       # Modal connexion service
└── DisconnectServiceComponent.razor    # Modal deconnexion (ou confirm dialog)
```

### Modeles a ajouter (SwarmModels.cs)

```csharp
public record NetworkDto(
    string Id,
    string Name,
    string Driver,
    string Scope,
    DateTime CreatedAt,
    int ServiceCount,
    bool IsUsed
);

public record NetworkDetailsDto(
    string Id,
    string Name,
    string Driver,
    string Scope,
    DateTime CreatedAt,
    Dictionary<string, string> Labels,
    Dictionary<string, string> Options,
    string? Subnet,
    string? Gateway,
    List<string> ConnectedServices,
    List<string> ConnectedContainers
);
```

### Service HTTP (ISwarmHttpService.cs)

```csharp
// Networks
[Get("/api/swarm/networks")]
Task<BaseResponse<GetNetworksResponse>> GetNetworksAsync();

[Get("/api/swarm/networks/{name}")]
Task<BaseResponse<GetNetworkDetailsResponse>> GetNetworkDetailsAsync(string name);

[Post("/api/swarm/networks")]
Task<BaseResponse<CreateNetworkResponse>> CreateNetworkAsync([Body] CreateNetworkRequest request);

[Delete("/api/swarm/networks/{name}")]
Task<BaseResponse<object>> DeleteNetworkAsync(string name);

[Post("/api/swarm/networks/{name}/connect")]
Task<BaseResponse<object>> ConnectServiceToNetworkAsync(string name, [Body] ConnectServiceRequest request);

[Post("/api/swarm/networks/{name}/disconnect")]
Task<BaseResponse<object>> DisconnectServiceFromNetworkAsync(string name, [Body] DisconnectServiceRequest request);

[Post("/api/swarm/networks/prune")]
Task<BaseResponse<PruneNetworksResponse>> PruneNetworksAsync();
```

## Methodes a ajouter a IDockerSwarmService

```csharp
// Networks
Task<IList<NetworkResponse>> GetNetworksAsync(CancellationToken cancellationToken = default);
Task<NetworkResponse?> GetNetworkByNameAsync(string name, CancellationToken cancellationToken = default);
Task<NetworkResponse?> GetNetworkByIdAsync(string id, CancellationToken cancellationToken = default);
Task<string> CreateNetworkAsync(CreateNetworkRequest request, CancellationToken cancellationToken = default);
Task DeleteNetworkAsync(string networkName, CancellationToken cancellationToken = default);
Task<IList<string>> GetNetworkServicesAsync(string networkName, CancellationToken cancellationToken = default);
Task ConnectServiceToNetworkAsync(string serviceName, string networkName, CancellationToken cancellationToken = default);
Task DisconnectServiceFromNetworkAsync(string serviceName, string networkName, CancellationToken cancellationToken = default);
Task<(int count, List<string> deletedNetworks)> PruneNetworksAsync(CancellationToken cancellationToken = default);
Task<IList<NetworkResponse>> GetUnusedNetworksAsync(CancellationToken cancellationToken = default);
```

## Pas de migration requise

Cette fonctionnalite n'implique PAS de modification d'entites en base de donnees. Les reseaux Docker sont geres directement via l'API Docker, pas via EF Core.

## Decision

**VALIDE** - L'issue peut etre implementee en suivant les patterns existants.

---

# Specifications Gherkin

## Feature: Gestion des reseaux Docker Swarm

```gherkin
Feature: Docker Swarm Network Management
  En tant qu'administrateur
  Je veux gerer les reseaux Docker Swarm
  Afin de configurer la communication entre les services

  Background:
    Given je suis authentifie en tant qu'administrateur
    And le cluster Docker Swarm est accessible

  # ===== QUERIES =====

  @Query @Networks
  Scenario: Lister tous les reseaux Docker Swarm
    Given des reseaux Docker existent dans le cluster
    When je demande la liste des reseaux
    Then je recois une liste de reseaux avec leurs proprietes
    And chaque reseau contient un Id, Name, Driver, Scope et ServiceCount

  @Query @Networks
  Scenario: Afficher les details d'un reseau
    Given un reseau "my-network" existe
    When je demande les details du reseau "my-network"
    Then je recois les informations detaillees du reseau
    And je vois la liste des services connectes
    And je vois la liste des conteneurs connectes
    And je vois la configuration IPAM (Subnet, Gateway)

  @Query @Networks
  Scenario: Lister les reseaux non utilises
    Given des reseaux sans services connectes existent
    When je demande la liste des reseaux non utilises
    Then je recois uniquement les reseaux sans services connectes

  @Query @Networks
  Scenario Outline: Filtrer les reseaux par driver
    Given des reseaux avec differents drivers existent
    When je demande les reseaux avec le driver "<driver>"
    Then je recois uniquement les reseaux de type "<driver>"

    Examples:
      | driver  |
      | overlay |
      | bridge  |
      | host    |

  # ===== COMMANDS - CREATE =====

  @Command @Networks
  Scenario: Creer un nouveau reseau overlay
    Given aucun reseau nomme "production-net" n'existe
    When je cree un reseau avec les parametres:
      | Name          | production-net |
      | Driver        | overlay        |
    Then le reseau "production-net" est cree avec succes
    And le reseau a le driver "overlay"
    And je recois l'ID du reseau cree

  @Command @Networks
  Scenario: Creer un reseau avec configuration IPAM personnalisee
    Given aucun reseau nomme "custom-net" n'existe
    When je cree un reseau avec les parametres:
      | Name    | custom-net      |
      | Driver  | overlay         |
      | Subnet  | 10.0.5.0/24     |
      | Gateway | 10.0.5.1        |
    Then le reseau "custom-net" est cree avec la configuration IPAM specifiee

  @Command @Networks @Validation
  Scenario: Echec creation reseau - nom invalide
    When je tente de creer un reseau avec le nom "invalid name!"
    Then la creation echoue avec une erreur de validation
    And le message d'erreur indique "Caracteres invalides dans le nom"

  @Command @Networks @Validation
  Scenario: Echec creation reseau - nom deja utilise
    Given un reseau "existing-net" existe deja
    When je tente de creer un reseau avec le nom "existing-net"
    Then la creation echoue avec une erreur de conflit 409
    And le message d'erreur indique "Le reseau 'existing-net' existe deja"

  @Command @Networks @Validation
  Scenario: Echec creation reseau - driver invalide
    When je tente de creer un reseau avec le driver "invalid-driver"
    Then la creation echoue avec une erreur de validation
    And le message d'erreur indique "Driver doit etre 'overlay' ou 'bridge'"

  # ===== COMMANDS - DELETE =====

  @Command @Networks
  Scenario: Supprimer un reseau
    Given un reseau "to-delete-net" existe sans services connectes
    When je supprime le reseau "to-delete-net"
    Then le reseau "to-delete-net" est supprime avec succes

  @Command @Networks @Validation
  Scenario: Echec suppression reseau - services connectes
    Given un reseau "in-use-net" existe avec des services connectes
    When je tente de supprimer le reseau "in-use-net"
    Then la suppression echoue avec une erreur
    And le message d'erreur indique que des services sont encore connectes

  @Command @Networks @Validation
  Scenario: Echec suppression reseau - reseau systeme
    Given le reseau "ingress" est un reseau systeme Docker
    When je tente de supprimer le reseau "ingress"
    Then la suppression echoue avec une erreur
    And le message d'erreur indique que les reseaux systeme ne peuvent pas etre supprimes

  @Command @Networks @Validation
  Scenario: Echec suppression reseau - inexistant
    Given aucun reseau nomme "ghost-net" n'existe
    When je tente de supprimer le reseau "ghost-net"
    Then la suppression echoue avec une erreur 404
    And le message d'erreur indique "Reseau 'ghost-net' non trouve"

  # ===== COMMANDS - CONNECT =====

  @Command @Networks
  Scenario: Connecter un service a un reseau
    Given un reseau "app-network" existe
    And un service "web-service" existe sans connexion a "app-network"
    When je connecte le service "web-service" au reseau "app-network"
    Then le service "web-service" est connecte au reseau "app-network"
    And le service peut communiquer avec les autres services du reseau

  @Command @Networks @Validation
  Scenario: Echec connexion - service deja connecte
    Given un reseau "app-network" existe
    And le service "web-service" est deja connecte a "app-network"
    When je tente de connecter le service "web-service" au reseau "app-network"
    Then la connexion echoue avec une erreur
    And le message d'erreur indique que le service est deja connecte

  @Command @Networks @Validation
  Scenario: Echec connexion - service inexistant
    Given un reseau "app-network" existe
    And aucun service nomme "ghost-service" n'existe
    When je tente de connecter le service "ghost-service" au reseau "app-network"
    Then la connexion echoue avec une erreur 404
    And le message d'erreur indique "Service 'ghost-service' non trouve"

  # ===== COMMANDS - DISCONNECT =====

  @Command @Networks
  Scenario: Deconnecter un service d'un reseau
    Given un reseau "app-network" existe
    And le service "web-service" est connecte au reseau "app-network"
    When je deconnecte le service "web-service" du reseau "app-network"
    Then le service "web-service" n'est plus connecte au reseau "app-network"

  @Command @Networks @Validation
  Scenario: Echec deconnexion - service non connecte
    Given un reseau "app-network" existe
    And le service "web-service" n'est pas connecte au reseau "app-network"
    When je tente de deconnecter le service "web-service" du reseau "app-network"
    Then la deconnexion echoue avec une erreur
    And le message d'erreur indique que le service n'est pas connecte

  # ===== COMMANDS - PRUNE =====

  @Command @Networks
  Scenario: Nettoyer les reseaux inutilises
    Given des reseaux inutilises existent dans le cluster
    When je lance le nettoyage des reseaux inutilises
    Then les reseaux sans services connectes sont supprimes
    And je recois le nombre de reseaux supprimes
    And je recois la liste des noms des reseaux supprimes

  @Command @Networks
  Scenario: Prune sans reseaux a nettoyer
    Given tous les reseaux ont des services connectes
    When je lance le nettoyage des reseaux inutilises
    Then aucun reseau n'est supprime
    And je recois un compte de 0 reseaux supprimes

  # ===== UI TESTS =====

  @UI @Networks
  Scenario: Afficher l'onglet Reseaux dans SwarmPage
    When je navigue vers la page Swarm
    Then je vois l'onglet "Reseaux" avec le compteur de reseaux
    And je peux cliquer sur l'onglet pour afficher la liste des reseaux

  @UI @Networks
  Scenario: Afficher le tableau des reseaux
    Given je suis sur l'onglet Reseaux de la page Swarm
    When la page se charge
    Then je vois un tableau avec les colonnes: Nom, Driver, Scope, Services, Actions
    And chaque ligne a des boutons pour Details et Supprimer

  @UI @Networks
  Scenario: Ouvrir le modal de creation de reseau
    Given je suis sur l'onglet Reseaux de la page Swarm
    When je clique sur le bouton "Nouveau Reseau"
    Then un modal de creation s'ouvre
    And le modal contient les champs: Nom, Driver, Labels

  @UI @Networks
  Scenario: Ouvrir le modal de details d'un reseau
    Given je suis sur l'onglet Reseaux de la page Swarm
    And un reseau "my-network" est affiche dans le tableau
    When je clique sur le bouton Details de "my-network"
    Then un modal de details s'ouvre
    And je vois les informations completes du reseau
    And je vois la liste des services connectes

  @UI @Networks
  Scenario: Confirmer la suppression d'un reseau
    Given je suis sur l'onglet Reseaux de la page Swarm
    And un reseau "to-delete" est affiche dans le tableau
    When je clique sur le bouton Supprimer de "to-delete"
    Then un dialog de confirmation s'affiche
    And le dialog demande confirmation de suppression
```

## Feature: Validation des reseaux

```gherkin
Feature: Network Validation Rules
  Les regles de validation pour la creation et gestion des reseaux

  @Validation @Networks
  Scenario Outline: Validation du nom de reseau
    When je tente de creer un reseau avec le nom "<nom>"
    Then la validation <resultat>

    Examples:
      | nom              | resultat |
      | valid-name       | reussit  |
      | my_network_123   | reussit  |
      | 123-starts-num   | reussit  |
      | name with space  | echoue   |
      | name@special     | echoue   |
      | -starts-dash     | echoue   |
      |                  | echoue   |

  @Validation @Networks
  Scenario Outline: Validation du driver
    When je tente de creer un reseau avec le driver "<driver>"
    Then la validation <resultat>

    Examples:
      | driver  | resultat |
      | overlay | reussit  |
      | bridge  | reussit  |
      | host    | reussit  |
      | custom  | echoue   |
      |         | echoue   |

  @Validation @Networks
  Scenario Outline: Validation du subnet CIDR
    When je tente de creer un reseau avec le subnet "<subnet>"
    Then la validation <resultat>

    Examples:
      | subnet          | resultat |
      | 10.0.0.0/24     | reussit  |
      | 192.168.1.0/16  | reussit  |
      | 172.16.0.0/12   | reussit  |
      | 10.0.0.0        | echoue   |
      | 10.0.0.0/33     | echoue   |
      | invalid         | echoue   |
```

---

# Fichiers a creer/modifier

## Backend

### Nouveaux fichiers
- `BackendAdmin.Application/Features/Swarm/Commands/CreateNetwork/CreateNetworkCommand.cs`
- `BackendAdmin.Application/Features/Swarm/Commands/CreateNetwork/CreateNetworkHandler.cs`
- `BackendAdmin.Application/Features/Swarm/Commands/CreateNetwork/CreateNetworkValidator.cs`
- `BackendAdmin.Application/Features/Swarm/Commands/DeleteNetwork/DeleteNetworkCommand.cs`
- `BackendAdmin.Application/Features/Swarm/Commands/DeleteNetwork/DeleteNetworkHandler.cs`
- `BackendAdmin.Application/Features/Swarm/Commands/ConnectServiceToNetwork/ConnectServiceToNetworkCommand.cs`
- `BackendAdmin.Application/Features/Swarm/Commands/ConnectServiceToNetwork/ConnectServiceToNetworkHandler.cs`
- `BackendAdmin.Application/Features/Swarm/Commands/ConnectServiceToNetwork/ConnectServiceToNetworkValidator.cs`
- `BackendAdmin.Application/Features/Swarm/Commands/DisconnectServiceFromNetwork/DisconnectServiceFromNetworkCommand.cs`
- `BackendAdmin.Application/Features/Swarm/Commands/DisconnectServiceFromNetwork/DisconnectServiceFromNetworkHandler.cs`
- `BackendAdmin.Application/Features/Swarm/Commands/PruneNetworks/PruneNetworksCommand.cs`
- `BackendAdmin.Application/Features/Swarm/Commands/PruneNetworks/PruneNetworksHandler.cs`
- `BackendAdmin.Application/Features/Swarm/Queries/GetNetworks/GetNetworksQuery.cs`
- `BackendAdmin.Application/Features/Swarm/Queries/GetNetworks/GetNetworksHandler.cs`
- `BackendAdmin.Application/Features/Swarm/Queries/GetNetworkDetails/GetNetworkDetailsQuery.cs`
- `BackendAdmin.Application/Features/Swarm/Queries/GetNetworkDetails/GetNetworkDetailsHandler.cs`
- `BackendAdmin.Application/Features/Swarm/Queries/GetUnusedNetworks/GetUnusedNetworksQuery.cs`
- `BackendAdmin.Application/Features/Swarm/Queries/GetUnusedNetworks/GetUnusedNetworksHandler.cs`
- `BackendAdmin.Application/Features/Swarm/DTOs/NetworkDTO.cs`
- `BackendAdmin.Application/Features/Swarm/DTOs/NetworkDetailsDTO.cs`
- `BackendAdmin.Api/Endpoints/Swarm/GetNetworks.cs`
- `BackendAdmin.Api/Endpoints/Swarm/GetNetworkDetails.cs`
- `BackendAdmin.Api/Endpoints/Swarm/CreateNetwork.cs`
- `BackendAdmin.Api/Endpoints/Swarm/DeleteNetwork.cs`
- `BackendAdmin.Api/Endpoints/Swarm/ConnectServiceToNetwork.cs`
- `BackendAdmin.Api/Endpoints/Swarm/DisconnectServiceFromNetwork.cs`
- `BackendAdmin.Api/Endpoints/Swarm/PruneNetworks.cs`

### Fichiers a modifier
- `BackendAdmin.Application/Services/IDockerSwarmService.cs` (ajouter methodes Networks)
- `BackendAdmin.Application/Services/DockerSwarmService.cs` (implementer methodes Networks)

## Frontend

### Nouveaux fichiers
- `FrontendAdmin.Shared/Pages/Swarm/Components/NetworkListComponent.razor`
- `FrontendAdmin.Shared/Pages/Swarm/Components/NetworkDetailsComponent.razor`
- `FrontendAdmin.Shared/Pages/Swarm/Components/CreateNetworkComponent.razor`
- `FrontendAdmin.Shared/Pages/Swarm/Components/ConnectServiceComponent.razor`

### Fichiers a modifier
- `FrontendAdmin.Shared/Pages/Swarm/SwarmPage.razor` (ajouter tab Networks)
- `FrontendAdmin.Shared/Pages/Swarm/Models/SwarmModels.cs` (ajouter DTOs Networks)
- `FrontendAdmin.Shared/Services/Https/ISwarmHttpService.cs` (ajouter endpoints Networks)

---

# Estimation

- **Backend**: ~25 fichiers (Commands, Queries, Handlers, DTOs, Endpoints)
- **Frontend**: ~5-6 fichiers (Page, Components, Models, Service)
- **Tests**: ~30 scenarios Gherkin

# Status: VALIDE

L'issue peut etre implementee. Aucun blocage detecte.
