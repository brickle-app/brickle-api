using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Queries.Leasing;

public class GetLeasingAmortizationQuery : IRequest<AmortizationTableDto>
{
    public Guid LeasingId { get; set; }

    public GetLeasingAmortizationQuery(Guid leasingId)
    {
        LeasingId = leasingId;
    }
}
