using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslyn_Extract_Methods {
    class MethodsCollector : CSharpSyntaxWalker {
        public List<MethodDeclarationSyntax> MethodDecls { get; } = new List<MethodDeclarationSyntax>();

        private readonly Dictionary<MethodDeclarationSyntax, string> _methodComments =
            new Dictionary<MethodDeclarationSyntax, string>();
        public Dictionary<MethodDeclarationSyntax, string> MethodComments {
            get {
                if (_methodComments.Count == 0) ExtractSummaryComments();
                return _methodComments;
            }
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node) {
            if (node.HasLeadingTrivia && node.HasStructuredTrivia) MethodDecls.Add(node);
        }

        private void ExtractSummaryComments() {
            foreach (var method in MethodDecls) {
                var xmlTrivia = method.GetLeadingTrivia()
                    .Select(i => i.GetStructure())
                    .OfType<DocumentationCommentTriviaSyntax>()
                    .FirstOrDefault();
                if (xmlTrivia == null) return;
                
                var summary = xmlTrivia.ChildNodes()
                    .OfType<XmlElementSyntax>()
                    .First(i => i.StartTag.Name.ToString().Equals("summary"));
                int nodecnt = summary.Content.Count;
                Console.WriteLine(nodecnt);
                _methodComments[method] = summary.Content.ToFullString();
            }
        }

       public void ExtractAPISequence() {
           int cnt = 0;
            foreach (var method in MethodDecls) {
                if (method.Body == null) continue;
                foreach (var statement in method.Body.Statements) {
                    Console.WriteLine();
                    Console.WriteLine(cnt++);
                    Console.WriteLine(statement.ToString());
                }
            }
        }

        //public override void VisitUsingDirective(UsingDirectiveSyntax node) {
        //    if (node.Name.ToString() != "System" &&
        //        !node.Name.ToString().StartsWith("System.")) {
        //        this.MethodDecls.Add(node);
        //    }
        //}
    }
}
