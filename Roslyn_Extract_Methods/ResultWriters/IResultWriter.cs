using System;
using System.Collections.Generic;
using Common;
using Common.Database;

namespace Roslyn_Extract_Methods.ResultWriters {
    interface IResultWriter {
        void Write(Dictionary<string, Tuple<MethodCommentInfo, List<ApiCall>, List<MethodParameter>>> methodsCommentsCalls, string slnPath);
    }
}