using AsciiPinyin.Web.Shared.Commons;
using AsciiPinyin.Web.Shared.Constants;
using AsciiPinyin.Web.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace AsciiPinyin.Web.Server.Validation.Attributes;

internal sealed class ValidRadicalPinyinAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        // The object instance is is definitely Chachar at this point.
        var chachar = (Chachar)validationContext.ObjectInstance;

        if (
            value is null
            && (chachar.RadicalCharacter is not null || chachar.RadicalTone is not null || chachar.RadicalAlternativeCharacter is not null)
        )
        {
            return new ValidationResult(Errors.MISSING);
        }

        // The value is null or string at this point.
        if (value is string radicalPinyin)
        {
            if (radicalPinyin.Length == 0)
            {
                return new ValidationResult(Errors.EMPTY);
            }
            else if (!Regexes.AsciiLettersRegex().IsMatch(radicalPinyin))
            {
                return new ValidationResult(Errors.NO_ASCII);
            }
        }

        return ValidationResult.Success!;
    }
}
