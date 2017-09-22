using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslyn_Extract_Methods.ResultWriters {
    interface IResultWriter {
        void Write(Dictionary<MethodDeclarationSyntax, Tuple<Tuple<string, string>, List<ApiCall>>> methodsCommentsCalls, string slnPath);
    }
}