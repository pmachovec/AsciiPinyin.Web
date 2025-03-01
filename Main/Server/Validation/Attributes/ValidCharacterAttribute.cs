using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Utils;
using System.ComponentModel.DataAnnotations;

namespace AsciiPinyin.Web.Server.Validation.Attributes;

internal sealed class ValidCharacterAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return new ValidationResult(Errors.MISSING);
        }

        // The value is is definitely string at this point.
        var theCharacter = (string)value;

        if (theCharacter!.Length == 0)
        {
            return new ValidationResult(Errors.EMPTY);
        }
        else if (TextUtils.GetStringRealLength(theCharacter!) > 1)
        {
            return new ValidationResult(Errors.ONLY_ONE_CHARACTER_ALLOWED);
        }
        else if (!TextUtils.IsOnlyChineseCharacters(theCharacter!))
        {
            return new ValidationResult(Errors.NO_SINGLE_CHINESE);
        }

        return ValidationResult.Success!;
    }
}
