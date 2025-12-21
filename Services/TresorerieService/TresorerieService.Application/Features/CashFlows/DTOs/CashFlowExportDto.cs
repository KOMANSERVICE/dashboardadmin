namespace TresorerieService.Application.Features.CashFlows.DTOs;

/// <summary>
/// DTO pour l'export des flux de tresorerie avec noms de colonnes en francais
/// </summary>
public record CashFlowExportDto(
    string Reference,
    string Type,
    string Statut,
    string Categorie,
    string Libelle,
    string? Description,
    decimal Montant,
    decimal Taxes,
    decimal TauxTVA,
    string Devise,
    string Compte,
    string? CompteDestination,
    string ModePaiement,
    DateTime Date,
    string? TypeDeTiers,
    string? NomDuTiers,
    string? IdTiers,
    DateTime CreeLe,
    string CreePar,
    DateTime? SoumisLe,
    string? SoumisPar,
    DateTime? ValideLe,
    string? ValidePar,
    string? RaisonDeRejet,
    string Rapproche,
    string? ReferenceBancaire
);

/// <summary>
/// Liste des colonnes disponibles pour l'export avec leurs identifiants
/// </summary>
public static class ExportColumns
{
    public const string Reference = "reference";
    public const string Type = "type";
    public const string Statut = "statut";
    public const string Categorie = "categorie";
    public const string Libelle = "libelle";
    public const string Description = "description";
    public const string Montant = "montant";
    public const string Taxes = "taxes";
    public const string TauxTVA = "taux_tva";
    public const string Devise = "devise";
    public const string Compte = "compte";
    public const string CompteDestination = "compte_destination";
    public const string ModePaiement = "mode_paiement";
    public const string Date = "date";
    public const string TypeDeTiers = "type_tiers";
    public const string NomDuTiers = "nom_tiers";
    public const string IdTiers = "id_tiers";
    public const string CreeLe = "cree_le";
    public const string CreePar = "cree_par";
    public const string SoumisLe = "soumis_le";
    public const string SoumisPar = "soumis_par";
    public const string ValideLe = "valide_le";
    public const string ValidePar = "valide_par";
    public const string RaisonDeRejet = "raison_rejet";
    public const string Rapproche = "rapproche";
    public const string ReferenceBancaire = "reference_bancaire";

    /// <summary>
    /// Liste de toutes les colonnes disponibles (par defaut)
    /// </summary>
    public static readonly string[] AllColumns =
    [
        Reference,
        Type,
        Statut,
        Categorie,
        Libelle,
        Description,
        Montant,
        Taxes,
        TauxTVA,
        Devise,
        Compte,
        CompteDestination,
        ModePaiement,
        Date,
        TypeDeTiers,
        NomDuTiers,
        IdTiers,
        CreeLe,
        CreePar,
        SoumisLe,
        SoumisPar,
        ValideLe,
        ValidePar,
        RaisonDeRejet,
        Rapproche,
        ReferenceBancaire
    ];

    /// <summary>
    /// Mapping des identifiants de colonnes vers les noms d'affichage en francais
    /// </summary>
    public static readonly Dictionary<string, string> ColumnDisplayNames = new()
    {
        { Reference, "Reference" },
        { Type, "Type" },
        { Statut, "Statut" },
        { Categorie, "Categorie" },
        { Libelle, "Libelle" },
        { Description, "Description" },
        { Montant, "Montant" },
        { Taxes, "Taxes" },
        { TauxTVA, "Taux TVA" },
        { Devise, "Devise" },
        { Compte, "Compte" },
        { CompteDestination, "Compte destination" },
        { ModePaiement, "Mode de paiement" },
        { Date, "Date" },
        { TypeDeTiers, "Type de tiers" },
        { NomDuTiers, "Nom du tiers" },
        { IdTiers, "ID tiers" },
        { CreeLe, "Cree le" },
        { CreePar, "Cree par" },
        { SoumisLe, "Soumis le" },
        { SoumisPar, "Soumis par" },
        { ValideLe, "Valide le" },
        { ValidePar, "Valide par" },
        { RaisonDeRejet, "Raison de rejet" },
        { Rapproche, "Rapproche" },
        { ReferenceBancaire, "Reference bancaire" }
    };

    /// <summary>
    /// Verifie si une colonne est valide
    /// </summary>
    public static bool IsValidColumn(string column)
    {
        return AllColumns.Contains(column.ToLowerInvariant());
    }

    /// <summary>
    /// Recupere le nom d'affichage d'une colonne
    /// </summary>
    public static string GetDisplayName(string column)
    {
        return ColumnDisplayNames.TryGetValue(column.ToLowerInvariant(), out var name) ? name : column;
    }
}
