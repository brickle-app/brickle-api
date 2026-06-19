using BricklePlatform.Api.Application.Commands.UserLeasingAgreement;
using BricklePlatform.Api.Application.Dtos;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.UserLeasingAgreement;

public class CreateUserLeasingAgreementCommandHandler : IRequestHandler<CreateUserLeasingAgreementCommand, UserLeasingAgreementDto>
{
    private readonly IUserLeasingAgreementRepository _agreementRepository;
    private readonly ILeasingRepository _leasingRepository;
    private readonly IUserRepository _userRepository;

    public CreateUserLeasingAgreementCommandHandler(
        IUserLeasingAgreementRepository agreementRepository,
        ILeasingRepository leasingRepository,
        IUserRepository userRepository)
    {
        _agreementRepository = agreementRepository;
        _leasingRepository = leasingRepository;
        _userRepository = userRepository;
    }

    public async Task<UserLeasingAgreementDto> Handle(CreateUserLeasingAgreementCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.User? user = await _userRepository.GetByIdAsync(request.Agreement.UserId);
        if (user == null)
            throw new ApplicationException($"Usuario con id {request.Agreement.UserId} no encontrado");

        Domain.Entities.Leasing? leasing = await _leasingRepository.GetByIdAsync(request.Agreement.LeasingId);
        if (leasing == null)
            throw new ApplicationException($"Leasing con id {request.Agreement.LeasingId} no encontrado");

        Domain.Entities.UserLeasingAgreement agreement = Domain.Entities.UserLeasingAgreement.Create(
            request.Agreement.UserId,
            request.Agreement.LeasingId,
            request.Agreement.AssetValue,
            request.Agreement.UsefulLife,
            request.Agreement.TermTime,
            request.Agreement.AgreementType,
            request.Agreement.PaymentTerm,
            request.Agreement.Currency,
            request.Agreement.ContractDetails,
            request.Agreement.StartDate,
            request.Agreement.EndDate,
            request.Agreement.InstallmentRate,
            request.Agreement.ResidualValue,
            request.Agreement.ManagementFee,
            request.Agreement.LeasingCoreAddress,
            request.Agreement.InsurancePercentage,
            request.Agreement.IbrRate,
            request.Agreement.RiskLevel,
            request.Agreement.RiskRate,
            request.Agreement.IVA,
            leasing.ReteIcaPct,
            leasing.ReteFuentePct,
            request.Agreement.BuyerRetentionPercentage,
            leasing
        );

        agreement = await _agreementRepository.AddAsync(agreement);

        return new UserLeasingAgreementDto
        {
            Id = agreement.Id,
            UserId = agreement.UserId,
            LeasingId = agreement.LeasingId,
            AssetValue = agreement.AssetValue,
            UsefulLife = agreement.UsefulLife,
            TermTime = agreement.TermTime,
            PaymentTerm = agreement.PaymentTerm,
            AgreementType = agreement.AgreementType,
            Currency = agreement.Currency,
            ContractDetails = agreement.ContractDetails,
            StartDate = agreement.StartDate,
            EndDate = agreement.EndDate,
            InstallmentRate = agreement.InstallmentRate,
            InstallmentAmount = agreement.InstallmentAmount,
            ManagementFee = agreement.ManagementFee,
            TotalValue = agreement.TotalValue,
            RemainingBalance = agreement.RemainingBalance,
            Status = agreement.Status,
            LeasingCoreAddress = agreement.LeasingCoreAddress,
            InsurancePercentage = agreement.InsurancePercentage,
            IbrRate = agreement.IbrRate,
            RiskLevel = agreement.RiskLevel,
            RiskRate = agreement.RiskRate,
            IVA = agreement.IVA,
            ReteIcaPct = agreement.ReteIcaPct,
            ReteFuentePct = agreement.ReteFuentePct,
            BuyerRetentionPercentage = agreement.BuyerRetentionPercentage,
            CreatedAt = agreement.CreatedAt,
            UpdatedAt = agreement.UpdatedAt
        };
    }
}