using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AsciiPinyin.Web.Shared.Models;

[Table("alternative")]
public class Alternative : IEntity
{
    [JsonPropertyName("the_character")]
    [Column("the_character")]
    [Required]
    public char TheCharacter { get; set; } = '\x0000';

    [JsonPropertyName("original_character")]
    [Column("original_character")]
    [Required]
    public char OriginalCharacter { get; set; } = '\x0000';

    [JsonPropertyName("original_ascii_pinyin")]
    [Column("original_ascii_pinyin")]
    [Required]
    public string OriginalAsciiPinyin { get; set; } = "";

    [JsonPropertyName("strokes")]
    [Column("strokes")]
    [Required]
    public byte Strokes { get; set; }
}
