# Analyse Issue #156 - Gestion Images Docker

## Informations Issue
- **Numero**: 156
- **Titre**: Gestion Images
- **Type**: Feature - Full Stack (Backend + Frontend)
- **Scope**: BackendAdmin + FrontendAdmin
- **Status**: VALIDE

## Resume de la demande

Ajouter la gestion des images Docker dans le module Docker Swarm existant avec les fonctionnalites suivantes:

| Fonctionnalite | Description |
|----------------|-------------|
| Lister les images | Toutes les images avec taille |
| Pull une image | Telecharger depuis registry |
| Supprimer une image | Retirer une image |
| Inspecter une image | Layers, taille, config |
| Historique d'une image | Layers et commandes |
| Tag une image | Renommer/versionner |
| Push une image | Envoyer vers registry |
| Pruner les images | Supprimer images inutilisees |
| Images dangling | Images sans tag |

## Analyse du code existant

### Pattern Docker Swarm identifie
Le projet utilise deja Docker.DotNet SDK pour la gestion des services, volumes et nodes.
Les patterns suivants sont etablis:
- **Backend**: Architecture Clean Vertical Slice avec CQRS (IQuery/ICommand, Handlers)
- **Docker Client**: `DockerSwarmService.cs` utilisant `Docker.DotNet.DockerClient`
- **Endpoints**: Carter modules avec routes `/api/swarm/*`
- **Frontend**: Composants Blazor + Refit service `ISwarmHttpService`

### API Docker.DotNet disponibles pour les images
```csharp
// Listing images
_client.Images.ListImagesAsync(ImagesListParameters parameters)

// Pull image
_client.Images.CreateImageAsync(ImagesCreateParameters parameters, AuthConfig authConfig, IProgress<JSONMessage> progress)

// Delete image
_client.Images.DeleteImageAsync(string name, ImageDeleteParameters parameters)

// Inspect image
_client.Images.InspectImageAsync(string name)

// Image history
_client.Images.GetImageHistoryAsync(string name)

// Tag image
_client.Images.TagImageAsync(string name, ImageTagParameters parameters)

// Push image
_client.Images.PushImageAsync(string name, ImagePushParameters parameters, AuthConfig authConfig, IProgress<JSONMessage> progress)

// Prune images
_client.Images.PruneImagesAsync(ImagesPruneParameters parameters)
```

## Fichiers a creer

### Backend - Application Layer

#### DTOs (`BackendAdmin.Application/Features/Swarm/DTOs/`)
```
ImageDTO.cs
- ImageDTO: Id, RepoTags[], RepoDigests[], Size, VirtualSize, Created, Labels
- ImageDetailsDTO: Id, RepoTags[], RepoDigests[], Size, VirtualSize, Created, Labels, Architecture, Os, Config, RootFS
- ImageHistoryDTO: Id, CreatedBy, Created, Size, Tags[]
- PullImageRequest: Image, Tag, Registry?
- PullImageResponse: ImageId, Status
- TagImageRequest: Repo, Tag
- PruneImagesResponse: DeletedCount, SpaceReclaimed, DeletedImages[]
```

#### Queries (`BackendAdmin.Application/Features/Swarm/Queries/`)
```
GetImages/
  - GetImagesQuery.cs
  - GetImagesHandler.cs

GetImageDetails/
  - GetImageDetailsQuery.cs
  - GetImageDetailsHandler.cs

GetImageHistory/
  - GetImageHistoryQuery.cs
  - GetImageHistoryHandler.cs

GetDanglingImages/
  - GetDanglingImagesQuery.cs
  - GetDanglingImagesHandler.cs
```

#### Commands (`BackendAdmin.Application/Features/Swarm/Commands/`)
```
PullImage/
  - PullImageCommand.cs
  - PullImageHandler.cs
  - PullImageValidator.cs

DeleteImage/
  - DeleteImageCommand.cs
  - DeleteImageHandler.cs
  - DeleteImageValidator.cs

TagImage/
  - TagImageCommand.cs
  - TagImageHandler.cs
  - TagImageValidator.cs

PushImage/
  - PushImageCommand.cs
  - PushImageHandler.cs
  - PushImageValidator.cs

PruneImages/
  - PruneImagesCommand.cs
  - PruneImagesHandler.cs
```

### Backend - Service Layer

