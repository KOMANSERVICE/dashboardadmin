using System.ComponentModel;

namespace MagasinService.Domain.Enums;

public enum StockMovementType
{
    [Description("Entrée de stock")]
    In = 1,

    [Description("Sortie de stock")]
    Out = 2,

    [Description("Transfert entre magasins")]
    Transfer = 3
}