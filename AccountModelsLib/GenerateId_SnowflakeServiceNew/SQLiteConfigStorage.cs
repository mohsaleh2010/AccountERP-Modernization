using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace AccountModelsLib
{
    public static class SQLiteConfigStorage
    {
        private const string DbFileName = "appconfig.db";
        private const string EncryptionPassword = "CHANGE_ME";

        private static string GetDbPath()
        {
            return Path.Combine(AppContext.BaseDirectory, DbFileName);
        }

        private static string GetConnectionString()
        {
            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = GetDbPath(),
                Mode = SqliteOpenMode.ReadWriteCreate,
                Password = EncryptionPassword
            };
            return builder.ConnectionString;
        }

        public static void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(GetConnectionString()))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS AppSettings (
                        Id INTEGER PRIMARY KEY,
                        BranchId INTEGER,
                        Epoch TEXT,
                        TimestampBits INTEGER,
                        GeneratorIdBits INTEGER,
                        SequenceBits INTEGER,
                        ConnectionString TEXT,
                        Theme TEXT
                    );";
                command.ExecuteNonQuery();
            }
        }

        public static SnowflakeSettings LoadSettings()
        {
            try
            {
                InitializeDatabase();

                using (var connection = new SqliteConnection(GetConnectionString()))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT BranchId, Epoch, TimestampBits, GeneratorIdBits, SequenceBits, ConnectionString, Theme FROM AppSettings WHERE Id = 1 LIMIT 1;";

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new SnowflakeSettings
                            {
                                BranchId = reader.GetInt32(0),
                                Epoch = DateTime.Parse(reader.GetString(1)),
                                TimestampBits = reader.GetInt32(2),
                                GeneratorIdBits = reader.GetInt32(3),
                                SequenceBits = reader.GetInt32(4),
                                ConnectionString = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                Theme = reader.IsDBNull(6) ? "Dark" : reader.GetString(6)
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading SQLite settings: {ex.Message}");
            }

            var defaultSettings = new SnowflakeSettings();
            SaveSettings(defaultSettings);
            return defaultSettings;
        }

        public static void SaveSettings(SnowflakeSettings settings)
        {
            try
            {
                InitializeDatabase();

                using (var connection = new SqliteConnection(GetConnectionString()))
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    command.CommandText = @"
                        INSERT INTO AppSettings (Id, BranchId, Epoch, TimestampBits, GeneratorIdBits, SequenceBits, ConnectionString, Theme)
                        VALUES (1, $branchId, $epoch, $timestampBits, $generatorIdBits, $sequenceBits, $connStr, $theme)
                        ON CONFLICT(Id) DO UPDATE SET
                            BranchId = excluded.BranchId,
                            Epoch = excluded.Epoch,
                            TimestampBits = excluded.TimestampBits,
                            GeneratorIdBits = excluded.GeneratorIdBits,
                            SequenceBits = excluded.SequenceBits,
                            ConnectionString = excluded.ConnectionString,
                            Theme = excluded.Theme;";

                    command.Parameters.AddWithValue("$branchId", settings.BranchId);
                    command.Parameters.AddWithValue("$epoch", settings.Epoch.ToString("o"));
                    command.Parameters.AddWithValue("$timestampBits", settings.TimestampBits);
                    command.Parameters.AddWithValue("$generatorIdBits", settings.GeneratorIdBits);
                    command.Parameters.AddWithValue("$sequenceBits", settings.SequenceBits);
                    command.Parameters.AddWithValue("$connStr", settings.ConnectionString ?? "");
                    command.Parameters.AddWithValue("$theme", settings.Theme ?? "Dark");

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving SQLite settings: {ex.Message}");
            }
        }
    }
}