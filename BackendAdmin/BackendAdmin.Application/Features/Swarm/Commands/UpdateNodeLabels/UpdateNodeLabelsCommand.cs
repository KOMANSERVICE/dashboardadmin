namespace BackendAdmin.Application.Features.Swarm.Commands.UpdateNodeLabels;

public record UpdateNodeLabelsCommand(string NodeId, Dictionary<string, string> Labels) : ICommand<UpdateNodeLabelsResult>;

public record UpdateNodeLabelsResult(string Message);
