using AsciiPinyin.Web.Client.Shared.Resources;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace AsciiPinyin.Web.Client.Shared;

public class SafeLocalization
{
    public static string GetLocalizedString(IStringLocalizer<Resource> localizer, string theString, string callerNameForLogs)
    {
        if (localizer[theString] == null)
        {
            Console.WriteLine($"SafeLocalizationSingleton.GetLocalizedString: Localized value for '{theString}' not found in {callerNameForLogs}.");
            return GetInvariantString(theString, callerNameForLogs);
        }

        return localizer[theString];
    }

    private static string GetInvariantString(string theString, string callerNameForLogs)
    {
        var invariantString = Resource.ResourceManager.GetString(theString, CultureInfo.InvariantCulture);

        if (invariantString == null)
        {
            Console.WriteLine($"SafeLocalizationSingleton.GetInvariantString: Invariant value for '{theString}' not found in {callerNameForLogs}.");
            return theString;
        }

        return invariantString;
    }
}
