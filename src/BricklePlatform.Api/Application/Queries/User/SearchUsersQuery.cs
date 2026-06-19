using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Queries.User;

public record SearchUsersQuery(HeaderRequestModel Header, string? SearchTerm = null, Guid? ExcludeUserId = null) : IRequest<IEnumerable<ContactDto>>; 