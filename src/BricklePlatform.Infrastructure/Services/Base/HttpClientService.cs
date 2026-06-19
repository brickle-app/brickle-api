using BricklePlatform.Infrastructure.Exceptions;
using BricklePlatform.Infrastructure.Interfaces;
using BricklePlatform.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace BricklePlatform.Infrastructure.Services.Base;

public class HttpClientService : IHttpClientService
{
    private readonly IHttpClientFactory _httpClient;
    private readonly ILogger<HttpClientService> _logger;

    public HttpClientService(
        IHttpClientFactory httpClient,
        ILogger<HttpClientService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Tuple<bool, string>> MakeRequestWithHeaders(RequestHttpModel request)
    {
        try
        {
            HttpResponseMessage response = await ExecuteRequest(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return Tuple.Create(false, "404 Not Found");
            }

            if (!response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                throw new InfrastructureException($"Error en la respuesta HTTP: {response.StatusCode} - {responseContent}");
            }

            (string errorMessage, string result) = await ReadHttpResponseFromJson<string>(response);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                throw new InfrastructureException(errorMessage);
            }

            return Tuple.Create(true, result ?? string.Empty);
        }
        catch (Exception ex)
        {
            string error = $"Error realizando petición {request.Method}, con los parámetros: {JsonConvert.SerializeObject(request, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })}";
            _logger.LogError("{message}", $"{error}, Detalle: {ex}");
            throw new InfrastructureException(error, ex);
        }
    }

    private async Task<HttpResponseMessage> ExecuteRequest(RequestHttpModel request)
    {
        HttpClient client = string.IsNullOrEmpty(request.HttpClientName) 
            ? _httpClient.CreateClient() 
            : _httpClient.CreateClient(request.HttpClientName);

        client.DefaultRequestHeaders
               .Accept
               .Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (request.Headers != null && request.Headers.Any())
        {
            foreach (KeyValuePair<string, string> header in request.Headers)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        HttpMethod httpMethod = request.Method switch
        {
            "GET" => HttpMethod.Get,
            "PUT" => HttpMethod.Put,
            "PATCH" => HttpMethod.Patch,
            _ => HttpMethod.Post,
        };

        using HttpRequestMessage httpRequest = new HttpRequestMessage(httpMethod, request.Url);

        if (!string.IsNullOrEmpty(request.Body))
        {
            httpRequest.Content = new StringContent(request.Body, Encoding.UTF8);
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        }

        return await client.SendAsync(httpRequest);
    }

    private async Task<Tuple<string, T?>> ReadHttpResponseFromJson<T>(HttpResponseMessage httpResponse)
    {
        if (httpResponse.Content == null || httpResponse.Content.Headers.ContentLength == 0)
        {
            httpResponse.Dispose();
            return Tuple.Create(string.Empty, default(T));
        }

        try
        {
            string content = await httpResponse.Content.ReadAsStringAsync();

            if (typeof(T) == typeof(string))
            {
                httpResponse.Dispose();
                return Tuple.Create(string.Empty, (T)(object)content);
            }

            T? objectResult = JsonConvert.DeserializeObject<T>(content);
            httpResponse.Dispose();
            return Tuple.Create(string.Empty, objectResult);
        }
        catch (Exception ex)
        {
            string errorMessage = $"Error al intentar leer el contenido de la respuesta http: {httpResponse.ToString()}.  Detalle del error: {ex.Message}";
            _logger.LogError("{Message}", $"--- HttpClientService {errorMessage} ---");
            return Tuple.Create(errorMessage, default(T));
        }
    }
}