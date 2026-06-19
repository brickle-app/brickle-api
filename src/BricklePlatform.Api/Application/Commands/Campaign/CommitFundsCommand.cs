using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Commands.Campaign;

public record CommitFundsCommand(
    HeaderRequestModel Header,
    Guid LeasingId,
    BuyAssetDto BuyAssetDto
) : IRequest<bool>;
