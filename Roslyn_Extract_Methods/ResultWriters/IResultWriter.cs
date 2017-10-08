using System;
using System.Collections.Generic;
using Common;
using Common.Database;

namespace Roslyn_Extract_Methods.ResultWriters {
    interface IResultWriter {
        void Write(List<Tuple<string, MethodCommentInfo, List<ApiCall>, List<MethodParameter>>> methodsTupleData, string slnPath);
    }
}