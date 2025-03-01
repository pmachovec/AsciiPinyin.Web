using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Utils;
using System.ComponentModel.DataAnnotations;

namespace AsciiPinyin.Web.Server.Validation.Attributes;

internal sealed class ValidRadicalAlternativeCharacterAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            // Missing radical alternative character is a valid input in all situations.
            return ValidationResult.Success!;
        }

        // The value is is definitely string at this point.
        var radicalAlternativeCharacter = (string)value;

        if (radicalAlternativeCharacter.Length == 0)
        {
            return new ValidationResult(Errors.EMPTY);
        }
        else if (TextUtils.GetStringRealLength(radicalAlternativeCharacter) > 1)
        {
            return new ValidationResult(Errors.ONLY_ONE_CHARACTER_ALLOWED);
        }
        else if (!TextUtils.IsOnlyChineseCharacters(radicalAlternativeCharacter))
        {
            return new ValidationResult(Errors.NO_SINGLE_CHINESE);
        }

        return ValidationResult.Success!;
    }
}
