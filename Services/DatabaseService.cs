using PdfFormOverlay.Maui.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfFormOverlay.Maui.Services
{
    // Database Service
    public class DatabaseService
    {
        private const string DatabasePassword = "Soldiers2024SecureDB!@#";
        private readonly string _databasePath;
        private SQLiteAsyncConnection _database;

        public DatabaseService()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var databasesFolder = Path.Combine(documentsPath, "databases");
            Directory.CreateDirectory(databasesFolder);

            _databasePath = Path.Combine(databasesFolder, "soldiers.db");
        }

        public async Task<SQLiteAsyncConnection> GetDatabaseAsync()
        {
            if (_database != null)
                return _database;

            var connectionString = new SQLiteConnectionString(_databasePath, true, key: DatabasePassword);
            _database = new SQLiteAsyncConnection(connectionString);

            await InitializeDatabaseAsync();
            return _database;
        }

        private async Task InitializeDatabaseAsync()
        {
            try
            {
                await _database.CreateTableAsync<FormDataRecord>();
                await _database.CreateTableAsync<UserSecurityRecord>();
                await _database.CreateTableAsync<AppSettingsRecord>();

                // Create indexes
                await _database.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_FormData_FormId ON FormData (FormId)");
                await _database.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_FormData_SavedDate ON FormData (SavedDate)");

                // Create trigger for LastModified
                await _database.ExecuteAsync(@"
                CREATE TRIGGER IF NOT EXISTS UpdateLastModified 
                AFTER UPDATE ON FormData 
                BEGIN 
                    UPDATE FormData SET LastModified = datetime('now') WHERE Id = NEW.Id; 
                END");

                System.Diagnostics.Debug.WriteLine($"Database initialized at: {_databasePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
                throw;
            }
        }

        public async Task CloseAsync()
        {
            if (_database != null)
            {
                await _database.CloseAsync();
                _database = null;
            }
        }
    }
}
