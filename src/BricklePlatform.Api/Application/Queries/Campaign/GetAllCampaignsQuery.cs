using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Queries.Campaign;

public record GetAllCampaignsQuery(
    HeaderRequestModel Header
) : IRequest<IEnumerable<CampaignDto>>;