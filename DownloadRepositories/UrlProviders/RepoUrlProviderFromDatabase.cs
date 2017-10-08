using System;
using Common;
using Common.Database;
using SQLite;

namespace DownloadRepositories.UrlProviders {
    public class RepoUrlProviderFromDatabase : IRepoUrlProvider {
        private Repo _curRepo = null;

        public string GetNextUrl() {
            if (_curRepo != null) {
                _curRepo.ProcessedTime = DateTime.Now;
                _sqLiteConnection.Update(_curRepo);
            }
            _curRepo = _sqLiteConnection.Get<Repo>(repo => repo.ProcessedTime == null);
            return _curRepo.Url;
        }

        public int GetCurRepoId() {
            return _curRepo.Id;
        }
        private readonly SQLiteConnection _sqLiteConnection;

        public RepoUrlProviderFromDatabase(string fileWithUrls, SQLiteConnection sqLiteConnection) {
            _sqLiteConnection = sqLiteConnection;
            var repoQuery = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Repo';";
            bool repoTableExists = _sqLiteConnection.ExecuteScalar<int>( repoQuery ) == 1;
            if (!repoTableExists) {
                new RepoIntoDatabaseInserter(_sqLiteConnection).InsertReposIntoDatabaseFromFile(fileWithUrls);
            }
            
        }
        
        public void Dispose() {
            _sqLiteConnection.Dispose();
        }

    }
}