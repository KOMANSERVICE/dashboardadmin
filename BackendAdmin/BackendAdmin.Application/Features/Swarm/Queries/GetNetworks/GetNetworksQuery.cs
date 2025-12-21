using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetNetworks;

public record GetNetworksQuery() : IQuery<GetNetworksResult>;

public record GetNetworksResult(List<NetworkDTO> Networks);
