using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace AsciiPinyin.Web.Server.Validation.Attributes;

internal sealed class ValidRadicalToneAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        // The object instance is is definitely Chachar at this point.
        var chachar = (Chachar)validationContext.ObjectInstance;

        if (
            value is null
            && (chachar.RadicalCharacter is not null || chachar.RadicalPinyin is not null || chachar.RadicalAlternativeCharacter is not null)
        )
        {
            return new ValidationResult(Errors.MISSING);
        }

        // The value is null or string at this point.
        // As the API doesn't allow any invalid value like strings, negative numbers etc., this is the only condition to verify.
        return value is byte radicalTone && radicalTone > ByteConstants.MAX_TONE
            ? new ValidationResult(Errors.ZERO_TO_FOUR)
            : ValidationResult.Success!;
    }
}
