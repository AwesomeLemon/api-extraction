using System;
using System.IO;

namespace DownloadRepositories.UrlProviders {
    public class RepoUrlProvider : IRepoUrlProvider {
        private readonly StreamReader _urlFileReader;

        public RepoUrlProvider(string FileWithUrls, int toSkipCount) {
            _urlFileReader = new StreamReader(FileWithUrls);
            int skippedCounter = 0;
            string curRepUrlClone, ownerAndNameStr, curRepUrlApi;
            while (skippedCounter++ < toSkipCount) {
                curRepUrlClone = _urlFileReader.ReadLine();
                ownerAndNameStr = _urlFileReader.ReadLine();
                curRepUrlApi = _urlFileReader.ReadLine();
                if (curRepUrlClone != null && curRepUrlApi != null) break;
            }
        }

        public Tuple<string, string> GetNextUrlAndOwner() {
            string curRepUrlClone, ownerAndNameStr, curRepUrlApi;
            curRepUrlClone = _urlFileReader.ReadLine();
            ownerAndNameStr = _urlFileReader.ReadLine();
            curRepUrlApi = _urlFileReader.ReadLine();
            return (curRepUrlClone == null || ownerAndNameStr == null) ? null 
                : new Tuple<string, string>(curRepUrlClone, ownerAndNameStr);
        }
    }
}