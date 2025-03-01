using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Utils;
using System.ComponentModel.DataAnnotations;

namespace AsciiPinyin.Web.Server.Validation.Attributes;

internal sealed class ValidIpaAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return new ValidationResult(Errors.MISSING);
        }

        // The value is is definitely string at this point.
        var ipa = (string)value;

        if (ipa.Length == 0)
        {
            return new ValidationResult(Errors.EMPTY);
        }
        else if (!TextUtils.IsOnlyIpaCharacters(ipa))
        {
            return new ValidationResult(Errors.NO_IPA);
        }

        return ValidationResult.Success!;
    }
}
