using System;
using System.IO;
using SQLite;

namespace Common.Database {
    public class DatabaseWrapper {
        private  SQLiteConnection _sqLiteConnection;

        public DatabaseWrapper(string databasePath) {
            _sqLiteConnection = new SQLiteConnection(databasePath);
            _sqLiteConnection.CreateTable<Repo>();
        }

        public void InsertReposIntoDatabaseFromFile(string filePath) {
            int i = 0;
            using (StreamReader sr = new StreamReader(filePath)) {
                do {
                    var curRepo = GetNextRepoFromFile(sr);
                    _sqLiteConnection.Insert(curRepo);
                    Console.WriteLine(i++);
                } while (sr.ReadLine() != null);
            }
        }

        private static Repo GetNextRepoFromFile(StreamReader sr) {
            var cloneUrl = sr.ReadLine();
            var fullName = sr.ReadLine();
            var url = sr.ReadLine();
            var stars = sr.ReadLine();
            var watchers = sr.ReadLine();
            var forks = sr.ReadLine();
            return new Repo(cloneUrl, Int32.Parse(stars), Int32.Parse(forks), Int32.Parse(watchers) );
        }
    }
}