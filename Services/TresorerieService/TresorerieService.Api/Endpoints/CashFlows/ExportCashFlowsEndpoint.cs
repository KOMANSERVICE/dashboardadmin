using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using Microsoft.AspNetCore.Mvc;
using TresorerieService.Application.Features.CashFlows.Queries.ExportCashFlows;
using TresorerieService.Domain.Enums;

namespace TresorerieService.Api.Endpoints.CashFlows;

public class ExportCashFlowsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/cash-flows/export", async (
            [FromHeader(Name = "X-Application-Id")] string applicationId,
            [FromHeader(Name = "X-Boutique-Id")] string boutiqueId,
            [FromQuery] string format = "csv",
            [FromQuery] string? columns = null,
            [FromQuery] CashFlowType? type = null,
            [FromQuery] CashFlowStatus? status = null,
            [FromQuery] Guid? accountId = null,
            [FromQuery] Guid? categoryId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? search = null,
            ISender sender = null!,
            HttpContext httpContext = null!,
            CancellationToken cancellationToken = default) =>
        {
            if (string.IsNullOrEmpty(applicationId))
            {
                return Results.BadRequest(new { error = "L'en-tete X-Application-Id est obligatoire" });
            }

            if (string.IsNullOrEmpty(boutiqueId))
            {
                return Results.BadRequest(new { error = "L'en-tete X-Boutique-Id est obligatoire" });
            }

            // Valider le format
            if (!format.Equals("csv", StringComparison.OrdinalIgnoreCase) &&
                !format.Equals("excel", StringComparison.OrdinalIgnoreCase))
            {
                return Results.BadRequest(new { error = "Le format doit etre 'csv' ou 'excel'" });
            }

            // Extraire le role de l'utilisateur du token JWT
            var userRole = httpContext.User.FindFirst("role")?.Value
                           ?? httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                           ?? string.Empty;

            // Determiner si l'utilisateur est manager ou admin
            var isManager = userRole.Equals("manager", StringComparison.OrdinalIgnoreCase)
                            || userRole.Equals("admin", StringComparison.OrdinalIgnoreCase);

            // Parser le format d'export
            var exportFormat = format.Equals("excel", StringComparison.OrdinalIgnoreCase)
                ? ExportFormat.Excel
                : ExportFormat.Csv;

            // Parser les colonnes (separees par virgule)
            string[]? columnsArray = null;
            if (!string.IsNullOrWhiteSpace(columns))
            {
                columnsArray = columns.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            }

            var query = new ExportCashFlowsQuery(
                ApplicationId: applicationId,
                BoutiqueId: boutiqueId,
                IsManager: isManager,
                Format: exportFormat,
                Columns: columnsArray,
                Type: type,
                Status: status,
                AccountId: accountId,
                CategoryId: categoryId,
                StartDate: startDate,
                EndDate: endDate,
                Search: search
            );

            var result = await sender.Send(query, cancellationToken);

            return Results.File(
                result.FileStream,
                result.ContentType,
                result.FileName
            );
        })
        .WithName("ExportCashFlows")
        .WithTags("CashFlows")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithSummary("Exporter les flux de tresorerie")
        .WithDescription("Exporte les flux de tresorerie au format CSV ou Excel. " +
            "Les memes filtres que la liste sont disponibles. " +
            "Un employe exporte uniquement ses propres flux (CreatedBy = userId). " +
            "Un manager ou admin exporte tous les flux de la boutique. " +
            "Le parametre 'columns' permet de choisir les colonnes a exporter (separees par virgule). " +
            "Le nom du fichier contient la date d'export.")
        .RequireAuthorization();
    }
}
