using AsciiPinyin.Web.Shared.Constants;
using System.Text.Json.Serialization;

namespace AsciiPinyin.Web.Shared.DTO;

public sealed class DatabaseIntegrityError(
    string error,
    IEnumerable<Conflict> conflicts
)
{
    public DatabaseIntegrityError(string errorMessage) : this(errorMessage, [])
    {
    }

    [JsonPropertyName(JsonPropertyNames.ERROR)]
    public string Error { get; } = error;

    [JsonPropertyName(JsonPropertyNames.CONFLICTS)]
    public IEnumerable<Conflict> Conflicts { get; } = conflicts;
}
