using Common.Database;
using SQLite;

namespace DownloadRepositories.UrlProviders {
    public class RepoUrlProviderFromDatabase : IRepoUrlProvider{
        public string GetNextUrl() {
            return _sqLiteConnection.Get<Repo>(repo => repo.ProcessedTime == null).Url;
//            return _sqLiteConnection.Table<Repo>().Where(repo => repo.ProcessedTime == null).Take(10);
        }
        private readonly SQLiteConnection _sqLiteConnection;

        public RepoUrlProviderFromDatabase(string databaseName) {
            _sqLiteConnection = new SQLiteConnection(databaseName);
        }
    }
}