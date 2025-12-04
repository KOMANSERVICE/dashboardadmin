# language: fr
Fonctionnalité: Gestion des mouvements inter-magasins
  En tant que gestionnaire de magasin
  Je veux pouvoir gérer les mouvements de produits entre magasins
  Afin de tracer et contrôler les transferts de stock

  Contexte:
    Étant donné que j'ai des magasins configurés dans le système
    Et que j'ai des produits provenant d'une application externe
    Et que je suis authentifié avec les droits appropriés

  Scénario: Créer un mouvement simple de produit entre deux magasins
    Étant donné un magasin source "MAG-001" de type "Store"
    Et un magasin destination "MAG-002" de type "Sale"
    Et un produit avec l'ID "PROD-123"
    Quand je crée un mouvement de stock avec:
      | Champ                | Valeur     |
      | Quantité             | 10         |
      | Reference            | MV-2024-001|
      | Type                 | Transfer   |
      | ProductId            | PROD-123   |
      | SourceLocationId     | MAG-001    |
      | DestinationLocationId| MAG-002    |
    Alors le mouvement est créé avec succès
    Et le mouvement a un ID unique généré
    Et la date du mouvement est définie à maintenant

  Scénario: Créer un bordereau d'entrée avec plusieurs produits
    Étant donné un magasin destination "MAG-001"
    Et une boutique avec l'ID "BOUT-123"
    Quand je crée un bordereau d'entrée avec:
      | Champ      | Valeur        |
      | Reference  | BE-2024-001   |
      | BoutiqueId | BOUT-123      |
      | Note       | Livraison du fournisseur X |
    Et j'ajoute les lignes suivantes au bordereau:
      | ProductId | Quantité | Prix Unitaire |
      | PROD-001  | 50       | 100.00        |
      | PROD-002  | 30       | 150.00        |
      | PROD-003  | 20       | 200.00        |
    Alors le bordereau est créé avec succès
    Et 3 mouvements de stock sont créés automatiquement
    Et chaque mouvement est lié au bordereau

  Scénario: Créer un bordereau de sortie pour transfert inter-magasin
    Étant donné un magasin source "MAG-001" de type "Store"
    Et un magasin destination "MAG-002" de type "Sale"
    Et une boutique avec l'ID "BOUT-123"
    Quand je crée un bordereau de sortie avec:
      | Champ                | Valeur      |
      | Reference            | BS-2024-001 |
      | BoutiqueId           | BOUT-123    |
      | Type                 | Transfer    |
      | SourceLocationId     | MAG-001     |
      | DestinationLocationId| MAG-002     |
    Et j'ajoute les produits à transférer:
      | ProductId | Quantité |
      | PROD-001  | 25       |
      | PROD-002  | 15       |
    Alors le bordereau est créé avec succès
    Et 2 mouvements de stock sont créés
    Et chaque mouvement indique le transfert de MAG-001 vers MAG-002

  Scénario: Consulter l'historique des mouvements d'un magasin
    Étant donné un magasin "MAG-001" avec des mouvements existants
    Quand je consulte l'historique des mouvements du magasin "MAG-001"
    Alors je vois tous les mouvements entrants (comme destination)
    Et je vois tous les mouvements sortants (comme source)
    Et les mouvements sont triés par date décroissante

  Scénario: Rechercher les mouvements par référence de bordereau
    Étant donné des bordereaux avec les références "BE-2024-001", "BS-2024-001"
    Quand je recherche les mouvements par référence "2024-001"
    Alors je trouve tous les mouvements liés aux bordereaux correspondants
    Et chaque mouvement affiche:
      | Information          |
      | Reference bordereau  |
      | Date                 |
      | Quantité             |
      | Produit ID           |
      | Magasin source       |
      | Magasin destination  |

  Scénario: Validation des règles métier pour les mouvements
    Quand je tente de créer un mouvement invalide:
      | Cas                                    | Résultat attendu                |
      | Quantité négative                      | Erreur: Quantité doit être > 0  |
      | Magasin source = destination           | Erreur: Source et destination identiques |
      | ProductId vide                         | Erreur: ProductId requis        |
      | Reference vide                         | Erreur: Reference requise       |
      | StockLocationId inexistant             | Erreur: Magasin non trouvé      |
    Alors le mouvement n'est pas créé
    Et j'obtiens un message d'erreur approprié

  Scénario: Annuler un mouvement de stock
    Étant donné un mouvement existant avec l'ID "MOV-123"
    Et que le mouvement n'a pas encore été confirmé/validé
    Quand j'annule le mouvement "MOV-123"
    Alors le mouvement est marqué comme annulé
    Et un mouvement inverse est créé automatiquement
    Et la date d'annulation est enregistrée

  Scénario: Générer un rapport des mouvements par période
    Étant donné des mouvements créés entre le "2024-01-01" et "2024-01-31"
    Quand je génère un rapport pour janvier 2024
    Avec les filtres:
      | Filtre      | Valeur   |
      | BoutiqueId  | BOUT-123 |
      | Type        | Transfer |
    Alors je reçois un rapport contenant:
      | Information                |
      | Total des mouvements       |
      | Quantité totale déplacée   |
      | Liste des produits         |
      | Top 10 produits mouvementés|
      | Magasins les plus actifs   |