using System;
using Common.Database;
using SQLite;

namespace DownloadRepositories.UrlProviders {
    public class RepoUrlProviderFromDatabase : IRepoUrlProvider {
        private Repo _prevRepo = null;
        public string GetNextUrl() {
            if (_prevRepo != null) {
                _prevRepo.ProcessedTime = DateTime.Now;
                _sqLiteConnection.Update(_prevRepo);
            }
            var curRepo = _sqLiteConnection.Get<Repo>(repo => repo.ProcessedTime == null);
            _prevRepo = curRepo;
            return curRepo.Url;
//            return _sqLiteConnection.Table<Repo>().Where(repo => repo.ProcessedTime == null).Take(10);
        }
        private readonly SQLiteConnection _sqLiteConnection;

        public RepoUrlProviderFromDatabase(string databaseName) {
            _sqLiteConnection = new SQLiteConnection(databaseName);
        }
        
        public void Dispose() {
            _sqLiteConnection.Dispose();
        }

    }
}