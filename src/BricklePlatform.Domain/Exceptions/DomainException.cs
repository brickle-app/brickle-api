using System.Net;

namespace BricklePlatform.Domain.Exceptions;

public class DomainException(HttpStatusCode code, object? errors = null) : Exception
{
    public HttpStatusCode Code { get; } = code;
    public object? Errors { get; } = errors;
}

public class BadRequestException : DomainException
{
    public BadRequestException(object? errors = null)
        : base(HttpStatusCode.BadRequest, errors)
    {
    }
}

public class NotFoundException : DomainException
{
    public NotFoundException(object? errors = null)
        : base(HttpStatusCode.NotFound, errors)
    {
    }
}

public class ConflictException : DomainException
{
    public ConflictException(object? errors = null)
        : base(HttpStatusCode.Conflict, errors)
    {
    }
}

public class InternalServerErrorException : DomainException
{
    public InternalServerErrorException(object? errors = null)
        : base(HttpStatusCode.InternalServerError, errors)
    {
    }
}