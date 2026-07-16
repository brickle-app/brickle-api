using System.Text.Json;
using BricklePlatform.Api.Application.Commands.UserDocument;
using Xunit;

namespace BricklePlatform.Test.Commands.UserDocument;

public class UpdateUserDocumentStatusCommandSerializationTests
{
    [Fact]
    public void CanDeserializeAdminPayloadWithoutRouteId()
    {
        var command = JsonSerializer.Deserialize<UpdateUserDocumentStatusCommand>(
            "{\"status\":\"APPROVED\"}",
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(command);
        Assert.Equal("APPROVED", command!.Status);
        Assert.Equal(Guid.Empty, command.Id);
    }
}
