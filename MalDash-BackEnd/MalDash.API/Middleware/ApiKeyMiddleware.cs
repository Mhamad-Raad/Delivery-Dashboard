namespace MalDash.API.Middleware;

public class ApiKeyMiddleware
{
    private const string ApiKeyHeaderName = "MalDash-Api-Key";
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiKeyMiddleware> _logger;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip API key validation for specific paths (OpenAPI, Scalar, health checks)
        var path = context.Request.Path.Value?.ToLower() ?? string.Empty;
        if (path.StartsWith("/openapi") ||
            path.StartsWith("/scalar") ||
            path.StartsWith("/health"))
        {
            await _next(context);
            return;
        }

        // Try to get the API key with case-insensitive comparison
        var apiKeyHeader = context.Request.Headers
            .FirstOrDefault(h => h.Key.Equals(ApiKeyHeaderName, StringComparison.OrdinalIgnoreCase));

        // If not found, check if Scalar is sending it as separate "key" and "value" headers
        string? extractedApiKey = null;

        if (!string.IsNullOrEmpty(apiKeyHeader.Key))
        {
            extractedApiKey = apiKeyHeader.Value.ToString();
        }
        else if (context.Request.Headers.TryGetValue("key", out var keyValue) &&
                 keyValue.ToString().Equals(ApiKeyHeaderName, StringComparison.OrdinalIgnoreCase) &&
                 context.Request.Headers.TryGetValue("value", out var valueValue))
        {
            // Scalar sends key/value as separate headers
            extractedApiKey = valueValue.ToString();
        }

        if (string.IsNullOrEmpty(extractedApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "API Key is missing",
                message = $"Please provide a valid API key in the '{ApiKeyHeaderName}' header"
            });
            return;
        }

        // Validate the API key
        var apiKey = _configuration["ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Server configuration error",
                message = "API key is not configured on the server"
            });
            return;
        }

        if (!apiKey.Equals(extractedApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Invalid API Key",
                message = "The provided API key is not valid"
            });
            return;
        }

        // API key is valid, continue processing
        await _next(context);
    }
}