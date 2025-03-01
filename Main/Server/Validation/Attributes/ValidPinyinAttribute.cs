using AsciiPinyin.Web.Shared.Commons;
using AsciiPinyin.Web.Shared.Constants;
using System.ComponentModel.DataAnnotations;

namespace AsciiPinyin.Web.Server.Validation.Attributes;

internal sealed class ValidPinyinAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return new ValidationResult(Errors.MISSING);
        }

        // The value is is definitely string at this point.
        var pinyin = (string)value;

        if (pinyin!.Length == 0)
        {
            return new ValidationResult(Errors.EMPTY);
        }
        else if (!Regexes.AsciiLettersRegex().IsMatch(pinyin!))
        {
            return new ValidationResult(Errors.NO_ASCII);
        }

        return ValidationResult.Success!;
    }
}
