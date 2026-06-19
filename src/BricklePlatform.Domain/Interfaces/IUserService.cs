using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Entities;

namespace BricklePlatform.Domain.Interfaces;

public interface IUserService
{
    Task<User> CreateUserAsync(CreateUserDto userDto);

    Task<User?> GetUserAsync(Guid id);

    Task<User?> GetUserByEmailAsync(string email);

    Task<User> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto);

    Task DeleteUserAsync(Guid id);

    Task<bool> ValidatePasswordAsync(string email, string password);

    Task<bool> UpdatePasswordAsync(Guid userId, string currentPassword, string newPassword);

    Task<bool> ResetPasswordAsync(string email, string newPassword);
}