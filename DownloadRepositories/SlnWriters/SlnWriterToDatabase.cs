using System.Collections.Generic;
using Common.Database;
using SQLite;

namespace DownloadRepositories.SlnWriters {
    public class SlnWriterToDatabase : ISlnWriter{
        public void Write(IEnumerable<string> slns) {
            _sqLiteConnection.InsertAll(slns);
        }
        
        private readonly SQLiteConnection _sqLiteConnection;

        public SlnWriterToDatabase(string databaseName) {
            _sqLiteConnection = new SQLiteConnection(databaseName);
            _sqLiteConnection.CreateTable<Solution>();
        }
    }
}