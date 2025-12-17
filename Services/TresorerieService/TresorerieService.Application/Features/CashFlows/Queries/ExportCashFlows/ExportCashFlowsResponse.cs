namespace TresorerieService.Application.Features.CashFlows.Queries.ExportCashFlows;

/// <summary>
/// Reponse de l'export des flux de tresorerie
/// </summary>
public record ExportCashFlowsResponse(
    /// <summary>
    /// Stream contenant le fichier exporte
    /// </summary>
    Stream FileStream,

    /// <summary>
    /// Nom du fichier avec date d'export
    /// </summary>
    string FileName,

    /// <summary>
    /// Type MIME du fichier
    /// </summary>
    string ContentType
);
