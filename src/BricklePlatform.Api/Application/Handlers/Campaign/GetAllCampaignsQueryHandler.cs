using BricklePlatform.Api.Application.Queries.Campaign;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BricklePlatform.Api.Application.Handlers.Campaign;

public class GetAllCampaignsQueryHandler : IRequestHandler<GetAllCampaignsQuery, IEnumerable<CampaignDto>>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly ILogger<GetAllCampaignsQueryHandler> _logger;

    public GetAllCampaignsQueryHandler(
        ICampaignRepository campaignRepository,
        ILogger<GetAllCampaignsQueryHandler> logger)
    {
        _campaignRepository = campaignRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<CampaignDto>> Handle(GetAllCampaignsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all campaigns - CorrelationId: {CorrelationId}",
            request.Header.CorrelationId);

        try
        {
            var campaigns = await _campaignRepository.GetAllAsync();

            var campaignDtos = campaigns.Select(campaign => new CampaignDto
            {
                Id = campaign.Id,
                MinCapital = campaign.MinCapital,
                MaxCapital = campaign.MaxCapital,
                Status = campaign.Status,
                BaseToken = campaign.BaseToken,
                BrickleAddress = campaign.BrickleAddress,
                CampaignAddress = campaign.CampaignAddress,
                CampaignTx = campaign.CampaignTx,
                LeasingId = campaign.LeasingId,
            });

            _logger.LogInformation("Retrieved {CampaignCount} campaigns - CorrelationId: {CorrelationId}",
                campaignDtos.Count(), request.Header.CorrelationId);

            return campaignDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all campaigns - CorrelationId: {CorrelationId}",
                request.Header.CorrelationId);
            throw;
        }
    }
}