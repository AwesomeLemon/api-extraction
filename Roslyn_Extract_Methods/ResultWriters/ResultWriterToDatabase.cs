using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Database;
using SQLite;

namespace Roslyn_Extract_Methods.ResultWriters {
    class ResultWriterToDatabase {
        private readonly SQLiteAsyncConnection _sqLiteConnection;

        public ResultWriterToDatabase(SQLiteAsyncConnection sqLiteConnection) {
            _sqLiteConnection = sqLiteConnection;
            _sqLiteConnection.CreateTableAsync<Method>();
            _sqLiteConnection.CreateTableAsync<MethodParameter>();
        }

        public void Write(
            List<Tuple<string, MethodCommentInfo, List<ApiCall>, List<MethodParameter>>> methodsTupleData,
            string slnPath, Solution curSolution) {
            var methodsInDatabase = new List<Method>();
            _sqLiteConnection.RunInTransactionAsync(transaction => {
                foreach (var data in methodsTupleData) {
                    var methodName = data.Item1;
                    var methodCommentInfo = data.Item2;
                    var apiCallsString = string.Join(" ", data.Item3.Select(x => x.ToString()));
                    var method = new Method(methodName, apiCallsString, methodCommentInfo);

                    transaction.Insert(method);
                    methodsInDatabase.Add(method);
                    List<MethodParameter> methodParameters = data.Item4;
                    foreach (var methodParameter in methodParameters) {
                        methodParameter.MethodId = method.Id;
                    }
                    transaction.InsertAll(methodParameters);
                }
            }).ContinueWith(t => {
                Console.WriteLine("done!");
            });
            curSolution.Methods = methodsInDatabase;
            curSolution.ProcessedTime = DateTime.Now;
            _sqLiteConnection.UpdateAsync(curSolution);
        }
    }
}