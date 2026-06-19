using BricklePlatform.Api.Application.Commands.User;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.User;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<DeleteUserHandler> _logger;

    public DeleteUserHandler(
        IUserRepository userRepository,
        ILogger<DeleteUserHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting user with ID: {UserId}. CorrelationId: {CorrelationId}",
                request.UserId, request.Header.CorrelationId);

            Domain.Entities.User? user = await _userRepository.GetByIdAsync(request.UserId);
            if (user is null)
            {
                throw new ApplicationException($"No se encontró  el usuario con Id {request.UserId}");
            }

            await _userRepository.DeleteAsync(request.UserId);

            _logger.LogInformation("Successfully deleted user with ID: {UserId}. CorrelationId: {CorrelationId}",
                request.UserId, request.Header.CorrelationId);

            return Unit.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID: {UserId}. CorrelationId: {CorrelationId}",
                request.UserId, request.Header.CorrelationId);
            throw;
        }
    }
}