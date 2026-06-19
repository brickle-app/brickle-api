using System.Numerics;
using BricklePlatform.Api.Application.Commands.Investment;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Services;
using MediatR;

namespace BricklePlatform.Api.Application.Commands.Campaign;

public class CommitFundsHandler : IRequestHandler<CommitFundsCommand, bool>
{
  private readonly IWebHookService _webHookService;
  private readonly ICampaignRepository _campaignRepository;
  private readonly ILeasingRepository _leasingRepository;
  private readonly ILogger<CommitFundsHandler> _logger;
  private readonly IMediator _mediator;
  private readonly IUserRepository _userRepository;
  private readonly IUserActivityLogService _userActivityLogService;

  public CommitFundsHandler(
    IWebHookService webHookService,
    ICampaignRepository campaignRepository,
    ILeasingRepository leasingRepository,
    ILogger<CommitFundsHandler> logger,
    IMediator mediator,
    IUserRepository userRepository,
    IUserActivityLogService userActivityLogService)
  {
    _webHookService = webHookService;
    _campaignRepository = campaignRepository;
    _leasingRepository = leasingRepository;
    _logger = logger;
    _mediator = mediator;
    _userRepository = userRepository;
    _userActivityLogService = userActivityLogService;
  }

  public async Task<bool> Handle(CommitFundsCommand request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Commiting Funds for Leasing: {LeasingId} - CorrelationId: {CorrelationId}",
       request.LeasingId, request.Header.CorrelationId);

    const string FEE = "100000";
    short[] vValues = { 27, 28 };
    if (!vValues.Contains(request.BuyAssetDto.PermitSignature.V) ||
       request.BuyAssetDto.PermitSignature.R == null ||
       request.BuyAssetDto.PermitSignature.S == null)
    {
      _logger.LogWarning("Permit signature is incomplete for Leasing: {LeasingId} - CorrelationId: {CorrelationId}",
          request.LeasingId, request.Header.CorrelationId);
      throw new ApplicationException("Permit signature is incomplete");
    }

    var campaign = await _campaignRepository.GetByLeasingIdAsync(request.LeasingId);

    if (campaign == null)
    {
      _logger.LogWarning("Campaign not found for Leasing: {LeasingId} - CorrelationId: {CorrelationId}",
          request.LeasingId, request.Header.CorrelationId);
      throw new ApplicationException("Campaign not found");
    }

    var commitFundsDto = new CommitFundsDto
    {
      Token = request.BuyAssetDto.Token,
      Sender = request.BuyAssetDto.Sender,
      Campaign = campaign.CampaignAddress,
      Amount = new BigInteger(request.BuyAssetDto.Amount * (decimal)Math.Pow(10, 6)).ToString(),
      Fee = FEE,
    };

    WebhookResponseDto webhookResponse = await _webHookService.ProcessCommitFunds(commitFundsDto, request.BuyAssetDto.Deadline, request.BuyAssetDto.PermitSignature);

    if (webhookResponse.Status)
    {
      // Get user by wallet address
      var user = await _userRepository.GetByWalletAddressAsync(request.BuyAssetDto.Sender);
      if (user != null)
      {
        // Get leasing info to calculate actual tokens purchased and update tokensAvailable
        var leasing = await _leasingRepository.GetByIdAsync(request.LeasingId);
        if (leasing != null)
        {
          var createInvestmentCommand = new CreateInvestmentCommand
          {
            UserId = user.Id,
            LeasingId = request.LeasingId,
            Amount = request.BuyAssetDto.Amount,
            BricksCount = request.BuyAssetDto.TotalTokens,
            BricksName = campaign.Leasing?.Name ?? "Unknown Asset"
          };

          await _mediator.Send(createInvestmentCommand, cancellationToken);

          _logger.LogInformation("Investment created successfully for User: {UserId}, Amount: {Amount}, Tokens: {Tokens} - CorrelationId: {CorrelationId}",
              user.Id, request.BuyAssetDto.Amount, request.BuyAssetDto.TotalTokens, request.Header.CorrelationId);

          // Log the INVESTMENT activity
          var investmentActivityLog = new UserActivityLogDto
          {
            UserId = user.Id,
            Type = "INVESTMENT",
            TxAmount = request.BuyAssetDto.Amount,
            Status = "SUCCESS",
            Receipt = string.Empty, // No receipt for blockchain transactions
            Hash = webhookResponse.Hash ?? string.Empty,
            Reference = $"Compra de {campaign.Leasing?.Name ?? "Unknown Asset"} - {request.BuyAssetDto.TotalTokens} tokens",
            LeasingId = request.LeasingId,
            Timestamp = DateTime.UtcNow
          };

          await _userActivityLogService.LogUserActivityAsync(investmentActivityLog);

          _logger.LogInformation("Investment activity logged for User: {UserId}, Hash: {Hash} - CorrelationId: {CorrelationId}",
              user.Id, webhookResponse.Hash, request.Header.CorrelationId);
        }
        else
        {
          _logger.LogWarning("Leasing not found for ID: {LeasingId} - CorrelationId: {CorrelationId}",
              request.LeasingId, request.Header.CorrelationId);
          throw new ApplicationException($"Leasing not found for ID: {request.LeasingId}");
        }
      }
      else
      {
        _logger.LogWarning("User not found for wallet address: {WalletAddress} - CorrelationId: {CorrelationId}",
            request.BuyAssetDto.Sender, request.Header.CorrelationId);
      }

      return true;
    }

    return false;
  }

}