using BricklePlatform.Api.Application.Commands.Leasing;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.Leasing;

public class CreateLeasingCommandHandler : IRequestHandler<CreateLeasingCommand, LeasingDto>
{
    private readonly ILeasingRepository _leasingRepository;

    public CreateLeasingCommandHandler(ILeasingRepository leasingRepository)
    {
        _leasingRepository = leasingRepository;
    }

    public async Task<LeasingDto> Handle(CreateLeasingCommand request, CancellationToken cancellationToken)
    {
        int tokensAvailable = request.LeasingDto.TokensAvailable > 0
            ? request.LeasingDto.TokensAvailable
            : request.LeasingDto.Tokens;

        Domain.Entities.Leasing leasing = Domain.Entities.Leasing.Create(
            request.LeasingDto.Name,
            request.LeasingDto.Quantity,
            request.LeasingDto.Price,
            request.LeasingDto.Tokens,
            tokensAvailable,
            request.LeasingDto.PricePerToken,
            request.LeasingDto.TIR,
            request.LeasingDto.Type,
            request.LeasingDto.Liquidity,
            request.LeasingDto.Active,
            request.LeasingDto.Description,
            request.LeasingDto.ContractTime,
            request.LeasingDto.CoverImageUrl,
            request.LeasingDto.MiniatureImageUrl,
            request.LeasingDto.DiscoverImageUrl,
            request.LeasingDto.ContractAddress,
            request.LeasingDto.Details?.Select(d => new Domain.Entities.AssetDetail
            {
                Title = d.Title,
                Value = d.Value
            }).ToList(),
            request.LeasingDto.ReteIcaPct,
            request.LeasingDto.ReteFuentePct
        );

        Domain.Entities.Leasing createdLeasing = await _leasingRepository.CreateAsync(leasing);

        return new LeasingDto
        {
            Id = createdLeasing.Id,
            Name = createdLeasing.Name,
            Quantity = createdLeasing.Quantity,
            Price = createdLeasing.Price,
            Tokens = createdLeasing.Tokens,
            TokensAvailable = createdLeasing.TokensAvailable,
            PricePerToken = createdLeasing.PricePerToken,
            TIR = createdLeasing.TIR,
            ReteIcaPct = createdLeasing.ReteIcaPct,
            ReteFuentePct = createdLeasing.ReteFuentePct,
            Description = createdLeasing.Description,
            Type = createdLeasing.Type.ToString(),
            ContractTime = createdLeasing.ContractTime,
            Liquidity = createdLeasing.Liquidity.ToString(),
            CoverImageUrl = createdLeasing.CoverImageUrl,
            MiniatureImageUrl = createdLeasing.MiniatureImageUrl,
            DiscoverImageUrl = createdLeasing.DiscoverImageUrl,
            ContractAddress = createdLeasing.ContractAddress,
            Active = createdLeasing.Active,
            Details = createdLeasing.Details?.Select(d => new AssetDetailDto
            {
                Title = d.Title,
                Value = d.Value
            }).ToList(),
            CreatedAt = createdLeasing.CreatedAt,
            UpdatedAt = createdLeasing.UpdatedAt
        };
    }
}