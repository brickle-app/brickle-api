using BricklePlatform.Api.Application.Commands.User;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.User;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<CreateUserHandler> _logger;

    public CreateUserHandler(
        IUserRepository userRepository,
        IPasswordService passwordService,
        ILogger<CreateUserHandler> logger)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _logger = logger;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating new user with email: {Email}. CorrelationId: {CorrelationId}",
                request.Body.Email, request.Header.CorrelationId);

            // Check if user already exists
            Domain.Entities.User? existingUser = await _userRepository.GetByEmailAsync(request.Body.Email);
            if (existingUser != null)
            {
                throw new ApplicationException($"El usuario con el correo electrónico {request.Body.Email} ya existe");
            }

            (byte[] hash, byte[] salt) = _passwordService.HashPassword(request.Body.Password);

            Domain.Entities.User user = Domain.Entities.User.Create(
                firstName: request.Body.FirstName,
                lastName: request.Body.LastName,
                email: request.Body.Email,
                phoneNumber: request.Body.PhoneNumber,
                termsAccepted: request.Body.TermsAccepted,
                passwordHash: hash,
                passwordSalt: salt,
                walletAddress: request.Body.WalletAddress,
                dateOfBirth: request.Body.DateOfBirth,
                nationality: request.Body.Nationality,
                countryOfResidence: request.Body.CountryOfResidence,
                documentType: request.Body.DocumentType,
                documentNumber: request.Body.DocumentNumber,
                kycCustomerId: request.Body.KycCustomerId,
                kycSubmissionId: request.Body.KycSubmissionId,
                pushNotificationToken: request.Body.PushNotificationToken,
                currentSession: request.Body.CurrentSession,
                externalWalletId: request.Body.ExternalWalletId
            );

            await _userRepository.AddAsync(user);

            _logger.LogInformation("Successfully created user with ID: {UserId}. CorrelationId: {CorrelationId}",
                user.Id, request.Header.CorrelationId);

            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ProfilePictureUrl = user.ProfilePictureUrl,
                WalletAddress = user.WalletAddress,
                PhoneNumber = user.PhoneNumber,
                TermsAccepted = user.TermsAccepted,
                DateOfBirth = user.DateOfBirth,
                Nationality = user.Nationality,
                CountryOfResidence = user.CountryOfResidence,
                DocumentType = user.DocumentType,
                DocumentNumber = user.DocumentNumber,
                KycCustomerId = user.KycCustomerId,
                KycSubmissionId = user.KycSubmissionId,
                PushNotificationToken = user.PushNotificationToken,
                CurrentSession = user.CurrentSession,
                ExternalWalletId = user.ExternalWalletId,
                CreatedAt = user.CreatedAt,
                IsBasicProfileComplete = user.IsBasicProfileComplete,
                IsFullProfileComplete = user.IsFullProfileComplete,
                Company = user.Company != null ? new CompanyDto
                {
                    Id = user.Company.Id,
                    Name = user.Company.Name,
                    OperationTime = user.Company.OperationTime,
                    OperationMeasure = user.Company.OperationMeasure,
                    CreditRating = user.Company.CreditRating,
                    LeasingContract = user.Company.LeasingContract,
                    UserId = user.Company.UserId,
                    CreatedAt = user.Company.CreatedAt,
                    UpdatedAt = user.Company.UpdatedAt
                } : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user with email: {Email}. CorrelationId: {CorrelationId}",
                request.Body.Email, request.Header.CorrelationId);
            throw new ApplicationException("Se produjo un error al crear el usuario");
        }
    }
}