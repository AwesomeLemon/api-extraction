using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslyn_Extract_Methods.ResultWriters {
    interface IResultWriter {
        void Write(Dictionary<string, Tuple<MethodCommentInfo, List<ApiCall>>> methodsCommentsCalls, string slnPath);
    }
}