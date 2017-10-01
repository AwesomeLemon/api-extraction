using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslyn_Extract_Methods {
    internal class MethodsCollector : CSharpSyntaxWalker {
        public List<MethodDeclarationSyntax> MethodDecls { get; } = new List<MethodDeclarationSyntax>();
        public override void VisitMethodDeclaration(MethodDeclarationSyntax node) {
//            var syntaxTokenList = node.Modifiers;
//            if (!syntaxTokenList.Select(t => t.Text.Equals("public")).Any()) return;
            if (node.HasLeadingTrivia && (node.Body != null)) MethodDecls.Add(node);
        }
    }
}