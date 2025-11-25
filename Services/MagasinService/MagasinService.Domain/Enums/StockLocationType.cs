namespace MagasinService.Domain.Enums;

public enum StockLocationType
{

    [Description("Magasin de de vente")]
    Sale = 1,
    [Description("Entrepôt / Magasin de stockage")]
    Store = 2,
    [Description("Chantier")]
    Site = 3
}
