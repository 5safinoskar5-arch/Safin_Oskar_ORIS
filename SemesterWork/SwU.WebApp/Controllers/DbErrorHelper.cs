using Npgsql;

namespace SwU.WebApp.Controllers;

internal static class DbErrorHelper
{
    public static string GetMessage(PostgresException exception, string? uniqueMessage = null)
    {
        switch (exception.SqlState)
        {
            case PostgresErrorCodes.UniqueViolation:
                return uniqueMessage ?? "A record with these values already exists.";
            case PostgresErrorCodes.ForeignKeyViolation:
                return "The related record was not found or is still used by other data.";
            case PostgresErrorCodes.StringDataRightTruncation:
                return "One of the values is too long for the database.";
            case PostgresErrorCodes.CheckViolation:
                return "A value is outside the allowed range.";
            default:
                return "Could not save the data. Please check the entered values.";
        }
    }
}
