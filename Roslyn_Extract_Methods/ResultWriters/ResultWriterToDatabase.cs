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
            List<Tuple<string, MethodCommentInfo, List<ApiCall>, List<MethodParameter>>> methodsTupleData,
            string slnPath, Solution curSolution) {
            var methodsInDatabase = new List<Method>();
            foreach (var data in methodsTupleData) {
                var methodName = data.Item1;
                var methodCommentInfo = data.Item2;
                var apiCallsString = string.Join(" ", data.Item3.Select(x => x.ToString()));
                var method = new Method(methodName, apiCallsString, methodCommentInfo);
                _sqLiteConnection.Insert(method);
                List<MethodParameter> methodParameters = data.Item4;
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