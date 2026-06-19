using BricklePlatform.Api.Models;
using MediatR;

namespace BricklePlatform.Api.Application.Commands.Campaign;

public record FinalizeCampaignCommand(
    HeaderRequestModel Header,
    Guid UserId,
    Guid CampaignId,
    bool BrickleAssumeInsurance = false
) : IRequest<FinalizeCampaignResponse>;