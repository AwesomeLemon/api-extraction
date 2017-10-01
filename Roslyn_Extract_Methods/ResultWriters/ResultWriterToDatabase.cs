﻿using System;
using System.Collections.Generic;
using System.Linq;
using Roslyn_Extract_Methods.Database;
using SQLite;

namespace Roslyn_Extract_Methods.ResultWriters {
    class ResultWriterToDatabase : IResultWriter {
        private readonly SQLiteConnection _sqLiteConnection;

        public ResultWriterToDatabase(string databaseName) {
            _sqLiteConnection = new SQLiteConnection(databaseName);
            _sqLiteConnection.CreateTable<Method>();
            _sqLiteConnection.CreateTable<MethodParameter>();
        }
        
        public void Write(Dictionary<string, Tuple<MethodCommentInfo, List<ApiCall>, List<MethodParameter>>> methodsCommentsCalls, string slnPath) {
            foreach (var keyValuePair in methodsCommentsCalls) {
                var methodName = keyValuePair.Key;
                var methodCommentInfo = keyValuePair.Value.Item1;
                var apiCallsString = string.Join(" ", keyValuePair.Value.Item2.Select(x => x.ToString()));
                var method = new Method(methodName, apiCallsString, methodCommentInfo);
                _sqLiteConnection.Insert(method);
                List<MethodParameter> methodParameters = keyValuePair.Value.Item3;
                foreach (var methodParameter in methodParameters) {
                    methodParameter.MethodId = method.Id;
                    _sqLiteConnection.Insert(methodParameter);
                }

            }
        }
    }
}