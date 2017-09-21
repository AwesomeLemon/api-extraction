using System.Collections.Generic;
using System.IO;

namespace DownloadRepositories.SlnWriters {
    public class SlnWriterToFile : ISlnWriter {
        private readonly StreamWriter _slnFileWriter; 
        
        public SlnWriterToFile(string slnFilePath) {
            var slnFile = File.Open(slnFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            _slnFileWriter = new StreamWriter(slnFile);
        }

        public void Write(IEnumerable<string> slns) {
            foreach (var slnPath in slns) {
                _slnFileWriter.WriteLine(slnPath);
            }
        }
    }
}