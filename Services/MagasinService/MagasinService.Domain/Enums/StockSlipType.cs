namespace MagasinService.Domain.Enums;

public enum StockSlipType
{
    [Description("Bordereau d'entrée")]
    Entry = 1,

    [Description("Bordereau de sortie")]
    Exit = 2,

    [Description("Bordereau de transfert")]
    Transfer = 3
}