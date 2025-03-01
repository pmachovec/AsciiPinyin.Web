using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using AsciiPinyin.Web.Shared.Utils;
using System.ComponentModel.DataAnnotations;

namespace AsciiPinyin.Web.Server.Validation.Attributes;

internal sealed class ValidRadicalCharacterAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        // The object instance is is definitely Chachar at this point.
        var chachar = (Chachar)validationContext.ObjectInstance;

        if (
            value is null
            && (chachar.RadicalPinyin is not null || chachar.RadicalTone is not null || chachar.RadicalAlternativeCharacter is not null)
        )
        {
            return new ValidationResult(Errors.MISSING);
        }

        // The value is null or string at this point.
        if (value is string radicalCharacter)
        {
            if (radicalCharacter.Length == 0)
            {
                return new ValidationResult(Errors.EMPTY);
            }
            else if (TextUtils.GetStringRealLength(radicalCharacter) > 1)
            {
                return new ValidationResult(Errors.ONLY_ONE_CHARACTER_ALLOWED);
            }
            else if (!TextUtils.IsOnlyChineseCharacters(radicalCharacter))
            {
                return new ValidationResult(Errors.NO_SINGLE_CHINESE);
            }
        }

        return ValidationResult.Success!;
    }
}
