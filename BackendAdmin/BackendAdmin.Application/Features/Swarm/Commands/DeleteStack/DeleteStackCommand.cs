namespace BackendAdmin.Application.Features.Swarm.Commands.DeleteStack;

public record DeleteStackCommand(string StackName) : ICommand<DeleteStackResult>;

public record DeleteStackResult(string StackName);
