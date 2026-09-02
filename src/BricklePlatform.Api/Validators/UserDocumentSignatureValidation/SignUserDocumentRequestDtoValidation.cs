using BricklePlatform.Api.Application.Dtos;
using BricklePlatform.Domain.Entities;
using FluentValidation;

namespace BricklePlatform.Api.Validators.UserDocumentSignatureValidation;

public class SignUserDocumentRequestDtoValidation : AbstractValidator<SignUserDocumentRequestDto>
{
    public SignUserDocumentRequestDtoValidation()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(ValidationMessages.REQUIREDFIELD);

        RuleFor(x => x.DocumentType)
            .NotEmpty().WithMessage(ValidationMessages.REQUIREDFIELD)
            .Must(UserSignatureDocumentType.IsValid)
            .WithMessage($"El tipo de documento debe ser uno de: {string.Join(", ", UserSignatureDocumentType.All)}");

        RuleFor(x => x.DocumentVersion)
            .NotEmpty().WithMessage(ValidationMessages.REQUIREDFIELD)
            .MaximumLength(50).WithMessage(ValidationMessages.MAXLENGTH);

        RuleFor(x => x.SignerName)
            .NotEmpty().WithMessage(ValidationMessages.REQUIREDFIELD)
            .MaximumLength(200).WithMessage(ValidationMessages.MAXLENGTH);

        RuleFor(x => x.SignaturePaths)
            .NotEmpty().WithMessage("La firma no puede estar vacía.");
    }
}
