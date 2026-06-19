using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Commands.Campaign;

public record CreateCampaignCommand(
    HeaderRequestModel Header,
    CreateTokenizeAsset TokenizeAsset
) : IRequest<CampaignDto>;
