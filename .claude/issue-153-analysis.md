## Analyse de l'Issue #153 - Gestion Ressources & Limites

### Scope
- **BackendAdmin**: Oui
- **FrontendAdmin**: Oui
- **Migration EF Core**: Oui (nouvelle entite requise)

---

## Resume de l'analyse

L'issue demande d'ajouter la gestion complete des ressources et limites Docker Swarm:

| Fonctionnalite | Description | Type Docker API |
|----------------|-------------|-----------------|
| Limite CPU | Nombre de CPUs max | `Resources.Limits.NanoCPUs` |
| Reservation CPU | CPUs garantis | `Resources.Reservations.NanoCPUs` |
| Limite memoire | RAM max (ex: 512MB) | `Resources.Limits.MemoryBytes` |
| Reservation memoire | RAM garantie | `Resources.Reservations.MemoryBytes` |
| Limite I/O disque | Vitesse lecture/ecriture | `BlkioWeight`, `BlkioWeightDevice` |
| Limite reseau | Bande passante | Non supporte nativement Docker Swarm |
| Pids limit | Nombre max de processus | `Resources.Limits.Pids` |
| Ulimits | Limites systeme (nofile, etc.) | `ContainerSpec.Ulimits` |

### Point critique - Persistance

> **"S'assurer que les valeurs qui seront defini demeurre meme si un deploiement est fait et meme si le contenu du docker compose est modifier manuellement"**

Cette contrainte necessite:
1. **Nouvelle entite** `ServiceResourceConfig` pour stocker les configurations en base de donnees
2. **Mecanisme de reapplication** automatique lors des mises a jour de service

---

## Fichiers a modifier

### Backend (BackendAdmin)

| Fichier | Action |
|---------|--------|
| `Domain/Models/ServiceResourceConfig.cs` | **CREER** - Nouvelle entite pour persister les configs |
| `Application/Data/IApplicationDbContext.cs` | Ajouter DbSet<ServiceResourceConfig> |
| `Infrastructure/Data/ApplicationDbContext.cs` | Ajouter DbSet + Configuration |
| `Infrastructure/Data/Configurations/ServiceResourceConfigConfiguration.cs` | **CREER** - Configuration EF |
| `Application/Features/Swarm/DTOs/ServiceResourcesDTO.cs` | **CREER** - DTO ressources |
| `Application/Features/Swarm/DTOs/CreateServiceRequest.cs` | Ajouter `Resources` |
| `Application/Features/Swarm/DTOs/UpdateServiceRequest.cs` | Ajouter `Resources` |
| `Application/Features/Swarm/DTOs/ServiceDetailsDTO.cs` | Ajouter `Resources` |
| `Application/Services/IDockerSwarmService.cs` | Ajouter methodes gestion ressources |
| `Application/Services/DockerSwarmService.cs` | Implementer avec `TaskSpec.Resources` |
| `Application/Features/Swarm/Commands/UpdateServiceResources/` | **CREER** - Command CQRS dediee |
| `Application/Features/Swarm/Queries/GetServiceResources/` | **CREER** - Query CQRS |
| `Api/Endpoints/Swarm/UpdateServiceResources.cs` | **CREER** - Endpoint PUT /api/swarm/services/{name}/resources |
| `Api/Endpoints/Swarm/GetServiceResources.cs` | **CREER** - Endpoint GET /api/swarm/services/{name}/resources |

### Frontend (FrontendAdmin)

| Fichier | Action |
|---------|--------|
| `Pages/Swarm/Models/SwarmModels.cs` | Ajouter `ServiceResourcesDto`, `UlimitDto` |
| `Pages/Swarm/Components/ServiceResourcesComponent.razor` | **CREER** - Composant formulaire ressources |
| `Pages/Swarm/Components/CreateServiceComponent.razor` | Integrer section ressources |
| `Pages/Swarm/Components/ServiceDetailsComponent.razor` | Afficher ressources configurees |
| `Services/Https/ISwarmHttpService.cs` | Ajouter methodes API ressources |

### Migration EF Core

