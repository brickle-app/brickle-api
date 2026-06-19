using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Entities;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Constants;
using BricklePlatform.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace BricklePlatform.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IBlobStorageRepository _blobStorageRepository;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IBlobStorageRepository blobStorageRepository,
        IPasswordService passwordService,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _blobStorageRepository = blobStorageRepository;
        _passwordService = passwordService;
        _logger = logger;
    }

    public async Task<User> CreateUserAsync(CreateUserDto userDto)
    {
        if (userDto == null)
            throw new ArgumentNullException(nameof(userDto));

        User? existingUser = await _userRepository.GetByEmailAsync(userDto.Email);
        if (existingUser != null)
            throw new InvalidOperationException("El email ya está registrado");

        (byte[] passwordHash, byte[] passwordSalt) = _passwordService.HashPassword(userDto.Password);

        User user = User.Create(
            firstName: userDto.FirstName,
            lastName: userDto.LastName,
            email: userDto.Email,
            phoneNumber: userDto.PhoneNumber,
            termsAccepted: userDto.TermsAccepted,
            passwordHash: passwordHash,
            passwordSalt: passwordSalt,
            walletAddress: userDto.WalletAddress,
            dateOfBirth: userDto.DateOfBirth,
            nationality: userDto.Nationality,
            countryOfResidence: userDto.CountryOfResidence,
            documentType: userDto.DocumentType,
            documentNumber: userDto.DocumentNumber,
            kycCustomerId: userDto.KycCustomerId,
            kycSubmissionId: userDto.KycSubmissionId,
            pushNotificationToken: userDto.PushNotificationToken
        );
        user = await _userRepository.AddAsync(user);

        return user;
    }

    public async Task<User?> GetUserAsync(Guid id)
    {
        User? user = await _userRepository.GetByIdAsync(id);
        if (user != null)
        {
            user.ClearSensitiveData();
        }
        return user;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email no puede estar vacío", nameof(email));

        User? user = await _userRepository.GetByEmailAsync(email);
        if (user != null)
        {
            user.ClearSensitiveData();
        }
        return user;
    }

    public async Task<bool> ValidatePasswordAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email no puede estar vacío", nameof(email));
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("La contraseña no puede estar vacía", nameof(password));

        User? user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            return false;
        }

        return _passwordService.VerifyPassword(password, user.PasswordHash, user.PasswordSalt);
    }

    public async Task<bool> UpdatePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(currentPassword))
            throw new ArgumentException("La contraseña actual no puede estar vacía", nameof(currentPassword));
        if (string.IsNullOrWhiteSpace(newPassword))
            throw new ArgumentException("La nueva contraseña no puede estar vacía", nameof(newPassword));

        User? user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        if (!_passwordService.VerifyPassword(currentPassword, user.PasswordHash, user.PasswordSalt))
        {
            return false;
        }

        (byte[] newPasswordHash, byte[] newPasswordSalt) = _passwordService.HashPassword(newPassword);
        user.UpdatePassword(newPasswordHash, newPasswordSalt);
        await _userRepository.UpdateAsync(user);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(string email, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email no puede estar vacío", nameof(email));
        if (string.IsNullOrWhiteSpace(newPassword))
            throw new ArgumentException("La nueva contraseña no puede estar vacía", nameof(newPassword));

        User? user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            return false;
        }

        (byte[] newPasswordHash, byte[] newPasswordSalt) = _passwordService.HashPassword(newPassword);
        user.UpdatePassword(newPasswordHash, newPasswordSalt);
        await _userRepository.UpdateAsync(user);

        return true;
    }

    public async Task<User> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto)
    {
        if (string.IsNullOrWhiteSpace(updateUserDto.FirstName))
            throw new ArgumentException("El nombre no puede estar vacío", nameof(updateUserDto.FirstName));

        if (string.IsNullOrWhiteSpace(updateUserDto.LastName))
            throw new ArgumentException("El apellido no puede estar vacío", nameof(updateUserDto.LastName));

        User? user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException("Usuario no encontrado");
        }

        user.Update(
            firstName: updateUserDto.FirstName,
            lastName: updateUserDto.LastName,
            phoneNumber: updateUserDto.PhoneNumber,
            profilePictureUrl: updateUserDto.ProfilePictureUrl,
            walletAddress: updateUserDto.WalletAddress,
            dateOfBirth: updateUserDto.DateOfBirth,
            nationality: updateUserDto.Nationality,
            countryOfResidence: updateUserDto.CountryOfResidence,
            documentType: updateUserDto.DocumentType,
            documentNumber: updateUserDto.DocumentNumber,
            kycCustomerId: updateUserDto.KycCustomerId,
            kycSubmissionId: updateUserDto.KycSubmissionId,
            pushNotificationToken: updateUserDto.PushNotificationToken,
            isBasicProfileComplete: updateUserDto.IsBasicProfileComplete,
            isFullProfileComplete: updateUserDto.IsFullProfileComplete
        );
        await _userRepository.UpdateAsync(user);
        return user;
    }

    public async Task DeleteUserAsync(Guid id)
    {
        User? user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException("Usuario no encontrado");
        }

        // Primero eliminamos los archivos del usuario
        await _blobStorageRepository.DeleteEntityFilesAsync(BlobConstants.EntityTypes.USER, id);

        // Luego eliminamos el usuario de la base de datos
        await _userRepository.DeleteAsync(id);
    }
}