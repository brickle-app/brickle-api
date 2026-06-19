namespace BricklePlatform.Infrastructure.Exceptions;

public class InfrastructureException : Exception
{
    public int StatusCode { get; }

    public InfrastructureException()
    { }

    public InfrastructureException(string message, int statusCode = 500)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public InfrastructureException(string message, Exception innerException, int statusCode = 500)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    public static InfrastructureException NotFound(string message, Exception innerException = null)
    {
        return new InfrastructureException(message, innerException, 404);
    }

    public static InfrastructureException PreconditionFailed(string message, Exception innerException = null)
    {
        return new InfrastructureException(message, innerException, 412);
    }
}