#### Modification `IDockerSwarmService.cs`
Ajouter les methodes:
```csharp
// Image management
Task<IList<ImagesListResponse>> GetImagesAsync(bool all = false, CancellationToken cancellationToken = default);
Task<ImageInspectResponse> GetImageByIdAsync(string imageId, CancellationToken cancellationToken = default);
Task<IList<ImageHistoryResponse>> GetImageHistoryAsync(string imageId, CancellationToken cancellationToken = default);
Task<IList<ImagesListResponse>> GetDanglingImagesAsync(CancellationToken cancellationToken = default);
Task<string> PullImageAsync(string image, string tag, string? registry, CancellationToken cancellationToken = default);
Task DeleteImageAsync(string imageId, bool force = false, bool noPrune = false, CancellationToken cancellationToken = default);
Task TagImageAsync(string imageId, string repo, string tag, CancellationToken cancellationToken = default);
Task PushImageAsync(string image, string tag, CancellationToken cancellationToken = default);
Task<(int count, long spaceReclaimed, List<string> deletedImages)> PruneImagesAsync(bool danglingOnly = true, CancellationToken cancellationToken = default);
```

#### Modification `DockerSwarmService.cs`
Implementer les methodes ci-dessus utilisant Docker.DotNet.

### Backend - API Layer

#### Endpoints (`BackendAdmin.Api/Endpoints/Swarm/`)
```
GetImages.cs         - GET    /api/swarm/images
GetImageDetails.cs   - GET    /api/swarm/images/{id}
GetImageHistory.cs   - GET    /api/swarm/images/{id}/history
GetDanglingImages.cs - GET    /api/swarm/images/dangling
PullImage.cs         - POST   /api/swarm/images/pull
DeleteImage.cs       - DELETE /api/swarm/images/{id}
TagImage.cs          - POST   /api/swarm/images/{id}/tag
PushImage.cs         - POST   /api/swarm/images/{id}/push
PruneImages.cs       - POST   /api/swarm/images/prune
```

### Frontend - Models

#### Modification `SwarmModels.cs`
Ajouter:
```csharp
// Image models
public record ImageDto(
    string Id,
    string[] RepoTags,
    string[] RepoDigests,
    long Size,
    long VirtualSize,
    DateTime Created,
    Dictionary<string, string> Labels
);

public record ImageDetailsDto(
    string Id,
    string[] RepoTags,
    string[] RepoDigests,
    long Size,
    long VirtualSize,
    DateTime Created,
    Dictionary<string, string> Labels,
    string Architecture,
    string Os,
    ImageConfigDto Config,
    ImageRootFSDto RootFS
);

public record ImageConfigDto(
    string[] Cmd,
    string[] Entrypoint,
    string[] Env,
    string WorkingDir,
    Dictionary<string, object> ExposedPorts,
    Dictionary<string, string> Labels
);

public record ImageRootFSDto(
    string Type,
    string[] Layers
);

public record ImageHistoryDto(
    string Id,
    string CreatedBy,
    DateTime Created,
    long Size,
    string[] Tags
);

public record GetImagesResponse(List<ImageDto> Images);
public record GetImageDetailsResponse(ImageDetailsDto Image);
public record GetImageHistoryResponse(List<ImageHistoryDto> History);
public record GetDanglingImagesResponse(List<ImageDto> Images);

public record PullImageRequest(string Image, string Tag = "latest", string? Registry = null);
public record PullImageResponse(string ImageId, string Status);

public record TagImageRequest(string Repo, string Tag);
public record TagImageResponse(string ImageId, string NewTag);

public record PushImageRequest(string Tag);
public record PushImageResponse(string ImageId, string Status);

public record PruneImagesResponse(int DeletedCount, long SpaceReclaimed, List<string> DeletedImages);
```

### Frontend - Service HTTP

#### Modification `ISwarmHttpService.cs`
Ajouter:
```csharp
// Images - List
[Get("/api/swarm/images")]
Task<BaseResponse<GetImagesResponse>> GetImagesAsync([Query] bool all = false);

// Images - Details
[Get("/api/swarm/images/{id}")]
Task<BaseResponse<GetImageDetailsResponse>> GetImageDetailsAsync(string id);

// Images - History
[Get("/api/swarm/images/{id}/history")]
Task<BaseResponse<GetImageHistoryResponse>> GetImageHistoryAsync(string id);

// Images - Dangling
[Get("/api/swarm/images/dangling")]
Task<BaseResponse<GetDanglingImagesResponse>> GetDanglingImagesAsync();

// Images - Pull
[Post("/api/swarm/images/pull")]
Task<BaseResponse<PullImageResponse>> PullImageAsync([Body] PullImageRequest request);

// Images - Delete
[Delete("/api/swarm/images/{id}")]
Task DeleteImageAsync(string id, [Query] bool force = false);

// Images - Tag
[Post("/api/swarm/images/{id}/tag")]
Task<BaseResponse<TagImageResponse>> TagImageAsync(string id, [Body] TagImageRequest request);

// Images - Push
[Post("/api/swarm/images/{id}/push")]
Task<BaseResponse<PushImageResponse>> PushImageAsync(string id, [Body] PushImageRequest request);

// Images - Prune
[Post("/api/swarm/images/prune")]
Task<BaseResponse<PruneImagesResponse>> PruneImagesAsync([Query] bool danglingOnly = true);
```

