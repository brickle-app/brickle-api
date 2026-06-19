using BricklePlatform.Api.Application.Commands.Investment;
using BricklePlatform.Api.Application.Dtos;
using BricklePlatform.Domain.Entities;
using BricklePlatform.Domain.Interfaces;
using MediatR;

namespace BricklePlatform.Api.Application.Handlers.Investment
{
    public class CreateInvestmentCommandHandler : IRequestHandler<CreateInvestmentCommand, CreateInvestmentDto>
    {
        private readonly IInvestmentRepository _investmentRepository;
        private readonly ILeasingRepository _leasingRepository;
        private readonly ILogger<CreateInvestmentCommandHandler> _logger;

        public CreateInvestmentCommandHandler(
            IInvestmentRepository investmentRepository,
            ILeasingRepository leasingRepository,
            ILogger<CreateInvestmentCommandHandler> logger)
        {
            _investmentRepository = investmentRepository;
            _leasingRepository = leasingRepository;
            _logger = logger;
        }

        public async Task<CreateInvestmentDto> Handle(CreateInvestmentCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating investment for User: {UserId}, Leasing: {LeasingId}, Amount: {Amount}, BricksCount: {BricksCount}", 
                request.UserId, request.LeasingId, request.Amount, request.BricksCount);

            // Get leasing to validate tokens availability and update tokensAvailable
            var leasing = await _leasingRepository.GetByIdAsync(request.LeasingId);
            if (leasing == null)
            {
                _logger.LogWarning("Leasing not found for ID: {LeasingId}", request.LeasingId);
                throw new ApplicationException($"Leasing not found for ID: {request.LeasingId}");
            }

            // Validate that the requested BricksCount doesn't exceed available tokens
            if ((decimal)leasing.TokensAvailable < request.BricksCount)
            {
                _logger.LogWarning("Insufficient tokens available for Leasing: {LeasingId}. Available: {Available}, Requested: {Requested}",
                    request.LeasingId, leasing.TokensAvailable, request.BricksCount);
                throw new ApplicationException($"Insufficient tokens available. Available: {leasing.TokensAvailable}, Requested: {request.BricksCount}");
            }

            // Check if user already has an investment in this asset
            var existingInvestment = await _investmentRepository.GetByUserIdAndLeasingIdAsync(request.UserId, request.LeasingId);

            Domain.Entities.Investment investment;

            if (existingInvestment != null)
            {
                // Update existing investment by adding new bricks and amount
                existingInvestment.AddToInvestment(request.Amount, request.BricksCount);
                investment = await _investmentRepository.UpdateAsync(existingInvestment);
                
                _logger.LogInformation("Existing investment updated for User: {UserId}, Leasing: {LeasingId}. Added Amount: {Amount}, Added Bricks: {BricksCount}. Total Amount: {TotalAmount}, Total Bricks: {TotalBricks}",
                    request.UserId, request.LeasingId, request.Amount, request.BricksCount, investment.Amount, investment.BricksCount);
            }
            else
            {
                // Create new investment
                investment = Domain.Entities.Investment.Create(
                    request.UserId,
                    request.LeasingId,
                    request.Amount,
                    request.BricksCount,
                    request.BricksName
                );

                investment = await _investmentRepository.CreateAsync(investment);
                
                _logger.LogInformation("New investment created for User: {UserId}, Leasing: {LeasingId}, Amount: {Amount}, BricksCount: {BricksCount}",
                    request.UserId, request.LeasingId, request.Amount, request.BricksCount);
            }

            // Update tokensAvailable in Leasing table
            int newTokensAvailable = leasing.TokensAvailable - (int)request.BricksCount;
            leasing.UpdateTokensAvailable(newTokensAvailable);
            await _leasingRepository.UpdateAsync(leasing);

            _logger.LogInformation("TokensAvailable updated for Leasing: {LeasingId}. Tokens used: {TokensUsed}, Remaining: {RemainingTokens}",
                request.LeasingId, request.BricksCount, newTokensAvailable);

            return new CreateInvestmentDto
            {
                UserId = investment.UserId,
                LeasingId = investment.LeasingId,
                Amount = investment.Amount,
                BricksCount = investment.BricksCount,
                BricksName = investment.BricksName
            };
        }
    }
}