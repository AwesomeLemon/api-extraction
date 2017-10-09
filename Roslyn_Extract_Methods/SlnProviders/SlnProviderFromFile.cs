using System.Collections;
using System.IO;

namespace Roslyn_Extract_Methods.SlnProviders {
    class SlnProviderFromFile : ISlnProvider {
        private readonly StreamReader _slnFileReader;
        private readonly string _fileProcessedSlnsCount;
        private int _processedNum;

        public SlnProviderFromFile(string slnPath, string fileProcessedSlnsCount) {
            _fileProcessedSlnsCount = fileProcessedSlnsCount;
            var slnFile = File.Open(slnPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _slnFileReader = new StreamReader(slnFile);
            int skippedCnt = 0;
            _processedNum = GetAlreadyProcessedNum();
            while (skippedCnt++ < _processedNum) {
                Current = _slnFileReader.ReadLine(); //not 'MoveNext', 'cause the processed counter is updated there
            }
        }

        private int GetAlreadyProcessedNum() {
            int processedNum;
            if (!File.Exists(_fileProcessedSlnsCount)) File.Create(_fileProcessedSlnsCount).Close();
            using (var sr = new StreamReader(_fileProcessedSlnsCount)) {
                processedNum = int.Parse(sr.ReadLine() ?? "0");
            }
            return processedNum;
        }

        public void Dispose() {
            _slnFileReader.Dispose();
        }

        public bool MoveNext() {
            Current = _slnFileReader.ReadLine();
            if (Current == null) return false;
            using (var sw = new StreamWriter(_fileProcessedSlnsCount)) {
//                sw.WriteLine(++_processedNum);
            }
            return true;
        }

        public void Reset() {
            throw new System.NotSupportedException();
        }

        object IEnumerator.Current => Current;

        public string Current { get; set; }
    }
}