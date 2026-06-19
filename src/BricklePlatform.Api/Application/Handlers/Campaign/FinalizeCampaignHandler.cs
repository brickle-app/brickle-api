using BricklePlatform.Api.Application.Commands.Campaign;
using BricklePlatform.Api.Application.Commands.Notifications;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BricklePlatform.Api.Application.Handlers.Campaign;

public class FinalizeCampaignHandler : IRequestHandler<FinalizeCampaignCommand, FinalizeCampaignResponse>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly IInvestmentRepository _investmentRepository;
    private readonly ILeasingRepository _leasingRepository;
    private readonly IUserLeasingAgreementRepository _userLeasingAgreementRepository;
    private readonly IThresholdFactoryService _thresholdFactoryService;
    private readonly IEmailService _emailService;
    private readonly IMediator _mediator;
    private readonly ILogger<FinalizeCampaignHandler> _logger;

    public FinalizeCampaignHandler(
        ICampaignRepository campaignRepository,
        IInvestmentRepository investmentRepository,
        ILeasingRepository leasingRepository,
        IUserLeasingAgreementRepository userLeasingAgreementRepository,
        IThresholdFactoryService thresholdFactoryService,
        IEmailService emailService,
        IMediator mediator,
        ILogger<FinalizeCampaignHandler> logger)
    {
        _campaignRepository = campaignRepository;
        _investmentRepository = investmentRepository;
        _leasingRepository = leasingRepository;
        _userLeasingAgreementRepository = userLeasingAgreementRepository;
        _thresholdFactoryService = thresholdFactoryService;
        _emailService = emailService;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<FinalizeCampaignResponse> Handle(FinalizeCampaignCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Finalizing campaign: {CampaignId} by user: {UserId} - CorrelationId: {CorrelationId}",
           request.CampaignId, request.UserId, request.Header.CorrelationId);

        try
        {
            // 1. Get campaign
            var campaign = await _campaignRepository.GetByIdAsync(request.CampaignId);
            if (campaign == null)
            {
                _logger.LogWarning("Campaign not found: {CampaignId} - CorrelationId: {CorrelationId}",
                    request.CampaignId, request.Header.CorrelationId);
                throw new ApplicationException("Campaign not found");
            }

            // 2. Verify campaign state on blockchain before finalizing (0=Active, 1=Successful, 2=Failed)
            var blockchainState = await _thresholdFactoryService.GetCampaignStateAsync(campaign.CampaignAddress);
            const byte SuccessfulState = 1;
            if (blockchainState != SuccessfulState)
            {
                var stateDescription = blockchainState switch
                {
                    0 => "Active (la campaña aún no ha alcanzado el capital máximo)",
                    2 => "Failed (la campaña ha fallado)",
                    _ => $"Desconocido ({blockchainState})"
                };
                _logger.LogWarning("Campaign {CampaignId} state on blockchain is {State}: {Description}. Cannot finalize. - CorrelationId: {CorrelationId}",
                    request.CampaignId, blockchainState, stateDescription, request.Header.CorrelationId);
                throw new ApplicationException(
                    $"La campaña no puede finalizarse. Estado en blockchain: {stateDescription}. " +
                    "Solo se puede finalizar cuando la campaña está en estado Successful (capital máximo alcanzado).");
            }

            // 3. Call blockchain finalizeCampaign (brickleAssumeInsurance set by admin); get leasingCore and token for persistence
            var (leasingCoreAddress, tokenAddress, transactionHash) = await _thresholdFactoryService.FinalizeCampaign(campaign.CampaignAddress, request.BrickleAssumeInsurance);

            _logger.LogInformation("Campaign finalized on blockchain. TxHash: {TransactionHash}, LeasingCore: {LeasingCoreAddress}, Token: {TokenAddress} - CorrelationId: {CorrelationId}",
                transactionHash, leasingCoreAddress, tokenAddress, request.Header.CorrelationId);

            if (string.IsNullOrWhiteSpace(leasingCoreAddress))
            {
                _logger.LogError("LeasingCore address from contract is empty for Campaign: {CampaignId} - CorrelationId: {CorrelationId}",
                    request.CampaignId, request.Header.CorrelationId);
                throw new ApplicationException("La dirección del LeasingCore devuelta por el contrato está vacía. No se puede actualizar Leasing ni UserLeasingAgreement.");
            }

            // 4. Update campaign status from Active (0) to Successful (1)
            campaign.Update(
                campaign.MinCapital,
                campaign.MaxCapital,
                Domain.Enums.CampaignStatusEnum.Successful
            );
            await _campaignRepository.UpdateAsync(campaign);

            _logger.LogInformation("Campaign status updated to Successful for Campaign: {CampaignId} - CorrelationId: {CorrelationId}",
                request.CampaignId, request.Header.CorrelationId);

            // 5. Persist leasingCore: Leasing.contract_address (DB column contract_address)
            var leasing = await _leasingRepository.GetByIdAsync(campaign.LeasingId);
            if (leasing == null)
            {
                _logger.LogError("Leasing not found for LeasingId: {LeasingId} (Campaign: {CampaignId}). Cannot update contract_address or UserLeasingAgreement. - CorrelationId: {CorrelationId}",
                    campaign.LeasingId, request.CampaignId, request.Header.CorrelationId);
                throw new ApplicationException($"No se encontró el Leasing con Id {campaign.LeasingId} asociado a la campaña. No se pueden actualizar contract_address ni leasingCore. Verifique la integridad de datos.");
            }

            {
                leasing.UpdateContractAddress(leasingCoreAddress);
                await _leasingRepository.UpdateAsync(leasing);

                _logger.LogInformation("Leasing contract_address updated to {LeasingCoreAddress} for Leasing: {LeasingId} - CorrelationId: {CorrelationId}",
                    leasingCoreAddress, campaign.LeasingId, request.Header.CorrelationId);

                // 6. Persist leasingCore: UserLeasingAgreement.leasing_address (DB column leasing_address). ReteIcaPct/ReteFuentePct are not modified (already set at campaign creation).
                var userAgreements = await _userLeasingAgreementRepository.GetAllByLeasingIdAsync(campaign.LeasingId);
                foreach (var agreement in userAgreements)
                {
                    agreement.UpdateLeasingCoreAddress(leasingCoreAddress);
                    await _userLeasingAgreementRepository.UpdateAsync(agreement);
                }

                _logger.LogInformation("Updated {AgreementCount} UserLeasingAgreements leasing_address to {LeasingCoreAddress} - CorrelationId: {CorrelationId}",
                    userAgreements.Count(), leasingCoreAddress, request.Header.CorrelationId);

                // 7. Get all investors for this campaign
                var investors = await _investmentRepository.GetInvestorsByLeasingIdAsync(campaign.LeasingId);

                // 8. Send Expo notifications to all investors
                if (investors.Any())
                {
                    var notificationCommand = new SendBulkNotificationCommand(
                        BatchSize: 100,
                        ActionId: "LEASING_ACTIVE",
                        Title: "¡Leasing Activo!",
                        Body: $"El leasing de {leasing.Name} está activo. ¡Ya puedes reclamar renta mensual!",
                        Data: new { campaignId = request.CampaignId, leasingId = campaign.LeasingId }
                    );

                    await _mediator.Send(notificationCommand, cancellationToken);

                    _logger.LogInformation("Bulk notification sent to {InvestorCount} investors - CorrelationId: {CorrelationId}",
                        investors.Count(), request.Header.CorrelationId);
                }

                // 9. Send email notifications to all investors
                var emailTasks = investors.Select(async investor =>
                {
                    try
                    {
                        await _emailService.SendLeasingActiveNotificationAsync(
                            investor.Email,
                            $"{investor.FirstName} {investor.LastName}",
                            leasing.Name
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending email to investor {UserId} - CorrelationId: {CorrelationId}",
                            investor.Id, request.Header.CorrelationId);
                    }
                });

                await Task.WhenAll(emailTasks);

                _logger.LogInformation("Email notifications sent to {InvestorCount} investors - CorrelationId: {CorrelationId}",
                    investors.Count(), request.Header.CorrelationId);
            }

            return new FinalizeCampaignResponse(true, leasingCoreAddress, tokenAddress ?? string.Empty, transactionHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finalizing campaign: {CampaignId} - CorrelationId: {CorrelationId}",
                request.CampaignId, request.Header.CorrelationId);
            throw;
        }
    }
}