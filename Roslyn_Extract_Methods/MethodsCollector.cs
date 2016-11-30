using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslyn_Extract_Methods {
    internal class MethodsCollector : CSharpSyntaxWalker {
        public readonly Dictionary<MethodDeclarationSyntax, string> MethodComments =
            new Dictionary<MethodDeclarationSyntax, string>();

        public List<MethodDeclarationSyntax> MethodDecls { get; } = new List<MethodDeclarationSyntax>();

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node) {
            if (node.HasLeadingTrivia && node.HasStructuredTrivia) MethodDecls.Add(node);
        }

        public void ExtractSummaryComments() {
            var toRemove = new List<MethodDeclarationSyntax>();
            foreach (var method in MethodDecls) {
                var xmlTrivia = method.GetLeadingTrivia()
                    .Select(i => i.GetStructure())
                    .OfType<DocumentationCommentTriviaSyntax>()
                    .FirstOrDefault();
                if (xmlTrivia == null) {
                    toRemove.Add(method);
                    return;
                }

                var summary = xmlTrivia.ChildNodes()
                    .OfType<XmlElementSyntax>()
                    .First(i => i.StartTag.Name.ToString().Equals("summary"));
                MethodComments[method] = summary.Content.ToString().Replace("///", "").Trim();
            }
            foreach (var methodDeclarationSyntax in toRemove) {
                MethodDecls.Remove(methodDeclarationSyntax);
            }
        }
    }
}