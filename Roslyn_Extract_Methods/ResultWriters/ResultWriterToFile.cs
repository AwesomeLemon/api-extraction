using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslyn_Extract_Methods.ResultWriters {
    class ResultWriterToFile : IResultWriter {
        private readonly StreamWriter _extractedDataWriter; 
        
        public ResultWriterToFile(string resFilePath) {
            _extractedDataWriter = new StreamWriter(resFilePath, true);
            _extractedDataWriter.AutoFlush = true;
        }

        public void Write(Dictionary<MethodDeclarationSyntax, Tuple<Tuple<string, string>, List<ApiCall>>> methodsCommentsCalls, string slnPath) {
            _extractedDataWriter.WriteLine("**" + slnPath);
            foreach (var keyValuePair in methodsCommentsCalls) {
                _extractedDataWriter.WriteLine("//" + keyValuePair.Key.Identifier);
                _extractedDataWriter.WriteLine(keyValuePair.Value.Item1);
                keyValuePair.Value.Item2.ForEach(i => _extractedDataWriter.Write(i + " "));
                _extractedDataWriter.WriteLine();
            }
            _extractedDataWriter.Flush();
        }
    }
}