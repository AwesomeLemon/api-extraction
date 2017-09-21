using System;

namespace DownloadRepositories.UrlProviders {
    public interface IRepoUrlProvider {
        string GetNextUrl();
    }
}