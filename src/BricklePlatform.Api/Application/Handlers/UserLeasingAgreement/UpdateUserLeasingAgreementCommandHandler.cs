using BricklePlatform.Api.Application.Commands.UserLeasingAgreement;
using BricklePlatform.Api.Application.Dtos;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.UserLeasingAgreement;

public class UpdateUserLeasingAgreementCommandHandler : IRequestHandler<UpdateUserLeasingAgreementCommand, UserLeasingAgreementDto>
{
    private readonly IUserLeasingAgreementRepository _agreementRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILeasingRepository _leasingRepository;

    public UpdateUserLeasingAgreementCommandHandler(
        IUserLeasingAgreementRepository agreementRepository,
        IUserRepository userRepository,
        ILeasingRepository leasingRepository)
    {
        _agreementRepository = agreementRepository;
        _userRepository = userRepository;
        _leasingRepository = leasingRepository;
    }

    public async Task<UserLeasingAgreementDto> Handle(UpdateUserLeasingAgreementCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.UserLeasingAgreement? agreement = await _agreementRepository.GetByIdAsync(request.Id);

        if (agreement == null)
            throw new ApplicationException($"Acuerdo de leasing con id {request.Id} no encontrado");

        Domain.Entities.User? user = await _userRepository.GetByIdAsync(agreement.UserId);
        if (user == null)
            throw new ApplicationException($"Usuario con id {agreement.UserId} no encontrado");

        Domain.Entities.Leasing? leasing = await _leasingRepository.GetByIdAsync(agreement.LeasingId);
        if (leasing == null)
            throw new ApplicationException($"Leasing con id {agreement.LeasingId} no encontrado");

        agreement.Update(
            request.AgreementDto.RemainingBalance,
            request.AgreementDto.EndDate,
            request.AgreementDto.Status
        );

        await _agreementRepository.UpdateAsync(agreement);

        return new UserLeasingAgreementDto
        {
            Id = agreement.Id,
            UserId = agreement.UserId,
            LeasingId = agreement.LeasingId,
            PaymentTerm = agreement.PaymentTerm,
            Currency = agreement.Currency,
            ContractDetails = agreement.ContractDetails,
            StartDate = agreement.StartDate,
            EndDate = agreement.EndDate,
            InstallmentAmount = agreement.InstallmentAmount,
            TotalValue = agreement.TotalValue,
            RemainingBalance = agreement.RemainingBalance,
            LeasingCoreAddress = agreement.LeasingCoreAddress,
            Status = agreement.Status,
            ReteIcaPct = agreement.ReteIcaPct,
            ReteFuentePct = agreement.ReteFuentePct,
            BuyerRetentionPercentage = agreement.BuyerRetentionPercentage,
            CreatedAt = agreement.CreatedAt,
            UpdatedAt = agreement.UpdatedAt
        };
    }
}