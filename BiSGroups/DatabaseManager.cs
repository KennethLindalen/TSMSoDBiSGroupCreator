using System.Data.SQLite;

namespace BiSGroups;

public class DatabaseManager
{
    private readonly string _connectionString;

    public DatabaseManager(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void CreateTableIfNotExists()
    {
        string sql =
            "CREATE TABLE IF NOT EXISTS items (Id INTEGER PRIMARY KEY AUTOINCREMENT, itemId TEXT, isBoP INTEGER)";

        using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }

    public void WriteToDatabase(string itemId, bool isBoP)
    {
        string sql = "INSERT INTO items (itemId, isBoP) VALUES (@ItemId, @IsBoP)";

        using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@ItemId", itemId);
                command.Parameters.AddWithValue("@IsBoP", isBoP ? 1 : 0);

                command.ExecuteNonQuery();
            }
        }
    }

    public Dictionary<string, string> CheckEntryInDatabase(string itemId)
    {
        string sql = "SELECT Id, itemId, isBoP FROM items WHERE itemId = @ItemId";

        using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@ItemId", itemId);

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Dictionary<string, string> entryData = new Dictionary<string, string>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string columnName = reader.GetName(i);
                            string columnValue = reader.GetValue(i).ToString()!;
                            entryData.Add(columnName, columnValue);
                        }

                        return entryData;
                    }

                    return null;
                }
            }
        }
    }
    public async Task<bool> CheckIfItemIdExists(string itemId)
    {
        string sql = "SELECT COUNT(*) FROM items WHERE itemId = @ItemId";

        using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@ItemId", itemId);

                // ExecuteScalar returns the first column of the first row in the result set
                // which is the count of records with the itemId
                int count = Convert.ToInt32(command.ExecuteScalar());

                // If count is greater than 0, itemId exists in the database
                return count > 0;
            }
        }
    }
}