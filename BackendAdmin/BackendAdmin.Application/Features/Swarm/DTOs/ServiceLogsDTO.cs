namespace BackendAdmin.Application.Features.Swarm.DTOs;

public record ServiceLogsDTO(
    string ServiceName,
    string Logs,
    DateTime FetchedAt
);
