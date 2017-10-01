using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslyn_Extract_Methods.ResultWriters {
    interface IResultWriter {
        void Write(Dictionary<string, Tuple<Tuple<string, string, bool>, List<ApiCall>>> methodsCommentsCalls, string slnPath);
    }
}