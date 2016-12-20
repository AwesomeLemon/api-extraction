using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Win32;

namespace Roslyn_Extract_Methods {
    internal class MethodsCollector : CSharpSyntaxWalker {
        public readonly Dictionary<MethodDeclarationSyntax, string> MethodComments =
            new Dictionary<MethodDeclarationSyntax, string>();

        public List<MethodDeclarationSyntax> MethodDecls { get; } = new List<MethodDeclarationSyntax>();
        public override void VisitMethodDeclaration(MethodDeclarationSyntax node) {
            var syntaxTokenList = node.Modifiers;
            if (!syntaxTokenList.Select(t => t.Text.Equals("public")).Any()) return;
            if (node.HasLeadingTrivia && node.HasStructuredTrivia && (node.Body != null)) MethodDecls.Add(node);
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
                    continue;
                }

                var summary = xmlTrivia.ChildNodes()
                    .OfType<XmlElementSyntax>()
                    .FirstOrDefault(i => i.StartTag.Name.ToString().Equals("summary"));
                if (summary == null) {
                    toRemove.Add(method);
                    continue;
                }
                var stringComment = summary.Content.ToString();
                stringComment = Regex.Replace(stringComment, @"\s+", " ", RegexOptions.Multiline).Replace("///", "").Trim();
                stringComment = removeStuffWithinSuchBrackets(stringComment, '(', ')');
                stringComment = removeStuffWithinSuchBrackets(stringComment, '<', '>');
                stringComment = removeStuffWithinSuchBrackets(stringComment, '[', ']');
                if (stringComment.Length < 3) {
                    toRemove.Add(method);
                    continue;
                }
                MethodComments[method] = stringComment;
            }
            foreach (var methodDeclarationSyntax in toRemove) {
                MethodDecls.Remove(methodDeclarationSyntax);
            }
        }
        
        private string removeStuffWithinSuchBrackets(string input, char leftChar , char rightChar) {
            int lbracket = input.IndexOf(leftChar);
            while (lbracket != -1) {
                var rbracket = input.LastIndexOf(rightChar);
                if (rbracket != -1 && lbracket < rbracket) {
                    if (leftChar.Equals('<') && input.Substring(lbracket, rbracket - lbracket + 1).Contains("see cref")) {
                        var substring = input.Substring(lbracket, rbracket - lbracket + 1);
                        var startIndex = substring.IndexOf("\"", StringComparison.Ordinal);
                        var lastIndexOf = substring.LastIndexOf("\"", StringComparison.Ordinal);
                        var length = lastIndexOf - startIndex - 1;
                        if (length < 1) input = input.Remove(lbracket, rbracket - lbracket + 1);
                        else input = input.Replace(substring, substring.Substring(startIndex + 1, length));
                    }
                    else input = input.Remove(lbracket, rbracket - lbracket + 1);
                }
                else break;
                lbracket = input.IndexOf(leftChar);
            }
            return input;
        }
    }
}