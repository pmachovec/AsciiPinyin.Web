using Newtonsoft.Json.Linq;
using System.Text.Json.Serialization;

namespace AsciiPinyin.Web.Models
{
    public class Chachar
    {
        // Workaround for nulls.
        private string ipa = "";
        private string pinyin = "";

        [JsonPropertyName("ipa")]
        public string Ipa { get => ipa; set => ipa = value; }

        [JsonPropertyName("pinyin")]
        public string Piniyin { get => pinyin; set => pinyin = value; }

        /*
         * The number of strokes can't be lower than 1 => using unsigned type is possible.
         * Highest theoretically possible value is 84 => byte is enough (byte is unsigned, signed would be sbyte).
         */
        [JsonPropertyName("strokes")]
        public byte Strokes { get; set; }

        // Not needed, can be retrieved from Unicode.
        //[JsonPropertyName("the_character")]
        //public char TheCharacter { get; set; }

        /*
         * The code can't be lower than 1 => using unsigned type is possible.
         * Highest theoretically possible code is 0x3134F = 201551 => ushort is too low (65535), uint is needed.
         */
        [JsonPropertyName("unicode")]
        public uint Unicode { get; set; }

        public override bool Equals(object? obj)
        {
            return (obj != null) && (obj is Chachar chachar) && (Unicode == chachar.Unicode);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Unicode, Piniyin, Ipa);
        }

        // This doesn't work properly since the character was removed, but the character is still wanted in JSON.
        //public override string ToString() => JsonSerializer.Serialize(this);

        public override string ToString()
        {
            JObject jo = JObject.FromObject(this);
            jo.Add("the_character", (char)Unicode);
            return jo.ToString();
        }
    }
}
