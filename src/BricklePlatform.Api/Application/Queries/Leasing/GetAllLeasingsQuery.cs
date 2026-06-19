using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Queries.Leasing;

public record GetAllLeasingsQuery(bool? Active = null) : IRequest<IEnumerable<LeasingDto>>;