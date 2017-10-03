using SQLite;

namespace DownloadRepositories.UrlProviders {
    public class RepoUrlProviderFromDatabase : IRepoUrlProvider{
        public string GetNextUrl() {
            throw new System.NotImplementedException();
        }
        private readonly SQLiteConnection _sqLiteConnection;

        public RepoUrlProviderFromDatabase(string databaseName) {
            _sqLiteConnection = new SQLiteConnection(databaseName);
        }
    }
}