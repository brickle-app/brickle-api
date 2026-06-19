using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Queries.Leasing;

public class GetLeasingByIdQuery : IRequest<LeasingDto>
{
    public Guid Id { get; set; }

    public GetLeasingByIdQuery(Guid id)
    {
        Id = id;
    }
}