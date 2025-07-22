using System.Globalization;
using CalendarExample.Models;
using Microsoft.Data.Sqlite;

namespace CalendarExample;

public class DatabaseManager
{
    private const string CONNECTION_STRING = "Data Source=Database/db.db";

    public static async Task<bool> DeleteAsync(int id) 
    {
        using var connection = new SqliteConnection(CONNECTION_STRING);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM events WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id);

        int rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public static async Task<EventModel?> GetByIdAsync(int id)
    {
        using var connection = new SqliteConnection(CONNECTION_STRING);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM events WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new EventModel
            {
                Id = reader.GetInt32(0),
                DateOf = DateTime.ParseExact(reader.GetString(1), "yyyy-MM-dd", CultureInfo.InvariantCulture),
                StartTime = TimeSpan.Parse(reader.GetString(2)),
                EndTime = reader.IsDBNull(3) ? TimeSpan.Zero : TimeSpan.Parse(reader.GetString(3)),
                Title = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                Description = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                Location = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
            };
        }

        return null; // no match found
    }

    public static async Task<IEnumerable<EventModel>> GetByMonthWithFullWeeksAsync(DateTime dateInMonth)
    {
        var events = new List<EventModel>();

        // Step 1: Get the first and last day of the target month
        DateTime firstOfMonth = new(dateInMonth.Year, dateInMonth.Month, 1);
        DateTime lastOfMonth = firstOfMonth.AddMonths(1).AddDays(-1);

        // Step 2: Extend range to include full weeks (Monday to Sunday)
        int daysBefore = ((int)firstOfMonth.DayOfWeek + 6) % 7; // Monday=0, Sunday=6
        int daysAfter = 7 - ((int)lastOfMonth.DayOfWeek + 6) % 7 - 1;

        DateTime startDate = firstOfMonth.AddDays(-daysBefore);
        DateTime endDate = lastOfMonth.AddDays(daysAfter);

        using var connection = new SqliteConnection(CONNECTION_STRING);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
        SELECT Id, DateOf, StartTime, EndTime, Title, Location
        FROM events
        WHERE DateOf BETWEEN @start AND @end";

        command.Parameters.AddWithValue("@start", startDate.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@end", endDate.ToString("yyyy-MM-dd"));

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var model = new EventModel
            {
                Id = reader.GetInt32(0),
                DateOf = DateTime.ParseExact(reader.GetString(1), "yyyy-MM-dd", CultureInfo.InvariantCulture),
                StartTime = TimeSpan.Parse(reader.GetString(2)),
                EndTime = reader.IsDBNull(3) ? TimeSpan.Zero : TimeSpan.Parse(reader.GetString(3)),
                Title = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                Location = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
            };

            events.Add(model);
        }

        return events;
    }

    public static async Task<IEnumerable<EventModel>> GetEventsByWeekAsync(DateTime anyDateInWeek)
    {
        var events = new List<EventModel>();

        // Normalize to week (Monday to Sunday)
        int daysBefore = ((int)anyDateInWeek.DayOfWeek + 6) % 7; // Monday = 0
        DateTime weekStart = anyDateInWeek.Date.AddDays(-daysBefore);
        DateTime weekEnd = weekStart.AddDays(6);

        using var connection = new SqliteConnection(CONNECTION_STRING);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
        SELECT Id, DateOf, StartTime, EndTime, Title, Description, Location
        FROM events
        WHERE DateOf BETWEEN @start AND @end
    ";
        command.Parameters.AddWithValue("@start", weekStart.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@end", weekEnd.ToString("yyyy-MM-dd"));

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var model = new EventModel
            {
                Id = reader.GetInt32(0),
                DateOf = DateTime.ParseExact(reader.GetString(1), "yyyy-MM-dd", CultureInfo.InvariantCulture),
                StartTime = TimeSpan.Parse(reader.GetString(2)),
                EndTime = reader.IsDBNull(3) ? TimeSpan.Zero : TimeSpan.Parse(reader.GetString(3)),
                Title = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                Location = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
            };

            events.Add(model);
        }

        return events;
    }

    public static async Task<int> InsertEventAsync(EventModel model)
    {
        using var connection = new SqliteConnection(CONNECTION_STRING);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
        INSERT INTO events (DateOf, StartTime, EndTime, Title, Description, Location)
        VALUES (@date, @start, @end, @title, @desc, @location);
        SELECT last_insert_rowid();
    ";

        command.Parameters.AddWithValue("@date", model.DateOf.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@start", model.StartTime.ToString(@"hh\:mm\:ss"));
        command.Parameters.AddWithValue("@end", model.EndTime.ToString(@"hh\:mm\:ss"));
        command.Parameters.AddWithValue("@title", model.Title ?? string.Empty);
        command.Parameters.AddWithValue("@desc", model.Description ?? string.Empty);
        command.Parameters.AddWithValue("@location", model.Location ?? string.Empty);

        object? result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }


    public static async Task<bool> UpdateEventAsync(EventModel model)
    {
        using var connection = new SqliteConnection(CONNECTION_STRING);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
        UPDATE events
        SET DateOf = @date,
            StartTime = @start,
            EndTime = @end,
            Title = @title,
            Description = @desc,
            Location = @location
        WHERE Id = @id
    ";

        command.Parameters.AddWithValue("@date", model.DateOf.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@start", model.StartTime.ToString(@"hh\:mm\:ss"));
        command.Parameters.AddWithValue("@end", model.EndTime.ToString(@"hh\:mm\:ss"));
        command.Parameters.AddWithValue("@title", model.Title);
        command.Parameters.AddWithValue("@desc", model.Description ?? string.Empty);
        command.Parameters.AddWithValue("@location", model.Location ?? string.Empty);
        command.Parameters.AddWithValue("@id", model.Id);

        int rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }
}