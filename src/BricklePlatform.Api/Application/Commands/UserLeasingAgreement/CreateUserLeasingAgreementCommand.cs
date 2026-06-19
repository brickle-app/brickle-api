using BricklePlatform.Domain.Interfaces;
using MediatR;
using BricklePlatform.Domain.Exceptions;
using BricklePlatform.Api.Application.Dtos;

namespace BricklePlatform.Api.Application.Commands.UserLeasingAgreement
{
    public class CreateUserLeasingAgreementCommand : IRequest<UserLeasingAgreementDto>
    {
        public CreateUserLeasingAgreementDto Agreement { get; }

        public CreateUserLeasingAgreementCommand(CreateUserLeasingAgreementDto agreement)
        {
            Agreement = agreement;
        }
    }

    public class CreateUserLeasingAgreementCommandHandler : IRequestHandler<CreateUserLeasingAgreementCommand, UserLeasingAgreementDto>
    {
        private readonly IUserLeasingAgreementRepository _agreementRepository;
        private readonly ILeasingRepository _leasingRepository;

        public CreateUserLeasingAgreementCommandHandler(
            IUserLeasingAgreementRepository agreementRepository,
            ILeasingRepository leasingRepository)
        {
            _agreementRepository = agreementRepository;
            _leasingRepository = leasingRepository;
        }

        public async Task<UserLeasingAgreementDto> Handle(CreateUserLeasingAgreementCommand request, CancellationToken cancellationToken)
        {
            Domain.Entities.Leasing? leasing = await _leasingRepository.GetByIdAsync(request.Agreement.LeasingId);
            if (leasing == null)
                throw new NotFoundException($"Leasing con id {request.Agreement.LeasingId} no encontrado");

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
}