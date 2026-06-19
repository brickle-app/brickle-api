using BricklePlatform.Api.Application.Commands.User;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Services;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.User;

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<UpdateUserHandler> _logger;

    public UpdateUserHandler(
        IUserRepository userRepository,
        IEmailService emailService,
        INotificationService notificationService,
        ILogger<UpdateUserHandler> logger)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating user with ID: {UserId}. CorrelationId: {CorrelationId}",
                request.UserId, request.Header.CorrelationId);

            Domain.Entities.User? user = await _userRepository.GetByIdAsync(request.UserId);
            if (user is null)
            {
                throw new ApplicationException($"No se encontró el usuario con Id {request.UserId}");
            }

            // Capture previous state before updating
            bool wasUnderReview = user.IsProfileUnderReview;
            bool wasFullProfileComplete = user.IsFullProfileComplete;

            user.Update(
                firstName: request.Body.FirstName,
                lastName: request.Body.LastName,
                phoneNumber: request.Body.PhoneNumber,
                profilePictureUrl: request.Body.ProfilePictureUrl,
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
                externalWalletId: request.Body.ExternalWalletId,
                isBasicProfileComplete: request.Body.IsBasicProfileComplete,
                isFullProfileComplete: request.Body.IsFullProfileComplete,
                isProfileUnderReview: request.Body.IsProfileUnderReview
            );

            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("Successfully updated user with ID: {UserId}. CorrelationId: {CorrelationId}",
                user.Id, request.Header.CorrelationId);

            var userName = $"{user.FirstName} {user.LastName}".Trim();

            // Send "profile under review" email when the flag is first set
            if (!wasUnderReview && user.IsProfileUnderReview)
            {
                _ = Task.Run(async () =>
                {
                    try { await _emailService.SendProfileUnderReviewAsync(user.Email, userName); }
                    catch (Exception ex) { _logger.LogError(ex, "Failed to send profile-under-review email to {Email}", user.Email); }
                }, CancellationToken.None);
            }

            // Send "profile approved" email + push when full profile is first completed
            if (!wasFullProfileComplete && user.IsFullProfileComplete)
            {
                _ = Task.Run(async () =>
                {
                    try { await _emailService.SendProfileApprovedAsync(user.Email, userName); }
                    catch (Exception ex) { _logger.LogError(ex, "Failed to send profile-approved email to {Email}", user.Email); }
                }, CancellationToken.None);

                if (!string.IsNullOrWhiteSpace(user.PushNotificationToken))
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _notificationService.SendNotificationAsync(
                                user.PushNotificationToken,
                                "¡Perfil aprobado!",
                                "Tu identidad fue verificada. Ya puedes invertir en activos reales con Brickle.",
                                new { type = "profile_approved" });
                        }
                        catch (Exception ex) { _logger.LogError(ex, "Failed to send profile-approved push to user {UserId}", user.Id); }
                    }, CancellationToken.None);
                }
            }

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
                IsProfileUnderReview = user.IsProfileUnderReview,
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
            _logger.LogError(ex, "Error updating user with ID: {UserId}. CorrelationId: {CorrelationId}",
                request.UserId, request.Header.CorrelationId);
            throw;
        }
    }
}