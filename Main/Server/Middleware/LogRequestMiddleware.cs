using AsciiPinyin.Web.Server.Commons;
using Microsoft.AspNetCore.Http.Extensions;

namespace AsciiPinyin.Web.Server.Middleware;

public sealed class LogRequestMiddleware(
    ILogger<LogRequestMiddleware> _logger,
    RequestDelegate _next
)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        LogCommons.LogRequestReceivedDebug(_logger);
        LogCommons.LogMethodDebug(_logger, request.Method);
        LogCommons.LogUrlDebug(_logger, request.GetDisplayUrl());
        LogCommons.LogHeadersDebug(_logger, string.Join(',', request.Headers.Select(h => $"{h.Key}:{h.Value}")));

        if (request.Body is { } body)
        {
            // The body is HttpRequestStream.
            // Checking the length of HttpRequestStream is not supported.
            // It's necessary to check the string length.
            // When the stream is read to string, its position is at the end.
            // Reading the body again further in the program would fail.
            // It's necessary to reset the stream position to 0.
            // Setting the postition of HttpRequestStream is not supported.
            // It's necessary to copy the original body stream to a MemoryStream, which supports position setting.
            // Copying the HttpRequestStream causes its position to be at the end.
            // When reading to string, it's necessary to use the MemoryStream.
            // After the MemoryStream is read, its position must be reset to 0.
            // After the MemoryStream position is reset, it must be set as the request body.
            var bodyMemoryStream = new MemoryStream();
            await body.CopyToAsync(bodyMemoryStream);

            using var streamReader = new StreamReader(bodyMemoryStream, leaveOpen: true);
            var bodyString = await streamReader.ReadToEndAsync(CancellationToken.None);

            if (!string.IsNullOrEmpty(bodyString))
            {
                LogCommons.LogBodyDebug(_logger, bodyString);
            }

            bodyMemoryStream.Position = 0;
            request.Body = bodyMemoryStream;
        }

        await _next(context);
    }
}
