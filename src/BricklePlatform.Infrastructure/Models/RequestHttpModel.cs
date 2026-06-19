namespace BricklePlatform.Infrastructure.Models;

public class RequestHttpModel
{
    public string HttpClientName { get; set; }
    public string Url { get; set; }

    public string Method { get; set; }
    public string Body { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
}