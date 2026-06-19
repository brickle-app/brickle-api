using BricklePlatform.Domain.DTOs;
using MediatR;
using System;

namespace BricklePlatform.Api.Application.Commands.UserDocument;

public class UpdateUserDocumentStatusCommand : IRequest<UserDocumentDto>
{
    public Guid Id { get; set; }
    public string Status { get; set; } = null!;
    public string? Observation { get; set; }

    public UpdateUserDocumentStatusCommand(Guid id, string status, string? observation = null)
    {
        Id = id;
        Status = status;
        Observation = observation;
    }
}
