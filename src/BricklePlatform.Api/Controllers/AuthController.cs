using System.ComponentModel.DataAnnotations;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BricklePlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IJwtService _jwtService;
    private readonly IUserRepository _userRepository;
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AuthController> _logger;

    private const string OtpCachePrefix = "otp_";
    private static readonly TimeSpan OtpExpiry = TimeSpan.FromMinutes(5);

    public AuthController(
        IJwtService jwtService,
        IUserRepository userRepository,
        IUserService userService,
        IEmailService emailService,
        IMemoryCache cache,
        ILogger<AuthController> logger)
    {
        _jwtService = jwtService;
        _userRepository = userRepository;
        _userService = userService;
        _emailService = emailService;
        _cache = cache;
        _logger = logger;
    }

    [HttpPost("google")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GoogleSignIn([FromBody] GoogleAuthRequest request)
    {
        try
        {
            GoogleJsonWebSignature.Payload payload;
            try
            {
                var validationSettings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { request.ClientId ?? "" }
                };

                if (string.IsNullOrEmpty(request.ClientId))
                    payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken);
                else
                    payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, validationSettings);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Google token verification failed");
                return Unauthorized(new { error = "Invalid Google token" });
            }

            var email = payload.Email;
            var name = payload.Name;
            var picture = payload.Picture;

            if (string.IsNullOrEmpty(email))
                return Unauthorized(new { error = "Email not available from Google account" });

            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
            {
                var nameParts = (name ?? email).Split(' ', 2, StringSplitOptions.TrimEntries);
                var firstName = nameParts[0];
                var lastName = nameParts.Length > 1 ? nameParts[1] : "";

                user = BricklePlatform.Domain.Entities.User.Create(
                    firstName: firstName,
                    lastName: lastName,
                    email: email,
                    phoneNumber: "",
                    termsAccepted: true,
                    passwordHash: Array.Empty<byte>(),
                    passwordSalt: Array.Empty<byte>(),
                    profilePictureUrl: picture);

                user = await _userRepository.AddAsync(user);
                _logger.LogInformation("New user created via Google Sign-In: {Email}", email);
            }
            else
            {
                var needsUpdate = false;
                if (!string.IsNullOrEmpty(picture))
                {
                    user.UpdateProfilePicture(picture);
                    needsUpdate = true;
                }
                if (needsUpdate)
                    await _userRepository.UpdateAsync(user);
            }

            var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Email);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var cacheKey = $"refresh_{refreshToken}";
            _cache.Set(cacheKey, user.Id.ToString(), TimeSpan.FromDays(30));

            var userDto = MapToDto(user);

            return Ok(new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = userDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google Sign-In failed for token");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    [HttpPost("send-otp")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { error = "Email is required" });

        try
        {
            var otpCode = GenerateOtp();

            var cacheKey = $"{OtpCachePrefix}{request.Email.ToLowerInvariant()}";
            _cache.Set(cacheKey, otpCode, OtpExpiry);

            var userName = request.Email.Split('@')[0];

            await _emailService.SendOtpEmailAsync(request.Email, userName, otpCode);

            _logger.LogInformation("OTP sent to {Email}", request.Email);

            return Ok(new { message = "OTP sent successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP to {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to send OTP" });
        }
    }

    [HttpPost("verify-otp")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Otp))
            return BadRequest(new { error = "Email and OTP are required" });

        var cacheKey = $"{OtpCachePrefix}{request.Email.ToLowerInvariant()}";

        if (!_cache.TryGetValue(cacheKey, out string? storedOtp) || storedOtp != request.Otp)
        {
            _logger.LogWarning("Invalid or expired OTP for {Email}", request.Email);
            return Unauthorized(new { error = "Invalid or expired OTP" });
        }

        _cache.Remove(cacheKey);

        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null)
            {
                var userName = request.Email.Split('@')[0];

                user = BricklePlatform.Domain.Entities.User.Create(
                    firstName: userName,
                    lastName: "",
                    email: request.Email,
                    phoneNumber: "",
                    termsAccepted: true,
                    passwordHash: Array.Empty<byte>(),
                    passwordSalt: Array.Empty<byte>());

                user = await _userRepository.AddAsync(user);
                _logger.LogInformation("New user created via OTP: {Email}", request.Email);
            }

            var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Email);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var refreshCacheKey = $"refresh_{refreshToken}";
            _cache.Set(refreshCacheKey, user.Id.ToString(), TimeSpan.FromDays(30));

            var userDto = MapToDto(user);

            return Ok(new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = userDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OTP verification failed for {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest(new { error = "Refresh token is required" });

        var cacheKey = $"refresh_{request.RefreshToken}";

        if (!_cache.TryGetValue(cacheKey, out string? userIdStr) || string.IsNullOrEmpty(userIdStr))
            return Unauthorized(new { error = "Invalid or expired refresh token" });

        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized(new { error = "Invalid refresh token" });

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return Unauthorized(new { error = "User not found" });

        _cache.Remove(cacheKey);

        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Email);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        var newCacheKey = $"refresh_{newRefreshToken}";
        _cache.Set(newCacheKey, user.Id.ToString(), TimeSpan.FromDays(30));

        var userDto = MapToDto(user);

        return Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            User = userDto
        });
    }

    private static string GenerateOtp()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    private static UserDto MapToDto(BricklePlatform.Domain.Entities.User user)
    {
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
            IsBasicProfileComplete = HasCompleteBasicProfile(user),
            IsFullProfileComplete = user.IsFullProfileComplete,
            IsProfileUnderReview = user.IsProfileUnderReview
        };
    }

    private static bool HasCompleteBasicProfile(BricklePlatform.Domain.Entities.User user)
    {
        return !string.IsNullOrWhiteSpace(user.FirstName) &&
            !string.IsNullOrWhiteSpace(user.LastName) &&
            !string.IsNullOrWhiteSpace(user.PhoneNumber) &&
            user.DateOfBirth.HasValue &&
            !string.IsNullOrWhiteSpace(user.Nationality) &&
            !string.IsNullOrWhiteSpace(user.CountryOfResidence) &&
            user.DocumentType.HasValue &&
            !string.IsNullOrWhiteSpace(user.DocumentNumber);
    }
}

public record GoogleAuthRequest
{
    [Required]
    public string IdToken { get; init; } = string.Empty;

    /// <summary>
    /// Optional Web Client ID from Google Cloud Console for audience validation.
    /// If omitted, audience validation is skipped.
    /// </summary>
    public string? ClientId { get; init; }
}

public record SendOtpRequest
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;
}

public record VerifyOtpRequest
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Otp { get; init; } = string.Empty;
}

public record RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; init; } = string.Empty;
}

public record AuthResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public UserDto User { get; init; } = null!;
}
