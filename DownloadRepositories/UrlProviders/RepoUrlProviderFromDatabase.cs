using System;
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

        public RepoUrlProviderFromDatabase( SQLiteConnection sqLiteConnection) {
            _sqLiteConnection = sqLiteConnection;
        }
        
        public void Dispose() {
            _sqLiteConnection.Dispose();
        }

    }
}