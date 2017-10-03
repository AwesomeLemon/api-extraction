namespace DownloadRepositories.UrlProviders {
    public interface IRepoUrlProvider {
        string GetNextUrl();
        void Dispose();
    }
}