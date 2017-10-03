using System.IO;

namespace DownloadRepositories.UrlProviders {
    public class RepoUrlProviderFromFile : IRepoUrlProvider {
        private readonly StreamReader _urlFileReader;

        public RepoUrlProviderFromFile(string FileWithUrls, int toSkipCount, string fileProcessedRepsCount) {
            _fileProcessedRepsCount = fileProcessedRepsCount;
            _toSkipNum = GetAlreadyProcessedNum();
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
            var curRepUrlClone = _urlFileReader.ReadLine();
            var ownerAndNameStr = _urlFileReader.ReadLine();
            var curRepUrlApi = _urlFileReader.ReadLine();
            return curRepUrlClone;
        }
        
        private int _toSkipNum;
        private readonly string _fileProcessedRepsCount;

        private int GetAlreadyProcessedNum() {
            if (!File.Exists(_fileProcessedRepsCount)) File.Create(_fileProcessedRepsCount).Close();
            using (var sr = new StreamReader(_fileProcessedRepsCount)) {
                _toSkipNum = int.Parse(sr.ReadLine() ?? "0");
            }
            return _toSkipNum;
        }
        
        public void Dispose() {
            using (var processedRepsFile = new StreamWriter(_fileProcessedRepsCount)) {
                processedRepsFile.WriteLine(++_toSkipNum);
            }
        }
    }
}