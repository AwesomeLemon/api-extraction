using System;
using System.Collections.Generic;
using System.IO;
using Common;
using Common.Database;

namespace Roslyn_Extract_Methods.ResultWriters {
    class ResultWriterToFile : IResultWriter {
        private readonly StreamWriter _extractedDataWriter; 
        
        public ResultWriterToFile(string resFilePath) {
            _extractedDataWriter = new StreamWriter(resFilePath, true);
            _extractedDataWriter.AutoFlush = true;
        }

        public void Write(List<Tuple<string, MethodCommentInfo, List<ApiCall>, List<MethodParameter>>> methodsTupleData,
            string slnPath) {
            _extractedDataWriter.WriteLine("**" + slnPath);
            foreach (var tuple in methodsTupleData) {
                _extractedDataWriter.WriteLine("//" + tuple.Item1);
                _extractedDataWriter.WriteLine(tuple.Item2.FullComment);
                tuple.Item3.ForEach(i => _extractedDataWriter.Write(i + " "));
                _extractedDataWriter.WriteLine();
                tuple.Item4.ForEach(i => _extractedDataWriter.Write(i + " "));
            }
            _extractedDataWriter.Flush();
        }
    }
}