```
Migration: AddServiceResourceConfig
- CreateTable: ServiceResourceConfigs
  - Id (Guid, PK)
  - ServiceName (string, unique, indexed)
  - CpuLimit (long?, NanoCPUs)
  - CpuReservation (long?, NanoCPUs)
  - MemoryLimit (long?, bytes)
  - MemoryReservation (long?, bytes)
  - PidsLimit (long?)
  - BlkioWeight (int?)
  - UlimitsJson (string, JSON pour ulimits)
  - CreatedAt (DateTime)
  - UpdatedAt (DateTime)
```

---

## Specifications Gherkin

```gherkin
Feature: Gestion des Ressources et Limites des Services Docker Swarm
  En tant qu'administrateur
  Je veux configurer les ressources et limites des services Docker
  Afin de controler la consommation des ressources et garantir la stabilite

  Background:
    Given je suis connecte en tant qu'administrateur
    And le cluster Docker Swarm est accessible
    And le service "test-service" existe

  # ==================== LIMITES CPU ====================

  Scenario: Definir une limite CPU sur un service
    When je configure la limite CPU a 2.0 cores pour le service "test-service"
    Then le service devrait avoir une limite CPU de 2000000000 NanoCPUs
    And la configuration devrait etre persistee en base de donnees

  Scenario: Definir une reservation CPU sur un service
    When je configure la reservation CPU a 0.5 cores pour le service "test-service"
    Then le service devrait avoir une reservation CPU de 500000000 NanoCPUs
    And Docker devrait garantir ces ressources au service

  # ==================== LIMITES MEMOIRE ====================

  Scenario: Definir une limite memoire sur un service
    When je configure la limite memoire a "512MB" pour le service "test-service"
    Then le service devrait avoir une limite memoire de 536870912 bytes
    And le conteneur sera tue si il depasse cette limite

  Scenario: Definir une reservation memoire sur un service
    When je configure la reservation memoire a "256MB" pour le service "test-service"
    Then le service devrait avoir une reservation memoire de 268435456 bytes
    And Docker devrait garantir cette memoire au service

  Scenario Outline: Valider les formats de memoire
    When je configure la limite memoire a "<valeur>" pour le service "test-service"
    Then la valeur en bytes devrait etre <bytes>

    Examples:
      | valeur | bytes       |
      | 512MB  | 536870912   |
      | 1GB    | 1073741824  |
      | 2g     | 2147483648  |
      | 256m   | 268435456   |

  # ==================== PIDS LIMIT ====================

  Scenario: Definir une limite de processus sur un service
    When je configure la limite de PIDs a 100 pour le service "test-service"
    Then le service devrait avoir une limite de 100 processus maximum
    And les fork bombs seront limitees

  # ==================== ULIMITS ====================

  Scenario: Configurer les ulimits d'un service
    When je configure les ulimits suivants pour le service "test-service":
      | name   | soft  | hard  |
      | nofile | 65536 | 65536 |
      | nproc  | 4096  | 8192  |
    Then le service devrait avoir ces ulimits appliques

  Scenario: Modifier un ulimit existant
    Given le service "test-service" a un ulimit nofile de 1024
    When je modifie l'ulimit nofile a 65536
    Then la nouvelle valeur devrait etre appliquee
    And l'ancienne configuration devrait etre ecrasee

  # ==================== PERSISTANCE ====================

  Scenario: Les ressources persistent apres un redemarrage du service
    Given le service "test-service" a une limite CPU de 2.0 cores
    When je redemarre le service
    Then la limite CPU devrait toujours etre de 2.0 cores

  Scenario: Les ressources sont reappliquees apres un redeploiement
    Given le service "test-service" a une limite memoire de "512MB"
    And la configuration est sauvegardee en base de donnees
    When le service est redeploye via docker-compose
    Then l API devrait reappliquer automatiquement la limite memoire de "512MB"

  Scenario: Les ressources sont reappliquees apres modification manuelle du docker-compose
    Given le service "test-service" a une configuration de ressources sauvegardee
    When quelqu un modifie manuellement le docker-compose et redeploie
    Then l API devrait detecter la difference
    And reappliquer les ressources configurees dans interface

  # ==================== INTERFACE UTILISATEUR ====================

  Scenario: Afficher les ressources actuelles d un service
    Given le service "test-service" a des ressources configurees
    When je consulte les details du service
    Then je devrais voir:
      | Ressource           | Valeur configuree |
      | Limite CPU          | 2.0 cores         |
      | Reservation CPU     | 0.5 cores         |
      | Limite Memoire      | 512MB             |
      | Reservation Memoire | 256MB             |
      | Limite PIDs         | 100               |

  Scenario: Formulaire de configuration des ressources
    When j ouvre le formulaire de configuration des ressources
    Then je devrais voir les champs:
      | Champ               | Type   | Unite        |
      | Limite CPU          | number | cores        |
      | Reservation CPU     | number | cores        |
      | Limite Memoire      | text   | MB/GB        |
      | Reservation Memoire | text   | MB/GB        |
      | Limite PIDs         | number | processus    |
      | Ulimits             | table  | name/soft/hard |

  # ==================== VALIDATION ====================

  Scenario: Validation - Limite CPU negative refusee
    When je configure la limite CPU a -1 core
    Then je devrais recevoir une erreur de validation
    And le message devrait indiquer "La limite CPU doit etre positive"

  Scenario: Validation - Reservation superieure a la limite refusee
    When je configure la limite CPU a 1.0 cores
    And je configure la reservation CPU a 2.0 cores
    Then je devrais recevoir une erreur de validation
    And le message devrait indiquer "La reservation ne peut pas depasser la limite"

  Scenario: Validation - Format memoire invalide refuse
    When je configure la limite memoire a "invalid"
    Then je devrais recevoir une erreur de validation
    And le message devrait indiquer "Format de memoire invalide (ex: 512MB, 1GB)"

  # ==================== API ENDPOINTS ====================

  Scenario: GET /api/swarm/services/{name}/resources
    When je fais une requete GET sur "/api/swarm/services/test-service/resources"
    Then je devrais recevoir un code 200
    And la reponse devrait contenir les ressources configurees

  Scenario: PUT /api/swarm/services/{name}/resources
    When je fais une requete PUT sur "/api/swarm/services/test-service/resources" avec:
      """json
      {
        "cpuLimit": 2.0,
        "cpuReservation": 0.5,
        "memoryLimit": "512MB",
        "memoryReservation": "256MB",
        "pidsLimit": 100,
        "ulimits": [
          { "name": "nofile", "soft": 65536, "hard": 65536 }
        ]
      }
      """
    Then je devrais recevoir un code 200
    And les ressources devraient etre appliquees au service
    And les ressources devraient etre sauvegardees en base
```

