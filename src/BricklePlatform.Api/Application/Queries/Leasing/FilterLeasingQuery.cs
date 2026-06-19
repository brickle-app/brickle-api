using BricklePlatform.Api.Application.Models;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Enums;
using MediatR;

namespace BricklePlatform.Api.Application.Queries.Leasing;

public class FilterLeasingQuery : IRequest<PaginatedResult<LeasingDto>>
{
    public IEnumerable<LeasingTypeEnum>? Categories { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
    public bool? Active { get; set; }

    public FilterLeasingQuery(int page, int limit, IEnumerable<LeasingTypeEnum>? categories = null, bool? active = null)
    {
        Page = page;
        Limit = limit;
        Categories = categories;
        Active = active;
    }
} 