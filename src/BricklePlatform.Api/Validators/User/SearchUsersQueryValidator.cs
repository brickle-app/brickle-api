using BricklePlatform.Api.Application.Queries.User;
using FluentValidation;

namespace BricklePlatform.Api.Validators.User;

public class SearchUsersQueryValidator : AbstractValidator<SearchUsersQuery>
{
    private const int MinSearchLength = 1;

    public SearchUsersQueryValidator()
    {
        RuleFor(x => x.SearchTerm)
            .NotEmpty()
            .WithMessage(ValidationMessages.CONTACT_SEARCH_CRITERIA_REQUIRED)
            .Must(IsValidSearchTerm)
            .WithMessage(ValidationMessages.CONTACT_SEARCH_INVALID_TERM);
    }

    /// <summary>
    /// Permite búsquedas con email/teléfono completo o prefijos para autocompletado.
    /// Acepta: email válido, teléfono válido, o prefijos de al menos MinSearchLength caracteres.
    /// </summary>
    private bool IsValidSearchTerm(string? searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm)) return false;

        if (IsValidEmail(searchTerm)) return true;

        if (IsValidPhoneNumber(searchTerm)) return true;

        // Permitir prefijos para búsqueda en tiempo real (ej: "orbitatech328@" o "orbitatech328@g")
        if (searchTerm.Length >= MinSearchLength)
            return true;

        return false;
    }

    private bool IsValidEmail(string email)
    {
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