using System.Text.Json;
using System.Text.Json.Serialization;

namespace AsciiPinyin.Web.Models
{
    public class Chachar
    {
        // Workaround for nulls.
        private string ipa = "";
        private string pinyin = "";
        private string unicode = "";

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

        [JsonPropertyName("unicode")]
        public string Unicode { get => unicode; set => unicode = value; }

        /*
         * Is not read from the database Json, doesn't have 'set' configured. But must have JsonProperty, because that serves even
         * for the serialization to Json.
         */
        [JsonPropertyName("the_character")]
        public char TheCharacter
        {
            get
            {
                uint unicodeAsInt = 0;

                try
                {
                    unicodeAsInt = uint.Parse(Unicode, System.Globalization.NumberStyles.HexNumber);
                }
                catch (ArgumentException)
                {
                    Console.Error.WriteLine($"Failed parsing of unicode '{Unicode}'.");
                }

                if (unicodeAsInt > 0)
                {
                    return (char)unicodeAsInt;
                }

                return '\x0000';
            }
        }

        public override bool Equals(object? obj)
        {
            return (obj != null) && (obj is Chachar chachar) && (Unicode == chachar.Unicode);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Unicode, Piniyin, Ipa);
        }

        public override string ToString()
        {
            var options = new JsonSerializerOptions 
            { 
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping 
            };
            
            return JsonSerializer.Serialize(this, options);
        }
    }
}
