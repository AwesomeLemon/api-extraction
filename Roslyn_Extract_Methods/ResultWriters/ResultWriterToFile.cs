using System;
using System.Collections.Generic;
using System.IO;
using Roslyn_Extract_Methods.Database;

namespace Roslyn_Extract_Methods.ResultWriters {
    class ResultWriterToFile : IResultWriter {
        private readonly StreamWriter _extractedDataWriter; 
        
        public ResultWriterToFile(string resFilePath) {
            _extractedDataWriter = new StreamWriter(resFilePath, true);
            _extractedDataWriter.AutoFlush = true;
        }

        public void Write(Dictionary<string, Tuple<MethodCommentInfo, List<ApiCall>, List<MethodParameter>>> methodsCommentsCalls, string slnPath) {
            _extractedDataWriter.WriteLine("**" + slnPath);
            foreach (var keyValuePair in methodsCommentsCalls) {
                _extractedDataWriter.WriteLine("//" + keyValuePair.Key);
                _extractedDataWriter.WriteLine(keyValuePair.Value.Item1);
                keyValuePair.Value.Item2.ForEach(i => _extractedDataWriter.Write(i + " "));
                _extractedDataWriter.WriteLine();
            }
            _extractedDataWriter.Flush();
        }
    }
}