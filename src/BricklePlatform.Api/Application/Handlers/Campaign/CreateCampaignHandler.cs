using System.Numerics;
using BricklePlatform.Api.Application.Commands.Campaign;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers;

public class CreateCampaingHandler : IRequestHandler<CreateCampaignCommand, CampaignDto>
{
  private readonly ICampaignRepository _campaignRepository;
  private readonly IThresholdFactoryService _thresholdFactoryService;
  private readonly IUserLeasingAgreementRepository _userLeasingAgreementRepository;
  private readonly ILeasingRepository _leasingRepository;
  private readonly ILogger<CreateCampaingHandler> _logger;

  public CreateCampaingHandler(
      ICampaignRepository campaignRepository,
      IThresholdFactoryService thresholdFactoryService,
      IUserLeasingAgreementRepository userLeasingAgreementRepository,
      ILeasingRepository leasingRepository,
      ILogger<CreateCampaingHandler> logger)
  {
    _campaignRepository = campaignRepository;
    _thresholdFactoryService = thresholdFactoryService;
    _userLeasingAgreementRepository = userLeasingAgreementRepository;
    _leasingRepository = leasingRepository;
    _logger = logger;
  }

  public async Task<CampaignDto> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
  {
    var campaignDto = request.TokenizeAsset.Campaign;
    var leasingDto = request.TokenizeAsset.Leasing;

    _logger.LogInformation("Creating Campaign: {Status} - CorrelationId: {CorrelationId}",
        campaignDto.Status, request.Header.CorrelationId);

    Domain.Entities.Leasing? leasing = await _leasingRepository.GetByIdAsync(campaignDto.LeasingId);

    if (leasing == null)
    {
      _logger.LogWarning("Activo no encontrado: {LeasingId}",
               campaignDto.LeasingId);
      throw new ApplicationException($"Activo no encontrado con Id: {campaignDto.LeasingId}.");
    }

    var existingCampaign = await _campaignRepository.GetByLeasingIdAsync(campaignDto.LeasingId);
    if (existingCampaign != null)
    {
      _logger.LogWarning("Ya existe una campaña para el LeasingId: {LeasingId}",
               campaignDto.LeasingId);
      throw new ApplicationException($"Ya existe una campaña para el LeasingId: {campaignDto.LeasingId}.");
    }

    var userAgreement = await _userLeasingAgreementRepository.GetByLeasingIdAsync(campaignDto.LeasingId);

    var dateTimeOffset = new DateTimeOffset(leasing.ContractTime ?? DateTime.UtcNow);
    long unixTimestamp = dateTimeOffset.ToUnixTimeSeconds();

    var campaignInfo = new CampaignInfoDto
    {
      minCap = new BigInteger(campaignDto.MinCapital * (decimal)Math.Pow(10, 6)),
      maxCap = new BigInteger(campaignDto.MaxCapital * (decimal)Math.Pow(10, 6)),
      totalLeasingTokens = new BigInteger(leasing.Tokens),
      tokenPrice = new BigInteger(leasing.PricePerToken * (decimal)Math.Pow(10, 6)),
      deadline = new BigInteger(unixTimestamp),
      baseToken = campaignDto.BaseToken,
      brickleAddress = campaignDto.BrickleAddress
    };

    LeasingInfoDto leasingInfo;
    if (userAgreement != null)
    {
      decimal tokenPriceScaled = (leasingDto.LeasingTokenPrice ?? leasing.PricePerToken) * (decimal)Math.Pow(10, 6);
      decimal finalPaymentScaled = (leasingDto.FinalPaymentAmount ?? (leasing.Price - userAgreement.TotalValue)) * (decimal)Math.Pow(10, 6);
      leasingInfo = new LeasingInfoDto
      {
        assetValue = new BigInteger(userAgreement.AssetValue * (decimal)Math.Pow(10, 6)),
        usefulLife = new BigInteger((long)Math.Round(userAgreement.UsefulLife * 100)),
        termMonths = new BigInteger(userAgreement.TermTime),
        leasingTokenPrice = new BigInteger(tokenPriceScaled),
        monthlyRate = new BigInteger(userAgreement.InstallmentRate * (decimal)Math.Pow(10, 6)),
        monthlyPayment = new BigInteger(userAgreement.InstallmentAmount * (decimal)Math.Pow(10, 6)),
        managementFee = new BigInteger(userAgreement.ManagementFee * (decimal)Math.Pow(10, 6)),
        insurancePct = new BigInteger(userAgreement.InsurancePercentage * 100),
        ibrRate = new BigInteger(userAgreement.IbrRate * 100),
        riskLevel = new BigInteger((long)Math.Round(userAgreement.RiskLevel * 100)),
        riskRate = new BigInteger(userAgreement.RiskRate * 100),
        IVA = new BigInteger(userAgreement.IVA * (decimal)Math.Pow(10, 6)),
        reteIcaPct = new BigInteger(leasing.ReteIcaPct * 100),
        reteFuentePct = new BigInteger(leasing.ReteFuentePct * 100),
        finalPaymentAmount = new BigInteger(finalPaymentScaled),
        buyerRetentionPercentage = new BigInteger(userAgreement.BuyerRetentionPercentage * 100),
      };
    }
    else
    {
      decimal monthlyPaymentAmount = ComputeMonthlyPayment(leasing.Price, leasingDto.ResidualValue, leasingDto.InstallmentRate, leasingDto.TermTime);
      if (monthlyPaymentAmount <= 0)
        throw new ApplicationException("El pago mensual calculado es inválido (<= 0). Revisa precio del activo, valor residual, tasa y plazo.");
      decimal tokenPriceScaled = (leasingDto.LeasingTokenPrice ?? leasing.PricePerToken) * (decimal)Math.Pow(10, 6);
      decimal finalPaymentScaled = (leasingDto.FinalPaymentAmount ?? leasingDto.ResidualValue) * (decimal)Math.Pow(10, 6);
      leasingInfo = new LeasingInfoDto
      {
        assetValue = new BigInteger(leasingDto.AssetValue * (decimal)Math.Pow(10, 6)),
        usefulLife = new BigInteger((long)Math.Round(leasingDto.UsefulLife * 100)),
        termMonths = new BigInteger(leasingDto.TermTime),
        leasingTokenPrice = new BigInteger(tokenPriceScaled),
        monthlyRate = new BigInteger(leasingDto.InstallmentRate * (decimal)Math.Pow(10, 6)),
        monthlyPayment = new BigInteger(monthlyPaymentAmount * (decimal)Math.Pow(10, 6)),
        managementFee = new BigInteger(leasingDto.ManagementFee * (decimal)Math.Pow(10, 6)),
        insurancePct = new BigInteger(leasingDto.InsurancePercentage * 100),
        ibrRate = new BigInteger(leasingDto.IbrRate * 100),
        riskLevel = new BigInteger(leasingDto.RiskLevel * 100),
        riskRate = new BigInteger(leasingDto.RiskRate * 100),
        IVA = new BigInteger(leasingDto.IVA * (decimal)Math.Pow(10, 6)),
        reteIcaPct = new BigInteger(leasing.ReteIcaPct * 100),
        reteFuentePct = new BigInteger(leasing.ReteFuentePct * 100),
        finalPaymentAmount = new BigInteger(finalPaymentScaled),
        buyerRetentionPercentage = new BigInteger(leasingDto.BuyerRetentionPercentage * 100),
      };
    }

    var campaignTx = await _thresholdFactoryService.CreateCampaign(campaignInfo, leasingInfo);

    if (campaignTx.ContractAddress == null && campaignTx.TransactionHash == null)
    {
      throw new ApplicationException("Falló la creación de la campaña.");
    }

    var campaign = Domain.Entities.Campaign.Create(
        campaignDto.LeasingId,
        campaignDto.MinCapital,
        campaignDto.MaxCapital,
        campaignDto.Status,
        campaignDto.BaseToken,
        campaignDto.BrickleAddress,
        campaignTx.ContractAddress ?? string.Empty,
        campaignTx.TransactionHash ?? string.Empty
      );

    await _campaignRepository.AddAsync(campaign);

    if (userAgreement == null)
    {
      var agreement = Domain.Entities.UserLeasingAgreement.Create(
        leasingDto.UserId,
        leasingDto.LeasingId,
        leasingDto.AssetValue,
        leasingDto.UsefulLife,
        leasingDto.TermTime,
        leasingDto.AgreementType,
        leasingDto.PaymentTerm,
        leasingDto.Currency,
        leasingDto.ContractDetails,
        leasingDto.StartDate,
        leasingDto.EndDate,
        leasingDto.InstallmentRate,
        leasingDto.ResidualValue,
        leasingDto.ManagementFee,
        leasingDto.LeasingCoreAddress,
        leasingDto.InsurancePercentage,
        leasingDto.IbrRate,
        leasingDto.RiskLevel,
        leasingDto.RiskRate,
        leasingDto.IVA,
        leasing.ReteIcaPct,
        leasing.ReteFuentePct,
        leasingDto.BuyerRetentionPercentage,
        leasing
      );
      await _userLeasingAgreementRepository.AddAsync(agreement);
    }

    return new CampaignDto
    {
      Id = campaign.Id,
      MinCapital = campaign.MinCapital,
      MaxCapital = campaign.MaxCapital,
      Status = campaign.Status,
      BaseToken = campaign.BaseToken,
      BrickleAddress = campaign.BrickleAddress,
      CampaignAddress = campaign.CampaignAddress,
      CampaignTx = campaign.CampaignTx
    };
  }

  private static decimal ComputeMonthlyPayment(decimal price, decimal residualValue, decimal installmentRate, decimal termTime)
  {
    double installmentPercentage = (double)installmentRate / 100;
    double term = (double)termTime;
    double priceD = (double)price;
    double residualD = (double)residualValue;
    double monthlyPayment = ((priceD * installmentPercentage * Math.Pow(1 + installmentPercentage, term)) - (residualD * installmentPercentage))
        / (Math.Pow(1 + installmentPercentage, term) - 1);
    return (decimal)monthlyPayment;
  }
}