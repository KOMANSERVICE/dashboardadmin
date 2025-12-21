# Issue #156 - Gestion Images Docker

Feature: Gestion des images Docker
  En tant qu'administrateur
  Je veux gerer les images Docker depuis l'interface d'administration
  Afin de pouvoir deployer et maintenir les applications conteneurisees

  Background:
    Given l'utilisateur est authentifie avec un role administrateur
    And le daemon Docker est accessible

  # ============================================
  # LISTER LES IMAGES
  # ============================================

  @backend @query
  Scenario: Lister toutes les images Docker
    When l'utilisateur demande la liste des images
    Then la liste des images est retournee avec succes
    And chaque image contient les informations suivantes:
      | Champ       | Description                    |
      | Id          | Identifiant unique de l'image  |
      | RepoTags    | Liste des tags (repo:tag)      |
      | Size        | Taille de l'image en octets    |
      | VirtualSize | Taille virtuelle en octets     |
      | Created     | Date de creation               |
      | Labels      | Labels associes                |

  @backend @query
  Scenario: Lister les images incluant les intermediaires
    When l'utilisateur demande la liste des images avec le parametre all=true
    Then la liste inclut les images intermediaires
    And les images sans tag sont incluses

  @frontend @component
  Scenario: Afficher la liste des images dans l'interface
    Given des images Docker existent sur le systeme
    When l'utilisateur navigue vers l'onglet "Images" de la page Swarm
    Then un tableau affiche la liste des images
    And le tableau contient les colonnes:
      | Colonne     | Format                    |
      | Repository  | repo:tag                  |
      | ID          | 12 premiers caracteres    |
      | Taille      | Format humain (Mo, Go)    |
      | Cree le     | Date relative ou absolue  |
      | Actions     | Boutons d'action          |

  # ============================================
  # INSPECTER UNE IMAGE
  # ============================================

  @backend @query
  Scenario: Obtenir les details d'une image
    Given une image avec l'id "sha256:abc123" existe
    When l'utilisateur demande les details de l'image "sha256:abc123"
    Then les details complets de l'image sont retournes:
      | Champ        | Description                     |
      | Id           | Identifiant complet             |
      | RepoTags     | Tous les tags                   |
      | RepoDigests  | Digests de l'image              |
      | Architecture | Architecture cible (amd64, arm) |
      | Os           | Systeme d'exploitation          |
      | Config       | Configuration du conteneur      |
      | RootFS       | Informations sur les layers     |

  @backend @query
  Scenario: Demander les details d'une image inexistante
    Given aucune image avec l'id "sha256:nonexistent" n'existe
    When l'utilisateur demande les details de l'image "sha256:nonexistent"
    Then une erreur 404 "Image non trouvee" est retournee

  @frontend @component
  Scenario: Afficher les details d'une image dans un modal
    Given une image existe dans la liste
    When l'utilisateur clique sur le bouton "Inspecter" d'une image
    Then un modal s'ouvre avec les details de l'image
    And le modal affiche les onglets:
      | Onglet        | Contenu                              |
      | General       | ID, Tags, Taille, Date creation      |
      | Configuration | Entrypoint, Cmd, Env, Ports exposes  |
      | Layers        | Liste des layers avec tailles        |

  # ============================================
  # HISTORIQUE D'UNE IMAGE
  # ============================================

  @backend @query
  Scenario: Obtenir l'historique d'une image
    Given une image avec l'id "sha256:abc123" existe
    When l'utilisateur demande l'historique de l'image "sha256:abc123"
    Then l'historique des layers est retourne
    And chaque entree contient:
      | Champ     | Description                      |
      | Id        | ID du layer (ou <missing>)       |
      | CreatedBy | Commande Dockerfile              |
      | Created   | Date de creation                 |
      | Size      | Taille du layer                  |
      | Tags      | Tags associes au layer           |

  @frontend @component
  Scenario: Afficher l'historique d'une image
    When l'utilisateur clique sur "Historique" pour une image
    Then un modal affiche la liste des layers
    And chaque layer montre la commande Dockerfile utilisee
    And les layers sont ordonnes du plus recent au plus ancien

  # ============================================
  # IMAGES DANGLING (SANS TAG)
  # ============================================

  @backend @query
  Scenario: Lister les images dangling
    Given des images sans tag existent sur le systeme
    When l'utilisateur demande la liste des images dangling
    Then seules les images sans tag sont retournees
    And le champ RepoTags est vide ou contient "<none>:<none>"

  @frontend @component
  Scenario: Filtrer les images dangling dans l'interface
    Given des images dangling existent
    When l'utilisateur active le filtre "Images dangling"
    Then seules les images sans tag sont affichees
    And un bouton "Nettoyer les dangling" est visible

  # ============================================
  # PULL UNE IMAGE
  # ============================================

  @backend @command
  Scenario: Pull une image depuis Docker Hub
    When l'utilisateur demande le pull de l'image "nginx" avec le tag "latest"
    Then l'image est telechargee avec succes
    And l'id de l'image telechargee est retourne
    And le statut "Pull complete" est retourne

  @backend @command
  Scenario: Pull une image avec un tag specifique
    When l'utilisateur demande le pull de l'image "nginx" avec le tag "1.25-alpine"
    Then l'image "nginx:1.25-alpine" est telechargee
    And l'image apparait dans la liste des images

  @backend @command
  Scenario: Pull une image deja presente
    Given l'image "nginx:latest" existe deja
    When l'utilisateur demande le pull de l'image "nginx" avec le tag "latest"
    Then le statut "Image up to date" est retourne
    And aucun telechargement n'est effectue

  @backend @command @validation
  Scenario: Pull avec un nom d'image invalide
    When l'utilisateur demande le pull de l'image "" avec le tag "latest"
    Then une erreur de validation est retournee
    And le message indique "Le nom de l'image est requis"

  @frontend @component
  Scenario: Pull une image via l'interface
    When l'utilisateur clique sur "Pull Image"
    Then un modal s'ouvre avec un formulaire
    And le formulaire contient les champs:
      | Champ    | Obligatoire | Defaut  |
      | Image    | Oui         | -       |
      | Tag      | Non         | latest  |
      | Registry | Non         | -       |
    When l'utilisateur remplit le formulaire et clique sur "Pull"
    Then un indicateur de progression s'affiche
    And l'image apparait dans la liste apres le telechargement

  # ============================================
  # SUPPRIMER UNE IMAGE
  # ============================================

  @backend @command
  Scenario: Supprimer une image non utilisee
    Given une image "nginx:1.24" existe
    And l'image n'est pas utilisee par un conteneur
    When l'utilisateur demande la suppression de l'image "nginx:1.24"
    Then l'image est supprimee avec succes
    And un statut 204 No Content est retourne

  @backend @command
  Scenario: Supprimer une image utilisee par un conteneur
    Given une image "nginx:latest" existe
    And l'image est utilisee par un conteneur en cours d'execution
    When l'utilisateur demande la suppression de l'image "nginx:latest"
    Then une erreur 409 Conflict est retournee
    And le message indique "L'image est utilisee par un conteneur"

  @backend @command
  Scenario: Forcer la suppression d'une image
    Given une image "nginx:latest" est utilisee par un conteneur
    When l'utilisateur demande la suppression avec force=true
    Then l'image est supprimee malgre l'utilisation
    And les tags associes sont supprimes

  @backend @command
  Scenario: Supprimer une image inexistante
    Given aucune image avec l'id "sha256:nonexistent" n'existe
    When l'utilisateur demande la suppression de l'image "sha256:nonexistent"
    Then une erreur 404 est retournee

  @frontend @component
  Scenario: Supprimer une image via l'interface
    When l'utilisateur clique sur "Supprimer" pour une image
    Then une boite de dialogue de confirmation s'affiche
    And le message indique le nom/tag de l'image a supprimer
    When l'utilisateur confirme la suppression
    Then l'image est retiree de la liste
    And un toast de succes s'affiche

  # ============================================
  # TAG UNE IMAGE
  # ============================================

  @backend @command
  Scenario: Tagger une image avec un nouveau tag
    Given une image "nginx:latest" existe
    When l'utilisateur taggue l'image avec repo="myrepo/nginx" et tag="v1.0"
    Then un nouveau tag "myrepo/nginx:v1.0" est cree
    And l'image originale conserve son tag "nginx:latest"

  @backend @command @validation
  Scenario: Tagger avec un nom de repository invalide
    Given une image "nginx:latest" existe
    When l'utilisateur taggue l'image avec repo="" et tag="v1.0"
    Then une erreur de validation est retournee
    And le message indique "Le repository est requis"

  @frontend @component
  Scenario: Tagger une image via l'interface
    When l'utilisateur clique sur "Tag" pour une image
    Then un modal s'ouvre avec un formulaire
    And le formulaire contient les champs:
      | Champ      | Obligatoire |
      | Repository | Oui         |
      | Tag        | Oui         |
    When l'utilisateur remplit le formulaire et clique sur "Tag"
    Then le nouveau tag apparait dans la liste
    And un toast de succes s'affiche

  # ============================================
  # PUSH UNE IMAGE
  # ============================================

  @backend @command
  Scenario: Push une image vers un registry
    Given une image "myrepo/nginx:v1.0" existe
    And l'utilisateur est authentifie aupres du registry
    When l'utilisateur demande le push de l'image "myrepo/nginx" avec le tag "v1.0"
    Then l'image est envoyee vers le registry
    And le statut "Push complete" est retourne

  @backend @command
  Scenario: Push sans authentification
    Given une image "myrepo/nginx:v1.0" existe
    And l'utilisateur n'est pas authentifie aupres du registry
    When l'utilisateur demande le push de l'image "myrepo/nginx:v1.0"
    Then une erreur 401 Unauthorized est retournee
    And le message indique "Authentification requise pour le registry"

  @frontend @component
  Scenario: Push une image via l'interface
    When l'utilisateur clique sur "Push" pour une image
    Then un modal s'ouvre avec les options de push
    And le tag a push est preselectionne
    When l'utilisateur clique sur "Push"
    Then un indicateur de progression s'affiche
    And un toast indique le succes ou l'echec

  # ============================================
  # PRUNER LES IMAGES
  # ============================================

  @backend @command
  Scenario: Pruner les images dangling
    Given des images dangling existent sur le systeme
    When l'utilisateur demande le prune des images avec danglingOnly=true
    Then seules les images sans tag sont supprimees
    And le nombre d'images supprimees est retourne
    And l'espace recupere en octets est retourne

  @backend @command
  Scenario: Pruner toutes les images inutilisees
    Given des images non utilisees par des conteneurs existent
    When l'utilisateur demande le prune des images avec danglingOnly=false
    Then toutes les images non utilisees sont supprimees
    And la liste des images supprimees est retournee

  @backend @command
  Scenario: Pruner sans images a supprimer
    Given aucune image dangling n'existe
    When l'utilisateur demande le prune des images
    Then le compteur retourne 0 images supprimees
    And l'espace recupere est 0

  @frontend @component
  Scenario: Pruner les images via l'interface
    When l'utilisateur clique sur "Pruner les images"
    Then une boite de dialogue de confirmation s'affiche
    And l'utilisateur peut choisir entre:
      | Option                     | Description                  |
      | Images dangling seulement  | Images sans tag              |
      | Toutes les inutilisees     | Images non utilisees         |
    When l'utilisateur confirme
    Then les images sont supprimees
    And un toast affiche le nombre d'images supprimees et l'espace recupere

  # ============================================
  # INTEGRATION FRONTEND
  # ============================================

  @frontend @integration
  Scenario: Navigation vers l'onglet Images
    Given l'utilisateur est sur la page Docker Swarm
    When l'utilisateur clique sur l'onglet "Images"
    Then le contenu de l'onglet Images s'affiche
    And la liste des images est chargee automatiquement

  @frontend @integration
  Scenario: Rafraichir la liste des images
    Given l'utilisateur est sur l'onglet Images
    When l'utilisateur clique sur le bouton "Rafraichir"
    Then la liste des images est rechargee
    And un indicateur de chargement s'affiche pendant le chargement

  @frontend @integration
  Scenario: Rechercher une image
    Given des images existent dans la liste
    When l'utilisateur saisit "nginx" dans le champ de recherche
    Then seules les images contenant "nginx" sont affichees
    And le filtre est applique en temps reel

  # ============================================
  # GESTION DES ERREURS
  # ============================================

  @backend @error-handling
  Scenario: Docker socket non accessible
    Given le socket Docker n'est pas accessible
    When l'utilisateur demande la liste des images
    Then une erreur 500 est retournee
    And le message indique "Docker socket non accessible"

  @frontend @error-handling
  Scenario: Affichage des erreurs dans l'interface
    Given une erreur survient lors d'une operation
    When l'erreur est retournee par le backend
    Then un toast d'erreur s'affiche avec le message d'erreur
    And l'utilisateur peut retenter l'operation
