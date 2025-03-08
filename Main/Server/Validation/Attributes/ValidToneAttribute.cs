using AsciiPinyin.Web.Shared.Constants;
using System.ComponentModel.DataAnnotations;

namespace AsciiPinyin.Web.Server.Validation.Attributes;

internal sealed class ValidToneAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return new ValidationResult(Errors.MISSING);
        }

        // The value is is definitely unsigned byte at this point.
        var tone = (short)value;

        // As the API doesn't allow any invalid value like strings, negative numbers etc., this is the only condition to verify.
        return tone > NumberConstants.MAX_TONE
            ? new ValidationResult(Errors.ZERO_TO_FOUR)
            : ValidationResult.Success!;
    }
}
