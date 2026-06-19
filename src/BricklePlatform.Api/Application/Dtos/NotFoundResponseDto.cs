using Newtonsoft.Json;

namespace BricklePlatform.Api.Application.Dtos;

public record NotFoundResponseDto
{
    [JsonProperty("message")]
    public string Message { get; set; } = null!;
}