using AsciiPinyin.Web.Server.Validation.Attributes;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace AsciiPinyin.Web.Server.Validation;

internal sealed class ControllerValidationMetadataProvider : IValidationMetadataProvider
{
    public void CreateValidationMetadata(ValidationMetadataProviderContext context)
    {
        if (context.Key.MetadataKind == ModelMetadataKind.Property)
        {
            if (context.Key.ContainerType == typeof(Chachar))
            {
                AddChacharAttributes(context);
            }
            else if (context.Key.ContainerType == typeof(Alternative))
            {
                AddAlternativeAttributes(context);
            }
        }
    }

    private static void AddChacharAttributes(ValidationMetadataProviderContext context)
    {
        Chachar? dummyChachar;

        switch (context.Key.Name)
        {
            case nameof(dummyChachar.TheCharacter):
                context.ValidationMetadata.ValidatorMetadata.Add(new ValidCharacterAttribute());
                break;

            case nameof(dummyChachar.Pinyin):
                context.ValidationMetadata.ValidatorMetadata.Add(new ValidPinyinAttribute());
                break;

            case nameof(dummyChachar.Tone):
                context.ValidationMetadata.ValidatorMetadata.Add(new ValidToneAttribute());
                break;

            case nameof(dummyChachar.Ipa):
                context.ValidationMetadata.ValidatorMetadata.Add(new ValidIpaAttribute());
                break;

            case nameof(dummyChachar.Strokes):
                context.ValidationMetadata.ValidatorMetadata.Add(new ValidStrokesAttribute());
                break;

            case nameof(dummyChachar.RadicalCharacter):
                context.ValidationMetadata.ValidatorMetadata.Add(new ValidRadicalCharacterAttribute());
                break;

            case nameof(dummyChachar.RadicalPinyin):
                context.ValidationMetadata.ValidatorMetadata.Add(new ValidRadicalPinyinAttribute());
                break;

            case nameof(dummyChachar.RadicalTone):
                context.ValidationMetadata.ValidatorMetadata.Add(new ValidRadicalToneAttribute());
                break;

            case nameof(dummyChachar.RadicalAlternativeCharacter):
                context.ValidationMetadata.ValidatorMetadata.Add(new ValidRadicalAlternativeCharacterAttribute());
                break;

            default:
                break;
        }
    }

    private static void AddAlternativeAttributes(ValidationMetadataProviderContext context)
    {
        Alternative? dummyAlternative;

        switch (context.Key.Name)
        {
            case nameof(dummyAlternative.TheCharacter):
            case nameof(dummyAlternative.OriginalCharacter):
                context.ValidationMetadata.ValidatorMetadata.Add(new ValidCharacterAttribute());
                break;

            case nameof(dummyAlternative.OriginalPinyin):
                context.ValidationMetadata.ValidatorMetadata.Add(new ValidPinyinAttribute());
                break;

            case nameof(dummyAlternative.OriginalTone):
                context.ValidationMetadata.ValidatorMetadata.Add(new ValidToneAttribute());
                break;

            case nameof(dummyAlternative.Strokes):
                context.ValidationMetadata.ValidatorMetadata.Add(new ValidStrokesAttribute());
                break;

            default:
                break;
        }
    }
}
