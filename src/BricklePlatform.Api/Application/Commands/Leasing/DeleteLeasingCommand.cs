using MediatR;

namespace BricklePlatform.Api.Application.Commands.Leasing;

public class DeleteLeasingCommand : IRequest
{
    public Guid Id { get; set; }

    public DeleteLeasingCommand(Guid id)
    {
        Id = id;
    }
}