using Newtonsoft.Json;

namespace BricklePlatform.Api.Application.Dtos
{
    public record BadRequestResponseDto
    {
        [JsonProperty("type")]
        public string Type { get; set; } = null!;

        [JsonProperty("title")]
        public string Title { get; set; } = null!;

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("traceId")]
        public string TraceId { get; set; } = null!;

        [JsonProperty("errors")]
        public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
    }
}