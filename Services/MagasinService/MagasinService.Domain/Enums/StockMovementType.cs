namespace MagasinService.Domain.Enums;

public enum StockMovementType
{
    [Description("Entrée de stock")]
    In = 1,

    [Description("Sortie de stock")]
    Out = 2,

    [Description("Transfert entre magasins")]
    Transfer = 3,

    [Description("Ajustement d'inventaire")]
    Adjustment = 4,

    [Description("Retour de produits")]
    Return = 5
}