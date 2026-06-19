using BricklePlatform.Domain.DTOs;

namespace BricklePlatform.Domain.Interfaces;

public interface ILogRepository
{
    Task InsertPaymentLogAsync(PaymentLogDto paymentLogDto);
}