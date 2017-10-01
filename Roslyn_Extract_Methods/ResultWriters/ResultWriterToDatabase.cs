using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn_Extract_Methods.Database;
using SQLite;

namespace Roslyn_Extract_Methods.ResultWriters {
    class ResultWriterToDatabase : IResultWriter {
        private readonly SQLiteConnection _sqLiteConnection;

        public ResultWriterToDatabase(string databaseName) {
            _sqLiteConnection = new SQLiteConnection(databaseName);
            _sqLiteConnection.CreateTable<Method>();
        }
        
        public void Write(Dictionary<string, Tuple<Tuple<string, string, bool>, List<ApiCall>>> methodsCommentsCalls, string slnPath) {
            foreach (var keyValuePair in methodsCommentsCalls) {
                var methodName = keyValuePair.Key;
                var fullComment = keyValuePair.Value.Item1.Item1;
                var summaryFirstSentence = keyValuePair.Value.Item1.Item2;
                var isXml = keyValuePair.Value.Item1.Item3;
                var apiCallsString = string.Join(" ", keyValuePair.Value.Item2.Select(x => x.ToString()));
                _sqLiteConnection.Insert(new Method(methodName, fullComment, summaryFirstSentence, apiCallsString,
                    isXml));
                
            }
        }
    }
}