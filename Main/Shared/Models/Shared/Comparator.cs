namespace AsciiPinyin.Web.Shared.Models.Shared;

internal static class Comparator
{
    public static bool EqualsForOperator<T>(T left, T right) where T : IEntity
    {
        if (left is null)
        {
            // If left is null, right must be also null to return true.
            return right is null;
        }

        // Handles the case of null only on the right.
        return left.Equals(right);
    }
}
