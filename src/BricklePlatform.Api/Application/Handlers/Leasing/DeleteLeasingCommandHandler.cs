using BricklePlatform.Api.Application.Commands.Leasing;
using BricklePlatform.Domain.Exceptions;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.Leasing;

public class DeleteLeasingCommandHandler : IRequestHandler<DeleteLeasingCommand>
{
    private readonly ILeasingRepository _leasingRepository;

    public DeleteLeasingCommandHandler(ILeasingRepository leasingRepository)
    {
        _leasingRepository = leasingRepository;
    }

    public async Task Handle(DeleteLeasingCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Leasing? leasing = await _leasingRepository.GetByIdAsync(request.Id);
        if (leasing == null)
            throw new NotFoundException($"Leasing con id {request.Id} no encontrado");

        await _leasingRepository.DeleteAsync(request.Id);
    }
}