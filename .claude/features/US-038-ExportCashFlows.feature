# language: fr
@US-038 @TresorerieService @Export
Fonctionnalite: US-038 - Exporter les flux de tresorerie
    En tant que manager
    Je veux exporter les flux de tresorerie
    Afin de faire des analyses externes

    Contexte:
        Soit un utilisateur authentifie avec le role "manager"
        Et les en-tetes obligatoires "X-Application-Id" et "X-Boutique-Id" sont fournis
        Et des flux de tresorerie existent dans la boutique

    # ============================================
    # CRITERE 1: Export au format CSV
    # ============================================

    @Export @CSV @HappyPath
    Scenario: Exporter les flux au format CSV
        Etant donne que je suis connecte en tant que manager
        Et que 10 flux de tresorerie existent pour ma boutique
        Quand je fais une requete GET sur "/api/cash-flows/export?format=csv"
        Alors je recois une reponse avec le code 200
        Et le Content-Type est "text/csv; charset=utf-8"
        Et le fichier contient les en-tetes de colonnes
        Et le fichier contient les 10 flux

    @Export @CSV @Filtres
    Scenario: Exporter les flux CSV avec filtres de date
        Etant donne que je suis connecte en tant que manager
        Et que des flux existent pour les dates du "2024-01-01" au "2024-12-31"
        Quand je fais une requete GET sur "/api/cash-flows/export?format=csv&startDate=2024-06-01&endDate=2024-06-30"
        Alors je recois une reponse avec le code 200
        Et le fichier contient uniquement les flux de juin 2024

    @Export @CSV @Filtres
    Scenario: Exporter les flux CSV avec filtre par type
        Etant donne que je suis connecte en tant que manager
        Et que des flux INCOME et EXPENSE existent
        Quand je fais une requete GET sur "/api/cash-flows/export?format=csv&type=INCOME"
        Alors je recois une reponse avec le code 200
        Et le fichier contient uniquement les flux de type INCOME

    @Export @CSV @Filtres
    Scenario: Exporter les flux CSV avec filtre par statut
        Etant donne que je suis connecte en tant que manager
        Et que des flux avec differents statuts existent
        Quand je fais une requete GET sur "/api/cash-flows/export?format=csv&status=APPROVED"
        Alors je recois une reponse avec le code 200
        Et le fichier contient uniquement les flux approuves

    @Export @CSV @Filtres
    Scenario: Exporter les flux CSV avec filtre par compte
        Etant donne que je suis connecte en tant que manager
        Et que des flux existent sur plusieurs comptes
        Quand je fais une requete GET sur "/api/cash-flows/export?format=csv&accountId={accountId}"
        Alors je recois une reponse avec le code 200
        Et le fichier contient uniquement les flux du compte specifie

    @Export @CSV @Filtres
    Scenario: Exporter les flux CSV avec filtre par categorie
        Etant donne que je suis connecte en tant que manager
        Et que des flux existent avec differentes categories
        Quand je fais une requete GET sur "/api/cash-flows/export?format=csv&categoryId={categoryId}"
        Alors je recois une reponse avec le code 200
        Et le fichier contient uniquement les flux de la categorie specifiee

    @Export @CSV @Recherche
    Scenario: Exporter les flux CSV avec recherche textuelle
        Etant donne que je suis connecte en tant que manager
        Et que des flux existent avec le label "Facture electricite"
        Quand je fais une requete GET sur "/api/cash-flows/export?format=csv&search=electricite"
        Alors je recois une reponse avec le code 200
        Et le fichier contient uniquement les flux correspondant a la recherche

    # ============================================
    # CRITERE 2: Export au format Excel
    # ============================================

    @Export @Excel @HappyPath
    Scenario: Exporter les flux au format Excel
        Etant donne que je suis connecte en tant que manager
        Et que 10 flux de tresorerie existent pour ma boutique
        Quand je fais une requete GET sur "/api/cash-flows/export?format=excel"
        Alors je recois une reponse avec le code 200
        Et le Content-Type est "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        Et le fichier Excel contient une feuille "Flux"
        Et la feuille contient les en-tetes de colonnes
        Et la feuille contient les 10 flux

    @Export @Excel @Filtres
    Scenario: Exporter les flux Excel avec tous les filtres combines
        Etant donne que je suis connecte en tant que manager
        Et que des flux varies existent
        Quand je fais une requete GET sur "/api/cash-flows/export?format=excel&type=EXPENSE&status=APPROVED&startDate=2024-01-01&endDate=2024-06-30"
        Alors je recois une reponse avec le code 200
        Et le fichier contient uniquement les flux EXPENSE approuves du premier semestre 2024

    # ============================================
    # CRITERE 3: Contenu des colonnes exportees
    # ============================================

    @Export @Colonnes @HappyPath
    Scenario: Verifier que l'export contient tous les champs des flux
        Etant donne que je suis connecte en tant que manager
        Et qu'un flux de tresorerie existe avec tous les champs remplis
        Quand je fais une requete GET sur "/api/cash-flows/export?format=csv"
        Alors le fichier contient les colonnes suivantes:
            | Colonne              |
            | Reference            |
            | Type                 |
            | Statut               |
            | Categorie            |
            | Libelle              |
            | Description          |
            | Montant              |
            | Taxes                |
            | Taux TVA             |
            | Devise               |
            | Compte               |
            | Compte destination   |
            | Mode de paiement     |
            | Date                 |
            | Type de tiers        |
            | Nom du tiers         |
            | ID tiers             |
            | Cree le              |
            | Cree par             |
            | Soumis le            |
            | Soumis par           |
            | Valide le            |
            | Valide par           |
            | Raison de rejet      |
            | Rapproche            |
            | Reference bancaire   |

    # ============================================
    # CRITERE 4: Selection des colonnes
    # ============================================

    @Export @Colonnes @Selection
    Scenario: Exporter uniquement les colonnes selectionnees
        Etant donne que je suis connecte en tant que manager
        Et que des flux existent
        Quand je fais une requete GET sur "/api/cash-flows/export?format=csv&columns=Reference,Type,Label,Amount,Date"
        Alors je recois une reponse avec le code 200
        Et le fichier contient exactement les colonnes:
            | Colonne   |
            | Reference |
            | Type      |
            | Libelle   |
            | Montant   |
            | Date      |

    @Export @Colonnes @Selection
    Scenario: Exporter avec une colonne invalide ignoree
        Etant donne que je suis connecte en tant que manager
        Quand je fais une requete GET sur "/api/cash-flows/export?format=csv&columns=Reference,ColonneInexistante,Amount"
        Alors je recois une reponse avec le code 200
        Et le fichier contient les colonnes Reference et Montant
        Et la colonne ColonneInexistante est ignoree

    @Export @Colonnes @Selection @Vide
    Scenario: Exporter sans specification de colonnes retourne toutes les colonnes
        Etant donne que je suis connecte en tant que manager
        Quand je fais une requete GET sur "/api/cash-flows/export?format=csv"
        Alors je recois une reponse avec le code 200
        Et le fichier contient toutes les colonnes disponibles

    # ============================================
    # CRITERE 5: Nom du fichier avec date
    # ============================================

    @Export @NomFichier @CSV
    Scenario: Le nom du fichier CSV contient la date d'export
        Etant donne que je suis connecte en tant que manager
        Et que la date du jour est "2024-12-17"
        Quand je fais une requete GET sur "/api/cash-flows/export?format=csv"
        Alors je recois une reponse avec le code 200
        Et l'en-tete Content-Disposition contient "flux-tresorerie-2024-12-17.csv"

    @Export @NomFichier @Excel
    Scenario: Le nom du fichier Excel contient la date d'export
        Etant donne que je suis connecte en tant que manager
        Et que la date du jour est "2024-12-17"
        Quand je fais une requete GET sur "/api/cash-flows/export?format=excel"
        Alors je recois une reponse avec le code 200
        Et l'en-tete Content-Disposition contient "flux-tresorerie-2024-12-17.xlsx"

    # ============================================
    # CAS D'ERREUR ET EDGE CASES
    # ============================================

    @Export @Erreur @FormatInvalide
    Scenario: Format d'export invalide retourne une erreur
        Etant donne que je suis connecte en tant que manager
        Quand je fais une requete GET sur "/api/cash-flows/export?format=pdf"
        Alors je recois une reponse avec le code 400
        Et le message d'erreur indique "Format d'export non supporte. Formats valides: csv, excel"

    @Export @Erreur @FormatManquant
    Scenario: Format d'export manquant utilise CSV par defaut
        Etant donne que je suis connecte en tant que manager
        Quand je fais une requete GET sur "/api/cash-flows/export"
        Alors je recois une reponse avec le code 200
        Et le Content-Type est "text/csv; charset=utf-8"

    @Export @Erreur @SansAuthentification
    Scenario: Export sans authentification retourne 401
        Etant donne que je ne suis pas authentifie
        Quand je fais une requete GET sur "/api/cash-flows/export?format=csv"
        Alors je recois une reponse avec le code 401

    @Export @Erreur @HeaderManquant
    Scenario: Export sans X-Application-Id retourne 400
        Etant donne que je suis connecte en tant que manager
        Et que l'en-tete "X-Application-Id" est absent
        Quand je fais une requete GET sur "/api/cash-flows/export?format=csv"
        Alors je recois une reponse avec le code 400
        Et le message d'erreur indique "L'en-tete X-Application-Id est obligatoire"

    @Export @Erreur @HeaderManquant
    Scenario: Export sans X-Boutique-Id retourne 400
        Etant donne que je suis connecte en tant que manager
        Et que l'en-tete "X-Boutique-Id" est absent
        Quand je fais une requete GET sur "/api/cash-flows/export?format=csv"
        Alors je recois une reponse avec le code 400
        Et le message d'erreur indique "L'en-tete X-Boutique-Id est obligatoire"

    @Export @Vide
    Scenario: Export sans flux retourne un fichier vide avec en-tetes
        Etant donne que je suis connecte en tant que manager
        Et qu'aucun flux n'existe pour ma boutique
        Quand je fais une requete GET sur "/api/cash-flows/export?format=csv"
        Alors je recois une reponse avec le code 200
        Et le fichier contient uniquement la ligne d'en-tetes

    # ============================================
    # SECURITE ET CONTROLE D'ACCES
    # ============================================

    @Export @Securite @Employee
    Scenario: Un employe exporte uniquement ses propres flux
        Etant donne que je suis connecte en tant que employee
        Et que 5 flux m'appartiennent et 10 flux appartiennent a d'autres
        Quand je fais une requete GET sur "/api/cash-flows/export?format=csv"
        Alors je recois une reponse avec le code 200
        Et le fichier contient exactement 5 flux

    @Export @Securite @Manager
    Scenario: Un manager exporte tous les flux de la boutique
        Etant donne que je suis connecte en tant que manager
        Et que 15 flux existent pour la boutique
        Quand je fais une requete GET sur "/api/cash-flows/export?format=csv"
        Alors je recois une reponse avec le code 200
        Et le fichier contient exactement 15 flux

    @Export @Securite @Admin
    Scenario: Un admin exporte tous les flux de la boutique
        Etant donne que je suis connecte en tant que admin
        Et que 15 flux existent pour la boutique
        Quand je fais une requete GET sur "/api/cash-flows/export?format=csv"
        Alors je recois une reponse avec le code 200
        Et le fichier contient exactement 15 flux

    # ============================================
    # PERFORMANCES
    # ============================================

    @Export @Performance
    Scenario: Export de grands volumes de donnees
        Etant donne que je suis connecte en tant que manager
        Et que 10000 flux existent pour ma boutique
        Quand je fais une requete GET sur "/api/cash-flows/export?format=csv"
        Alors je recois une reponse avec le code 200
        Et le temps de reponse est inferieur a 30 secondes
        Et le fichier contient les 10000 flux

    @Export @Performance @Streaming
    Scenario: Export utilise le streaming pour gros fichiers
        Etant donne que je suis connecte en tant que manager
        Et que 5000 flux existent
        Quand je fais une requete GET sur "/api/cash-flows/export?format=excel"
        Alors la memoire utilisee reste stable pendant l'export
        Et le fichier est telecharge progressivement
