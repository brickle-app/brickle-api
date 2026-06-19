using BricklePlatform.Api.Models;
using MediatR;

namespace BricklePlatform.Api.Application.Queries.File;

public record GetFileQuery(
    HeaderRequestModel Header,
    string EntityType,
    Guid EntityId,
    string? FileType = null) : IRequest<string>;