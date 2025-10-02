namespace Nova.API.Application.Helpers;

public static class DateTimeHelper
{
    public static DateTime EnsureUtc(DateTime dateTime)
    {
        return dateTime.Kind == DateTimeKind.Utc ? dateTime : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    }
    public static DateTime? EnsureUtc(DateTime? dateTime)
    {
        return dateTime.HasValue ? EnsureUtc(dateTime.Value) : null;
    }
}