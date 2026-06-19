using BricklePlatform.Api.Application.Queries.Property;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BricklePlatform.Api.Application.Handlers.Campaign;

public class GetCampaignQueryHandler : IRequestHandler<GetCampaignQuery, CampaignDto>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly ILogger<GetCampaignQueryHandler> _logger;

    public GetCampaignQueryHandler(
        ICampaignRepository campaignRepository,
        ILogger<GetCampaignQueryHandler> logger)
    {
        _campaignRepository = campaignRepository;
        _logger = logger;
    }

    public async Task<CampaignDto> Handle(GetCampaignQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting campaign by ID: {CampaignId} - CorrelationId: {CorrelationId}",
            request.Id, request.Header.CorrelationId);

        try
        {
            var campaign = await _campaignRepository.GetByIdAsync(request.Id);

            if (campaign == null)
            {
                _logger.LogWarning("Campaign not found: {CampaignId} - CorrelationId: {CorrelationId}",
                    request.Id, request.Header.CorrelationId);
                throw new ApplicationException("Campaign not found");
            }

            var campaignDto = new CampaignDto
            {
                Id = campaign.Id,
                MinCapital = campaign.MinCapital,
                MaxCapital = campaign.MaxCapital,
                Status = campaign.Status,
                BaseToken = campaign.BaseToken,
                BrickleAddress = campaign.BrickleAddress,
                CampaignAddress = campaign.CampaignAddress,
                CampaignTx = campaign.CampaignTx,
                LeasingId = campaign.LeasingId
            };

            _logger.LogInformation("Campaign retrieved successfully: {CampaignId} - CorrelationId: {CorrelationId}",
                request.Id, request.Header.CorrelationId);

            return campaignDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving campaign: {CampaignId} - CorrelationId: {CorrelationId}",
                request.Id, request.Header.CorrelationId);
            throw;
        }
    }
}