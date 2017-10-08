using System;
using System.IO;
using Common.Database;
using SQLite;

namespace Common {
    public class RepoIntoDatabaseInserter {
        private  SQLiteConnection _sqLiteConnection;

        public RepoIntoDatabaseInserter(string databasePath) {
            _sqLiteConnection = new SQLiteConnection(databasePath);
            _sqLiteConnection.CreateTable<Repo>();
        }

        public RepoIntoDatabaseInserter(SQLiteConnection connection) {
            _sqLiteConnection = connection;
            _sqLiteConnection.CreateTable<Repo>();
        }

        public void InsertReposIntoDatabaseFromFile(string filePath) {
            int i = 0;
            _sqLiteConnection.RunInTransaction(() => {
                using (StreamReader sr = new StreamReader(filePath)) {
                    do {
                        var curRepo = GetNextRepoFromFile(sr);
                        if (curRepo == null) break;
                        _sqLiteConnection.Insert(curRepo);
                        Console.WriteLine(++i);
                    } while (sr.ReadLine() != null);
                }
            });
            _sqLiteConnection.Close();
        }

        private static Repo GetNextRepoFromFile(StreamReader sr) {
            var cloneUrl = sr.ReadLine();
            if (cloneUrl == null) return null;
            var fullName = sr.ReadLine();
            var url = sr.ReadLine();
            var stars = sr.ReadLine();
            var watchers = sr.ReadLine();
            var forks = sr.ReadLine();
            return new Repo(cloneUrl, Int32.Parse(stars), Int32.Parse(forks), Int32.Parse(watchers) );
        }

        /*public static void Main(string[] args) {
            new RepoIntoDatabaseInserter(@"D:\hubic\DeepApi#").InsertReposIntoDatabaseFromFile(_fileWithUrls);
        }*/
    }
}