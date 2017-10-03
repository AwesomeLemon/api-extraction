using System.IO;

namespace DownloadRepositories.UrlProviders {
    public class RepoUrlProviderFromFile : IRepoUrlProvider {
        private readonly StreamReader _urlFileReader;

        public RepoUrlProviderFromFile(string FileWithUrls, int toSkipCount) {
            _urlFileReader = new StreamReader(FileWithUrls);
            int skippedCounter = 0;
            string curRepUrlClone, ownerAndNameStr, curRepUrlApi;
            while (skippedCounter++ < toSkipCount) {
                curRepUrlClone = _urlFileReader.ReadLine();
                ownerAndNameStr = _urlFileReader.ReadLine();
                curRepUrlApi = _urlFileReader.ReadLine();
                if (curRepUrlClone == null && curRepUrlApi == null) break;
            }
        }

        public string GetNextUrl() {
            string curRepUrlClone, ownerAndNameStr, curRepUrlApi;
            curRepUrlClone = _urlFileReader.ReadLine();
            ownerAndNameStr = _urlFileReader.ReadLine();
            curRepUrlApi = _urlFileReader.ReadLine();
            return curRepUrlClone;
        }
    }
}