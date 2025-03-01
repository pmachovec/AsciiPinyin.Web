using AsciiPinyin.Web.Server.Commons;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Reflection;

namespace AsciiPinyin.Web.Server.Controllers.ActionFilters;

/// <summary>
/// To be used as a replacement for the default model state invalid filter.
/// Sets labels of error arrays to values of 'DisplayName' attributes of corresponding properties (by default, the label is the corresponding property name).
/// Reason: Eventual processing of errors on the client side. The client doesn't know property names by default, it knows only value labels.
/// If 'DisplayName' attribute is not set for a property, the default behavior - using the property name - is preserved.
/// Additionally, errors are logged.
/// </summary>
/// <param name="_apiBehaviorOptions">Used to retrieve the response type value, an instance is available by default by dependency injection.</param>
internal sealed class ModelStateInvalidFilter(
    ILogger<ModelStateInvalidFilter> _logger,
    IOptions<ApiBehaviorOptions> _apiBehaviorOptions
) : ActionFilterAttribute
{
    private const string DOLLAR_START = "$.";

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var problemDetails = new ValidationProblemDetails(context.ModelState)
            {
                Type = _apiBehaviorOptions.Value.ClientErrorMapping[StatusCodes.Status400BadRequest].Link
            };

            problemDetails.Extensions["traceId"] = Activity.Current?.Id;

            if (context.ActionDescriptor.Parameters.FirstOrDefault()?.ParameterType is { } parameterType)
            {
                var errors = new Dictionary<string, string[]>();

                foreach (var entry in context.ModelState)
                {
                    var propertyName = entry.Key;
                    var property = parameterType.GetProperty(propertyName);
                    var displayName = property?.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? propertyName;
                    errors[displayName] = [.. entry.Value.Errors.Select(e => e.ErrorMessage.Replace(propertyName, displayName))];
                }

                problemDetails.Errors.Clear();
                var tableName = parameterType.GetCustomAttribute<TableAttribute>()?.Name;

                foreach (var error in errors)
                {
                    // Gets rid of the "missing field" error for the whole entity when the value contains a mismatch or is not a valid JSON.
                    if (tableName is null || errors.Count == 1 || error.Key != tableName)
                    {
                        // Gets rid of the "$." prefix in the key if it's there.
                        var key = error.Key.StartsWith(DOLLAR_START, StringComparison.InvariantCulture)
                            ? error.Key[2..]
                            : error.Key;

                        problemDetails.Errors.Add(key, error.Value);
                        LogCommons.LogFieldErrors(_logger, key, string.Join(',', error.Value));
                    }
                }
            }

            context.Result = new BadRequestObjectResult(problemDetails);
        }
    }
}
