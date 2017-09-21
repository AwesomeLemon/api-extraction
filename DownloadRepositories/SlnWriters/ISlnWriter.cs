using System.Collections.Generic;

namespace DownloadRepositories.SlnWriters {
    public interface ISlnWriter {
        void Write(IEnumerable<string> slns);
    }
}