---

## Notes techniques

### API Docker.DotNet - Mapping des ressources

```csharp
// Dans TaskSpec.Resources
var resources = new ResourceRequirements
{
    Limits = new Resources
    {
        NanoCPUs = cpuLimitInNanoCpus,    // 1 core = 1_000_000_000
        MemoryBytes = memoryLimitInBytes,
        Pids = pidsLimit
    },
    Reservations = new Resources
    {
        NanoCPUs = cpuReservationInNanoCpus,
        MemoryBytes = memoryReservationInBytes
    }
};

// Ulimits dans ContainerSpec
containerSpec.Ulimits = new List<Ulimit>
{
    new Ulimit { Name = "nofile", Soft = 65536, Hard = 65536 }
};
```

### Limites I/O disque et reseau

- **I/O disque**: Supporte via `BlkioWeight` (1-1000) et `BlkioWeightDevice` mais limite en Swarm mode
- **Reseau**: Non supporte nativement dans Docker Swarm. Alternatives possibles: plugins reseau, tc (traffic control), mais hors scope initial

### Recommandation

Implementer en priorite:
1. CPU Limit/Reservation
2. Memory Limit/Reservation
3. PIDs Limit
4. Ulimits (nofile, nproc)

Laisser I/O disque et reseau pour une iteration future.

---

## Statut: VALIDE

L issue est complete et prete pour implementation.

Agent: `orchestrator`
