namespace BackendAdmin.Application.Features.Swarm.DTOs;

public record ScaleServiceRequest(int Replicas);

public record ScaleServiceResponse(
    string ServiceName,
    int PreviousReplicas,
    int NewReplicas
);
