using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Entities;
using BricklePlatform.Domain.Exceptions;
using BricklePlatform.Domain.Interfaces;

namespace BricklePlatform.Infrastructure.Services;

public class LeasingService : ILeasingService
{
    private readonly ILeasingRepository _leasingRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFileService _fileService;

    public LeasingService(
        ILeasingRepository leasingRepository,
        IUserRepository userRepository,
        IFileService fileService)
    {
        _leasingRepository = leasingRepository;
        _userRepository = userRepository;
        _fileService = fileService;
    }

    public async Task<Leasing> CreateLeasingAsync(CreateLeasingDto leasingDto, User createdBy)
    {
        Leasing leasing = Leasing.Create(
            leasingDto.Name,
            leasingDto.Quantity,
            leasingDto.Price,
            leasingDto.Tokens,
            leasingDto.TokensAvailable,
            leasingDto.PricePerToken,
            leasingDto.TIR,
            leasingDto.Type,
            leasingDto.Liquidity,
            leasingDto.Active,
            leasingDto.Description,
            leasingDto.ContractTime,
            leasingDto.CoverImageUrl,
            leasingDto.MiniatureImageUrl,
            leasingDto.ContractAddress
        );

        return await _leasingRepository.CreateAsync(leasing);
    }

    public async Task<Leasing?> GetLeasingAsync(Guid id)
    {
        Leasing? leasing = await _leasingRepository.GetByIdAsync(id);
        if (leasing == null)
            throw new NotFoundException($"Leasing con id {id} no encontrado");

        return leasing;
    }

    public async Task<IEnumerable<Leasing>> GetAllLeasingsAsync()
    {
        return await _leasingRepository.GetAllAsync();
    }

    public async Task<Leasing> UpdateLeasingAsync(Guid id, UpdateLeasingDto leasingDto, User updatedBy)
    {
        Leasing? leasing = await GetLeasingAsync(id);
        if (leasing == null)
            throw new NotFoundException($"Leasing con id {id} no encontrado");

        leasing.Update(
            leasingDto.Name,
            leasingDto.Quantity,
            leasingDto.Price,
            leasingDto.Tokens,
            leasingDto.TokensAvailable,
            leasingDto.PricePerToken,
            leasingDto.Type,
            leasingDto.Liquidity,
            leasingDto.TIR,
            leasingDto.Description,
            leasingDto.ContractTime,
            leasingDto.CoverImageUrl,
            leasingDto.MiniatureImageUrl,
            leasingDto.ContractAddress
        );

        await _leasingRepository.UpdateAsync(leasing);
        return leasing;
    }

    public async Task DeleteLeasingAsync(Guid id)
    {
        Leasing? leasing = await GetLeasingAsync(id);
        if (leasing == null)
            throw new NotFoundException($"Leasing with id {id} not found");

        await _leasingRepository.DeleteAsync(id);
    }
}