### Frontend - Composants Blazor

#### Nouveaux composants (`FrontendAdmin.Shared/Pages/Swarm/Components/`)
```
ImageListComponent.razor
  - Tableau listant toutes les images
  - Colonnes: Repository:Tag, ID (short), Taille, Date creation, Actions
  - Actions: Inspecter, Historique, Supprimer, Tag, Push

ImageDetailsComponent.razor
  - Modal affichant les details d'une image
  - Onglets: Info generale, Configuration, Layers

ImageHistoryComponent.razor
  - Modal affichant l'historique des layers
  - Liste des commandes Dockerfile avec tailles

PullImageComponent.razor
  - Modal pour pull une nouvelle image
  - Champs: Registry (optionnel), Image, Tag
  - Progress indicator pendant le pull

TagImageComponent.razor
  - Modal pour tagger une image
  - Champs: Nouveau repository, Nouveau tag

PushImageComponent.razor
  - Modal pour push une image
  - Selection du tag a push
  - Progress indicator pendant le push
```

#### Modification `SwarmPage.razor`
Ajouter un 4eme onglet "Images" dans l'interface tabbed existante.

## Architecture des donnees

### Flux de donnees
```
Frontend (Blazor)
    |
    v
ISwarmHttpService (Refit)
    |
    v
Backend API (Carter Endpoints)
    |
    v
CQRS (Commands/Queries + Handlers)
    |
    v
IDockerSwarmService
    |
    v
Docker.DotNet SDK
    |
    v
Docker Socket (/var/run/docker.sock)
```

## Estimation des fichiers

| Layer | Fichiers a creer | Fichiers a modifier |
|-------|-----------------|---------------------|
| DTOs | 1 (ImageDTO.cs) | 0 |
| Queries | 8 (4 queries + 4 handlers) | 0 |
| Commands | 15 (5 commands + 5 handlers + 5 validators) | 0 |
| Endpoints | 9 | 0 |
| Services | 0 | 2 (IDockerSwarmService + DockerSwarmService) |
| Frontend Models | 0 | 1 (SwarmModels.cs) |
| Frontend Service | 0 | 1 (ISwarmHttpService.cs) |
| Frontend Components | 5 | 1 (SwarmPage.razor) |
| **Total** | **38** | **5** |

## Considerations techniques

### Authentification Registry
Pour les operations `pull` et `push`, le SDK Docker.DotNet supporte `AuthConfig`:
```csharp
var authConfig = new AuthConfig
{
    Username = "user",
    Password = "password",
    ServerAddress = "registry.example.com"
};
```
Cette fonctionnalite peut etre implementee dans une version ulterieure.
Pour la v1, utiliser le registry Docker Hub public (images publiques).

### Progress des operations longues
Les operations `pull` et `push` peuvent prendre du temps.
Utiliser `IProgress<JSONMessage>` pour le suivi cote backend.
Cote frontend, afficher un spinner avec message de statut.

### Taille des images
Les tailles retournees par Docker sont en octets.
Prevoir un formatteur cote frontend (Ko, Mo, Go).

### Images dangling
Utiliser le filtre `dangling=true` dans `ImagesListParameters`:
```csharp
var parameters = new ImagesListParameters
{
    Filters = new Dictionary<string, IDictionary<string, bool>>
    {
        ["dangling"] = new Dictionary<string, bool> { ["true"] = true }
    }
};
```

## Validation

L'analyse est **VALIDE** car:
- [x] Le scope est clairement defini (Backend + Frontend)
- [x] Les patterns existants (Docker Swarm) peuvent etre reutilises
- [x] Le SDK Docker.DotNet supporte toutes les operations demandees
- [x] Aucune contradiction avec le code existant
- [x] Les entites ne sont pas modifiees (pas de migration)
- [x] Utilise IDR.Library.BuildingBlocks (CQRS pattern)

## Decision
**Statut**: VALIDE
**Action**: Deplacer vers "Todo"
