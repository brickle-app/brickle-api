using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Enums;
using MediatR;

namespace BricklePlatform.Api.Application.Queries.Leasing;

public record GetLeasingsByGroupCategoryQuery : IRequest<IEnumerable<LeasingDto>>
{
    public LeasingGroupCategoryEnum GroupCategory { get; }
    public bool? Active { get; }

    public GetLeasingsByGroupCategoryQuery(LeasingGroupCategoryEnum groupCategory, bool? active = null)
    {
        GroupCategory = groupCategory;
        Active = active;
    }
}