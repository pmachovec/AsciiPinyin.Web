namespace AsciiPinyin.Web.Shared.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

[Table("chachar")]
public class Chachar: IEntity
{
    [JsonPropertyName("the_character")]
    [Column("the_character")]
    [Key]
    public char TheCharacter { get; set; } = '\x0000';

    [JsonPropertyName("pinyin")]
    [Column("pinyin")]
    [Key]
    public string Piniyin { get; set; } = "";

    [JsonPropertyName("ipa")]
    [Column("ipa")]
    [Required]
    public string Ipa { get; set; } = "";

    /*
     * The number of strokes can't be lower than 1 => using unsigned type is possible.
     * Highest theoretically possible value is 84 => byte is enough (byte is unsigned, signed would be sbyte).
     */
    [JsonPropertyName("strokes")]
    [Column("strokes")]
    [Required]
    public byte Strokes { get; set; }

    public override bool Equals(object? other)
    {
        return other is Chachar otherChachar && otherChachar.TheCharacter == TheCharacter && otherChachar.Piniyin == Piniyin;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TheCharacter, Piniyin);
    }

    public override string ToString()
    {
        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        return JsonSerializer.Serialize(this, options);
    }
}
