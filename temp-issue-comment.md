## Analyse de l'issue #148 - Ajout de Docker Swarm

### Classification
- **Scope**: BackendAdmin API
- **Type**: Nouvelle feature
- **Complexite**: Moyenne
- **Entites modifiees**: Non (pas de migration requise)

---

### Analyse technique

#### Endpoints demandes

| Methode | Endpoint | Description |
|---------|----------|-------------|
| GET | `/api/swarm/services` | Liste tous les services Docker Swarm |
| GET | `/api/swarm/services/{name}/logs` | Recupere les logs d'un service |
| POST | `/api/swarm/services/{name}/restart` | Redemarre un service |
| POST | `/api/swarm/services/{name}/scale` | Modifie le nombre de replicas |

#### Architecture proposee

**Structure CQRS conforme au projet:**

```
BackendAdmin/
├── BackendAdmin.Application/
│   ├── Features/
│   │   └── Swarm/
│   │       ├── Queries/
│   │       │   ├── GetSwarmServices/
│   │       │   │   ├── GetSwarmServicesQuery.cs
│   │       │   │   └── GetSwarmServicesHandler.cs
│   │       │   └── GetServiceLogs/
│   │       │       ├── GetServiceLogsQuery.cs
│   │       │       └── GetServiceLogsHandler.cs
│   │       ├── Commands/
│   │       │   ├── RestartService/
│   │       │   │   ├── RestartServiceCommand.cs
│   │       │   │   ├── RestartServiceHandler.cs
│   │       │   │   └── RestartServiceValidator.cs
│   │       │   └── ScaleService/
│   │       │       ├── ScaleServiceCommand.cs
│   │       │       ├── ScaleServiceHandler.cs
│   │       │       └── ScaleServiceValidator.cs
│   │       └── DTOs/
│   │           ├── SwarmServiceDTO.cs
│   │           ├── ServiceLogsDTO.cs
│   │           └── ScaleServiceDTO.cs
│   └── Services/
│       └── DockerSwarmService.cs
│
└── BackendAdmin.Api/
    └── Endpoints/
        └── Swarm/
            ├── GetSwarmServices.cs
            ├── GetServiceLogs.cs
            ├── RestartService.cs
            └── ScaleService.cs
```

#### Package requis
- **Docker.DotNet** (Version 3.125.x) - Client Docker officiel pour .NET
  - A ajouter dans `BackendAdmin.Application.csproj`

#### Prerequis Docker
Le container BackendAdmin doit avoir acces au socket Docker:
```yaml
volumes:
  - /var/run/docker.sock:/var/run/docker.sock:ro
```
Deja present dans `docker-compose.dev.yml` et `docker-compose.prod.yml`

---

### Specifications Gherkin

