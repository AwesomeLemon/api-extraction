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
        
        public void Write(Dictionary<string, Tuple<MethodCommentInfo, List<ApiCall>>> methodsCommentsCalls, string slnPath) {
            foreach (var keyValuePair in methodsCommentsCalls) {
                var methodName = keyValuePair.Key;
                var methodCommentInfo = keyValuePair.Value.Item1;
                var apiCallsString = string.Join(" ", keyValuePair.Value.Item2.Select(x => x.ToString()));
                _sqLiteConnection.Insert(new Method(methodName, apiCallsString, methodCommentInfo));
                
            }
        }
    }
}