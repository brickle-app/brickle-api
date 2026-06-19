using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Entities;
using BricklePlatform.Infrastructure.Services.Base;
using BricklePlatform.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace BricklePlatform.Infrastructure.Repositories;

public class LogRepository : AzureTableStorageBase<PaymentLogEntity>, ILogRepository
{
    private readonly ILogger<LogRepository> _logger;
    private readonly string tableName;

    public LogRepository
        (
            IOptions<InfrastructureSettings> settings,
            ILogger<LogRepository> logger
        )
        : base(settings.Value.AzureSettings.ConnectionString, settings.Value.AzureSettings.LogsTableName)
    {
        _logger = logger;
        tableName = settings.Value.AzureSettings.LogsTableName;
    }

    public async Task InsertPaymentLogAsync(PaymentLogDto paymentLogDto)
    {
        try
        {
            DateTime currentDate = DateTime.UtcNow;
            string currentDateString = currentDate.ToString("ddMMyyyy");
            string partitionKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(currentDateString));

            PaymentLogEntity paymentLogEntity = new()
            {
                PartitionKey = partitionKey,
                RowKey = Guid.NewGuid().ToString(),
                UserLeasingAgreementId = paymentLogDto.UserLeasingAgreementId.ToString(),
                Hash = paymentLogDto.Hash,
                PaymentAmount = paymentLogDto.PaymentAmount,
                TotalValue = paymentLogDto.TotalValue,
                RemainingBalance = paymentLogDto.RemainingBalance,
                LeasingContractAddress = paymentLogDto.LeasingContractAddress,
                UserWallet = paymentLogDto.UserWallet,
                Status = paymentLogDto.Status,
                Timestamp = currentDate
            };

            await InsertAsync(paymentLogEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error insertando registro de pagos en la tabla {LogsTableName}", tableName);
            throw;
        }
    }
}