```gherkin
Feature: Gestion des services Docker Swarm
  En tant qu'administrateur
  Je veux gerer mes services Docker Swarm depuis l'interface d'administration
  Afin de surveiller et controler mes deploiements

  Background:
    Given l'utilisateur est authentifie avec un token JWT valide
    And le container BackendAdmin a acces au socket Docker

  # Liste des services
  Scenario: Lister tous les services Docker Swarm
    When l'utilisateur appelle GET /api/swarm/services
    Then le serveur retourne un statut 200
    And la reponse contient la liste des services avec leurs informations:
      | Propriete       | Description                    |
      | id              | Identifiant unique du service  |
      | name            | Nom du service                 |
      | replicas        | Nombre de replicas actifs      |
      | desiredReplicas | Nombre de replicas souhaites   |
      | image           | Image Docker utilisee          |
      | status          | Etat du service                |
      | createdAt       | Date de creation               |
      | updatedAt       | Date de derniere mise a jour   |

  Scenario: Aucun acces au socket Docker
    Given le container n'a pas acces au socket Docker
    When l'utilisateur appelle GET /api/swarm/services
    Then le serveur retourne un statut 500
    And la reponse contient le message "Docker socket non accessible"

  # Logs d'un service
  Scenario: Recuperer les logs d'un service existant
    Given un service "backendadmin.api" existe dans le Swarm
    When l'utilisateur appelle GET /api/swarm/services/backendadmin.api/logs
    Then le serveur retourne un statut 200
    And la reponse contient les logs du service

  Scenario: Recuperer les logs avec parametres optionnels
    Given un service "backendadmin.api" existe dans le Swarm
    When l'utilisateur appelle GET /api/swarm/services/backendadmin.api/logs?tail=100&since=1h
    Then le serveur retourne un statut 200
    And la reponse contient les 100 dernieres lignes des logs de la derniere heure

  Scenario: Recuperer les logs d'un service inexistant
    When l'utilisateur appelle GET /api/swarm/services/service-inexistant/logs
    Then le serveur retourne un statut 404
    And la reponse contient le message "Service 'service-inexistant' non trouve"

  # Redemarrer un service
  Scenario: Redemarrer un service existant
    Given un service "backendadmin.api" existe dans le Swarm
    When l'utilisateur appelle POST /api/swarm/services/backendadmin.api/restart
    Then le serveur retourne un statut 200
    And la reponse contient le message "Service redemarre avec succes"
    And le service est en cours de redemarrage

  Scenario: Redemarrer un service inexistant
    When l'utilisateur appelle POST /api/swarm/services/service-inexistant/restart
    Then le serveur retourne un statut 404
    And la reponse contient le message "Service 'service-inexistant' non trouve"

  # Scaler un service
  Scenario: Augmenter le nombre de replicas
    Given un service "backendadmin.api" existe avec 1 replica
    When l'utilisateur appelle POST /api/swarm/services/backendadmin.api/scale avec:
      | replicas | 3 |
    Then le serveur retourne un statut 200
    And la reponse contient:
      | previousReplicas | 1 |
      | newReplicas      | 3 |
    And le service est mis a jour avec 3 replicas

  Scenario: Reduire le nombre de replicas a zero
    Given un service "backendadmin.api" existe avec 3 replicas
    When l'utilisateur appelle POST /api/swarm/services/backendadmin.api/scale avec:
      | replicas | 0 |
    Then le serveur retourne un statut 200
    And la reponse contient:
      | previousReplicas | 3 |
      | newReplicas      | 0 |
    And le service n'a plus de replicas actifs

  Scenario: Scaler avec une valeur negative
    When l'utilisateur appelle POST /api/swarm/services/backendadmin.api/scale avec:
      | replicas | -1 |
    Then le serveur retourne un statut 400
    And la reponse contient le message "Le nombre de replicas doit etre >= 0"

  Scenario: Scaler un service inexistant
    When l'utilisateur appelle POST /api/swarm/services/service-inexistant/scale avec:
      | replicas | 2 |
    Then le serveur retourne un statut 404
    And la reponse contient le message "Service 'service-inexistant' non trouve"

  # Authentification
  Scenario: Acces sans authentification
    Given l'utilisateur n'est pas authentifie
    When l'utilisateur appelle GET /api/swarm/services
    Then le serveur retourne un statut 401

  Scenario: Token expire
    Given l'utilisateur a un token JWT expire
    When l'utilisateur appelle GET /api/swarm/services
    Then le serveur retourne un statut 401
```

---

### DTOs proposes

```csharp
// SwarmServiceDTO.cs
public record SwarmServiceDTO(
    string Id,
    string Name,
    int Replicas,
    int DesiredReplicas,
    string Image,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

// ServiceLogsDTO.cs
public record ServiceLogsDTO(
    string ServiceName,
    string Logs,
    DateTime FetchedAt
);

// ScaleServiceDTO.cs
public record ScaleServiceRequest(int Replicas);
public record ScaleServiceResponse(
    string ServiceName,
    int PreviousReplicas,
    int NewReplicas
);
```

---

### Points d'attention

1. **Securite**: Les endpoints doivent etre proteges par `.RequireAuthorization()`
2. **Logs sensibles**: Potentiellement filtrer les informations sensibles dans les logs
3. **Timeout**: Prevoir des timeouts appropries pour les operations Docker
4. **Gestion d'erreurs**: Mapper correctement les exceptions Docker en HTTP status codes

---

### Estimation

- **Fichiers a creer**: ~15 fichiers
- **Package a ajouter**: 1 (Docker.DotNet)
- **Tests a ecrire**: ~10 scenarios de test

---

**Statut**: VALIDE - Pret pour implementation

Analyse generee automatiquement par l'agent orchestrateur
