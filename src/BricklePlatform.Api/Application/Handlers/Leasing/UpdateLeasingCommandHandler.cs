using BricklePlatform.Api.Application.Commands.Leasing;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Exceptions;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.Leasing;

public class UpdateLeasingCommandHandler : IRequestHandler<UpdateLeasingCommand, LeasingDto>
{
    private readonly ILeasingRepository _leasingRepository;

    public UpdateLeasingCommandHandler(ILeasingRepository leasingRepository)
    {
        _leasingRepository = leasingRepository;
    }

    public async Task<LeasingDto> Handle(UpdateLeasingCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Leasing? leasing = await _leasingRepository.GetByIdAsync(request.Id);
        if (leasing == null)
            throw new NotFoundException($"Leasing con id {request.Id} no encontrado");

        leasing.Update(
            request.LeasingDto.Name,
            request.LeasingDto.Quantity,
            request.LeasingDto.Price,
            request.LeasingDto.Tokens,
            request.LeasingDto.TokensAvailable,
            request.LeasingDto.PricePerToken,
            request.LeasingDto.Type,
            request.LeasingDto.Liquidity,
            request.LeasingDto.TIR,
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

        await _leasingRepository.UpdateAsync(leasing);

        return new LeasingDto
        {
            Id = leasing.Id,
            Name = leasing.Name,
            Quantity = leasing.Quantity,
            Price = leasing.Price,
            Tokens = leasing.Tokens,
            TokensAvailable = leasing.TokensAvailable,
            PricePerToken = leasing.PricePerToken,
            TIR = leasing.TIR,
            ReteIcaPct = leasing.ReteIcaPct,
            ReteFuentePct = leasing.ReteFuentePct,
            Description = leasing.Description,
            Type = leasing.Type.ToString(),
            ContractTime = leasing.ContractTime,
            Liquidity = leasing.Liquidity.ToString(),
            CoverImageUrl = leasing.CoverImageUrl,
            MiniatureImageUrl = leasing.MiniatureImageUrl,
            DiscoverImageUrl = leasing.DiscoverImageUrl,
            ContractAddress = leasing.ContractAddress,
            Active = leasing.Active,
            Details = leasing.Details?.Select(d => new AssetDetailDto
            {
                Title = d.Title,
                Value = d.Value
            }).ToList(),
            CreatedAt = leasing.CreatedAt,
            UpdatedAt = leasing.UpdatedAt
        };
    }
}