namespace TresorerieService.Domain.Extensions;

/// <summary>
/// Extensions pour la conversion des DateTime en UTC.
/// PostgreSQL avec Npgsql exige des valeurs UTC pour les colonnes 'timestamp with time zone'.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Convertit un DateTime en UTC.
    /// - Si Kind=UTC: retourne tel quel
    /// - Si Kind=Local: convertit en UTC via ToUniversalTime()
    /// - Si Kind=Unspecified: marque comme UTC (DateTime.SpecifyKind)
    /// </summary>
    public static DateTime ToUtc(this DateTime dateTime)
    {
        return dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
        };
    }

    /// <summary>
    /// Convertit un DateTime nullable en UTC.
    /// Retourne null si la valeur est null.
    /// </summary>
    public static DateTime? ToUtc(this DateTime? dateTime)
    {
        return dateTime?.ToUtc();
    }
}
