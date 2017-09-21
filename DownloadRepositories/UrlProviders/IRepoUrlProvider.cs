using System;

namespace DownloadRepositories.UrlProviders {
    public interface IRepoUrlProvider {
        Tuple<string, string> GetNextUrlAndOwner();
    }
}