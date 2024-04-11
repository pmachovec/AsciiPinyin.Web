namespace AsciiPinyin.Web.Shared.Models.Tools;

internal static class Comparator
{
    public static bool EqualsForOperator<TEntity>(TEntity left, TEntity right) where TEntity : IEntity
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
