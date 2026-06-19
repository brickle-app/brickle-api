using BricklePlatform.Api.Application.Queries.User;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.User;

public class GetUserContactsQueryHandler : IRequestHandler<GetUserContactsQuery, IEnumerable<ContactDto>>
{
    private readonly IUserContactRepository _userContactRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetUserContactsQueryHandler> _logger;

    public GetUserContactsQueryHandler(
        IUserContactRepository userContactRepository,
        IUserRepository userRepository,
        ILogger<GetUserContactsQueryHandler> logger)
    {
        _userContactRepository = userContactRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ContactDto>> Handle(GetUserContactsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Obteniendo contactos del usuario {UserId} - CorrelationId: {CorrelationId}",
            request.UserId, request.Header.CorrelationId);

        Domain.Entities.User? user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new ArgumentException($"Usuario con ID {request.UserId} no encontrado");
        }

        IEnumerable<Domain.Entities.UserContact> userContacts = await _userContactRepository.GetContactsByUserIdAsync(request.UserId);
        List<ContactDto> contactDtos = new List<ContactDto>();

        foreach (Domain.Entities.UserContact userContact in userContacts)
        {
            Domain.Entities.User? contact = await _userRepository.GetByIdAsync(userContact.ContactId);
            if (contact != null)
            {
                contactDtos.Add(new ContactDto
                {
                    Id = contact.Id,
                    FirstName = contact.FirstName,
                    LastName = contact.LastName,
                    Email = contact.Email,
                    PhoneNumber = contact.PhoneNumber,
                    WalletAddress = contact.WalletAddress,
                    ProfilePictureUrl = contact.ProfilePictureUrl
                });
            }
        }

        return contactDtos;
    }
}