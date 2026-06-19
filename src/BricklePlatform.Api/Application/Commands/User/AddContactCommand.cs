using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using MediatR;

namespace BricklePlatform.Api.Application.Commands.User;

public record AddContactCommand(HeaderRequestModel Header, Guid UserId, AddContactDto AddContactDto) : IRequest<ContactDto>;