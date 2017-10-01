using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Roslyn_Extract_Methods.SlnProviders {
    public interface ISlnProvider : IEnumerator<string> {
    }
}