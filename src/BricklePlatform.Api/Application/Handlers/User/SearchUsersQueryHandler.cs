using BricklePlatform.Api.Application.Queries.User;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.User;

public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, IEnumerable<ContactDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SearchUsersQueryHandler> _logger;

    public SearchUsersQueryHandler(
        IUserRepository userRepository,
        ILogger<SearchUsersQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ContactDto>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando usuarios - SearchTerm: {SearchTerm}, ExcludeUserId: {ExcludeUserId} - CorrelationId: {CorrelationId}",
            request.SearchTerm, request.ExcludeUserId, request.Header.CorrelationId);

        string? email = null;
        string? phoneNumber = null;

        if (IsValidEmail(request.SearchTerm))
        {
            email = request.SearchTerm;
        }
        else if (IsValidPhoneNumber(request.SearchTerm))
        {
            phoneNumber = request.SearchTerm;
        }
        else
        {
            // Búsqueda parcial para autocompletado (ej: "orbitatech328@" o "orbitatech328@g")
            if (request.SearchTerm.Contains('@'))
                email = request.SearchTerm;
            else if (request.SearchTerm.All(char.IsDigit))
                phoneNumber = request.SearchTerm;
            else
                email = request.SearchTerm; // Por defecto buscar en email
        }

        if (request.ExcludeUserId.HasValue)
        {
            Domain.Entities.User? excludedUser = null;
            string searchType = "";

            if (email != null)
            {
                excludedUser = await _userRepository.GetByEmailAsync(email);
                searchType = "email";
            }
            else if (phoneNumber != null)
            {
                excludedUser = await _userRepository.GetByPhoneNumberAsync(phoneNumber);
                searchType = "teléfono";
            }

            if (excludedUser != null && excludedUser.Id == request.ExcludeUserId.Value)
            {
                _logger.LogWarning("El usuario está buscándose a sí mismo por {SearchType} - UserId: {UserId} - CorrelationId: {CorrelationId}",
                    searchType, request.ExcludeUserId.Value, request.Header.CorrelationId);

                throw new KeyNotFoundException("No se encontraron usuarios con los criterios de búsqueda proporcionados. Verifica los datos e intenta nuevamente.");
            }
        }

        IEnumerable<Domain.Entities.User> users = await _userRepository.SearchUsersAsync(email, phoneNumber, request.ExcludeUserId);

        if (!users.Any())
        {
            _logger.LogWarning("No se encontraron usuarios con los criterios de búsqueda - SearchTerm: {SearchTerm} - CorrelationId: {CorrelationId}",
                request.SearchTerm, request.Header.CorrelationId);

            throw new KeyNotFoundException("No se encontraron usuarios con los criterios de búsqueda proporcionados. Verifica los datos e intenta nuevamente.");
        }

        return users.Select(user => new ContactDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            WalletAddress = user.WalletAddress,
            ProfilePictureUrl = user.ProfilePictureUrl
        });
    }

    private bool IsValidEmail(string? email)
    {
        if (string.IsNullOrEmpty(email)) return false;

        try
        {
            System.Net.Mail.MailAddress emailAddress = new System.Net.Mail.MailAddress(email);
            return emailAddress.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidPhoneNumber(string? phoneNumber)
    {
        return !string.IsNullOrEmpty(phoneNumber) && phoneNumber.All(char.IsDigit);
    }
}