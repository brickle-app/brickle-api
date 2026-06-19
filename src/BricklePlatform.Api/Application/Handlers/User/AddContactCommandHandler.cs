using BricklePlatform.Api.Application.Commands.User;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Entities;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.User;

public class AddContactCommandHandler : IRequestHandler<AddContactCommand, ContactDto>
{
    private readonly IUserContactRepository _userContactRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AddContactCommandHandler> _logger;

    public AddContactCommandHandler(
        IUserContactRepository userContactRepository,
        IUserRepository userRepository,
        ILogger<AddContactCommandHandler> logger)
    {
        _userContactRepository = userContactRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<ContactDto> Handle(AddContactCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Agregando contacto {ContactId} al usuario {UserId} - CorrelationId: {CorrelationId}",
            request.AddContactDto.ContactId, request.UserId, request.Header.CorrelationId);

        Domain.Entities.User? user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new ArgumentException($"Usuario con ID {request.UserId} no encontrado");
        }

        Domain.Entities.User? contact = await _userRepository.GetByIdAsync(request.AddContactDto.ContactId);
        if (contact == null)
        {
            throw new ArgumentException($"Contacto con ID {request.AddContactDto.ContactId} no encontrado");
        }

        bool contactExists = await _userContactRepository.ContactExistsAsync(request.UserId, request.AddContactDto.ContactId);
        if (contactExists)
        {
            throw new InvalidOperationException($"El contacto ya está agregado al usuario");
        }

        UserContact userContact = UserContact.Create(request.UserId, request.AddContactDto.ContactId);
        await _userContactRepository.AddContactAsync(userContact);

        return new ContactDto
        {
            Id = contact.Id,
            FirstName = contact.FirstName,
            LastName = contact.LastName,
            Email = contact.Email,
            PhoneNumber = contact.PhoneNumber,
            WalletAddress = contact.WalletAddress,
            ProfilePictureUrl = contact.ProfilePictureUrl
        };
    }
}