using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Queries.User;

public record GetUserContactsQuery(HeaderRequestModel Header, Guid UserId) : IRequest<IEnumerable<ContactDto>>; 