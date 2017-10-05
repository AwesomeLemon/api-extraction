using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Database;
using SQLite;

namespace Roslyn_Extract_Methods.ResultWriters {
    class ResultWriterToDatabase {
        private readonly SQLiteConnection _sqLiteConnection;

        public ResultWriterToDatabase(SQLiteConnection sqLiteConnection) {
            _sqLiteConnection = sqLiteConnection;
            _sqLiteConnection.CreateTable<Method>();
            _sqLiteConnection.CreateTable<MethodParameter>();
        }

        public void Write(
            Dictionary<string, Tuple<MethodCommentInfo, List<ApiCall>, List<MethodParameter>>> methodsCommentsCalls,
            string slnPath, Solution curSolution) {
            var methodsInDatabase = new List<Method>();
            foreach (var keyValuePair in methodsCommentsCalls) {
                var methodName = keyValuePair.Key;
                var methodCommentInfo = keyValuePair.Value.Item1;
                var apiCallsString = string.Join(" ", keyValuePair.Value.Item2.Select(x => x.ToString()));
                var method = new Method(methodName, apiCallsString, methodCommentInfo);
                _sqLiteConnection.Insert(method);
                List<MethodParameter> methodParameters = keyValuePair.Value.Item3;
                foreach (var methodParameter in methodParameters) {
                    methodParameter.MethodId = method.Id;
                }
                _sqLiteConnection.InsertAll(methodParameters);
            }
            curSolution.Methods = methodsInDatabase;
            _sqLiteConnection.Update(curSolution);
        }
    }
}