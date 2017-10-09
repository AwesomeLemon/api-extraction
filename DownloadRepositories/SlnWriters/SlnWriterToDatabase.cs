using System.Collections.Generic;
using System.Linq;
using Common.Database;
using SQLite;

namespace DownloadRepositories.SlnWriters {
    public class SlnWriterToDatabase {
        public void Write(IEnumerable<string> slns, int repoId) {
            _sqLiteConnection.InsertAll(slns.Select(slnPath => new Solution(repoId, slnPath)));
        }

        private readonly SQLiteConnection _sqLiteConnection;

        public SlnWriterToDatabase(SQLiteConnection sqLiteConnection) {
            _sqLiteConnection = sqLiteConnection;
            _sqLiteConnection.CreateTable<Solution>();
        }
    }
}