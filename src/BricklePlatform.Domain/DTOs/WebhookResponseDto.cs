namespace BricklePlatform.Domain.DTOs;

public class WebhookResponseDto
{
    public string Hash { get; set; } = string.Empty;
    public bool Status { get; set; }
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Parsea la respuesta del webhook Defender/relayer. Soporta múltiples formatos:
    /// - Formato directo autotask: { "statusCode": 200, "body": "{\"hash\":\"0x...\",\"status\":1}" }
    /// - Formato Defender envuelto: { "result": { "statusCode": 200, "body": "..." } }
    /// - Formato con result string: { "result": "{\"body\":\"...\"}" }
    /// - Formato error: { "error": "...", "details": { "message": "...", "reason": "..." } }
    /// </summary>
    public static WebhookResponseDto FromWebhookResult(string resultJson)
    {
        using var doc = System.Text.Json.JsonDocument.Parse(resultJson);
        var root = doc.RootElement;

        // 1. Detectar si es respuesta de error (statusCode 4xx/5xx o campo error)
        if (root.TryGetProperty("error", out var errorProp))
        {
            var errorMsg = errorProp.GetString() ?? "Error desconocido";
            string? details = null;
            if (root.TryGetProperty("details", out var detailsProp))
            {
                if (detailsProp.TryGetProperty("reason", out var reasonProp))
                    details = reasonProp.GetString();
                else if (detailsProp.TryGetProperty("shortMessage", out var shortProp))
                    details = shortProp.GetString();
                else if (detailsProp.TryGetProperty("message", out var msgProp))
                    details = msgProp.GetString();
                else
                    details = detailsProp.GetRawText();
            }
            return new WebhookResponseDto
            {
                Hash = string.Empty,
                Status = false,
                ErrorMessage = string.IsNullOrEmpty(details) ? errorMsg : $"{errorMsg}: {details}"
            };
        }

        // 2. Obtener el body (string JSON) desde distintos formatos
        string? bodyJson = null;

        if (root.TryGetProperty("body", out var bodyDirect))
        {
            bodyJson = bodyDirect.GetString();
        }
        else if (root.TryGetProperty("result", out var resultProp))
        {
            if (resultProp.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                using var resultDoc = System.Text.Json.JsonDocument.Parse(resultProp.GetString() ?? "{}");
                if (resultDoc.RootElement.TryGetProperty("body", out var bodyInResult))
                    bodyJson = bodyInResult.GetString();
                else
                    bodyJson = resultProp.GetString();
            }
            else if (resultProp.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                if (resultProp.TryGetProperty("body", out var bodyInResult))
                    bodyJson = bodyInResult.GetString();
            }
        }

        if (string.IsNullOrEmpty(bodyJson))
            return new WebhookResponseDto
            {
                Hash = string.Empty,
                Status = false,
                ErrorMessage = "Formato de respuesta del webhook no reconocido. Respuesta: " + (resultJson.Length > 200 ? resultJson[..200] + "..." : resultJson)
            };

        // 3. Parsear body para extraer hash y status
        using var bodyDoc = System.Text.Json.JsonDocument.Parse(bodyJson);
        var body = bodyDoc.RootElement;

        var hash = body.TryGetProperty("hash", out var hashProp) ? hashProp.GetString() ?? string.Empty
            : body.TryGetProperty("txHash", out var txHashProp) ? txHashProp.GetString() ?? string.Empty
            : string.Empty;

        var status = body.TryGetProperty("status", out var statusProp)
            ? (statusProp.ValueKind == System.Text.Json.JsonValueKind.Number ? statusProp.GetInt32() == 1 : statusProp.GetBoolean())
            : false;

        if (!status)
        {
            string? errMsg = null;
            if (body.TryGetProperty("error", out var bodyError))
                errMsg = bodyError.GetString();
            else if (body.TryGetProperty("details", out var details))
                errMsg = details.TryGetProperty("message", out var msg) ? msg.GetString()
                    : details.TryGetProperty("reason", out var reason) ? reason.GetString() : details.GetRawText();
            return new WebhookResponseDto { Hash = hash, Status = false, ErrorMessage = errMsg };
        }

        return new WebhookResponseDto { Hash = hash, Status = status };
    }
} 