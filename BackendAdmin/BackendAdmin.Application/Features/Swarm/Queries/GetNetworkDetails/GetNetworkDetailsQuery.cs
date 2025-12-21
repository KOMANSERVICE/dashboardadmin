using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetNetworkDetails;

public record GetNetworkDetailsQuery(string NetworkName) : IQuery<GetNetworkDetailsResult>;

public record GetNetworkDetailsResult(NetworkDetailsDTO Network);
