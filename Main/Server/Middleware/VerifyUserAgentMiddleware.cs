using AsciiPinyin.Web.Server.Commons;
using AsciiPinyin.Web.Server.Constants;
using AsciiPinyin.Web.Shared.Constants;
using System.Net;

namespace AsciiPinyin.Web.Server.Middleware;

public sealed class VerifyUserAgentMiddleware(
    ILogger<VerifyUserAgentMiddleware> _logger,
    RequestDelegate _next
)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.ContainsKey(RequestHeaderKeys.USER_AGENT))
        {
            LogCommons.LogError(_logger, Errors.USER_AGENT_MISSING);
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = ContentTypes.TEXT_PLAIN;
            await context.Response.WriteAsync(Errors.USER_AGENT_MISSING);
            return;
        }

        await _next(context);
    }
}
