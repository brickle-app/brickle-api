using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Queries.Property;

public record GetCampaignQuery(
    HeaderRequestModel Header,
    Guid Id
) : IRequest<CampaignDto>;