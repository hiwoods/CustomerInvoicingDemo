namespace Invoicing.Core.Utilities;

public static class DateTimeUtilities
{
    public static bool IsValidSqlDateTime(DateTime? dateTime)
    {
        if (dateTime is null) return true;

        DateTime minValue = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue;
        DateTime maxValue = (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue;

        return minValue <= dateTime.Value && maxValue >= dateTime.Value;
    }

    public static bool IsValidSqlDateTime(DateOnly? dateOnly) => IsValidSqlDateTime(dateOnly?.ToDateTime(TimeOnly.MinValue));
}