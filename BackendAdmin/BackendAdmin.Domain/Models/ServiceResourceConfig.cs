namespace BackendAdmin.Domain.Models;

/// <summary>
/// Configuration des ressources et limites pour un service Docker Swarm.
/// Persiste les valeurs pour les reappliquer apres un redeploiement.
/// </summary>
[Table("TK00002")]
public class ServiceResourceConfig
{
    [Column("rc1")]
    public Guid Id { get; set; }

    /// <summary>
    /// Nom unique du service Docker Swarm
    /// </summary>
    [Column("rc2")]
    public string ServiceName { get; set; } = default!;

    /// <summary>
    /// Limite CPU en NanoCPUs (1 core = 1_000_000_000)
    /// </summary>
    [Column("rc3")]
    public long? CpuLimit { get; set; }

    /// <summary>
    /// Reservation CPU en NanoCPUs (garantie)
    /// </summary>
    [Column("rc4")]
    public long? CpuReservation { get; set; }

    /// <summary>
    /// Limite memoire en bytes
    /// </summary>
    [Column("rc5")]
    public long? MemoryLimit { get; set; }

    /// <summary>
    /// Reservation memoire en bytes (garantie)
    /// </summary>
    [Column("rc6")]
    public long? MemoryReservation { get; set; }

    /// <summary>
    /// Limite du nombre de processus (PIDs)
    /// </summary>
    [Column("rc7")]
    public long? PidsLimit { get; set; }

    /// <summary>
    /// Poids I/O disque (1-1000)
    /// </summary>
    [Column("rc8")]
    public int? BlkioWeight { get; set; }

    /// <summary>
    /// Ulimits au format JSON
    /// </summary>
    [Column("rc9")]
    public string? UlimitsJson { get; set; }

    [Column("rc10")]
    public DateTime CreatedAt { get; set; }

    [Column("rc11")]
    public DateTime UpdatedAt { get; set; }
}
