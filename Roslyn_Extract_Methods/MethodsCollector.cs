using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Win32;
using Octokit;

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
                stringComment = CleanUpTags(stringComment);
                stringComment = removeStuffWithinSuchBrackets(stringComment, '(', ')');
                stringComment = removeStuffWithinSuchBrackets(stringComment, '[', ']');
                stringComment = removeStuffWithinSuchBrackets(stringComment, '{', '}');
                var sb = new StringBuilder();
                foreach (var xmlNodeSyntax in summary.Content) {
                    if (xmlNodeSyntax is XmlElementSyntax) {
                        var elementSyntax = (XmlElementSyntax) xmlNodeSyntax;
                        elementSyntax.Content;
                    }
                }
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

        public static string CleanUpTags(string input) {
            int start = input.IndexOf('<');
            string inputbackup = (string) input.Clone();
            while (start != -1) {
                int end = input.IndexOf('>');
                if (end != -1 && start < end) {
                    var tag = input.Substring(start, end - start + 1);
                    if (tag.Contains("see cref")) {
                        var startIndex = tag.IndexOf("cref=\"", StringComparison.Ordinal) + 6;
                        var lastIndexOf = tag.IndexOf("\"", startIndex, StringComparison.Ordinal);
                        input = input.Replace(tag, tag.Substring(startIndex, lastIndexOf - startIndex));
                    }
                    else input = input.Remove(start, end - start + 1);
                }
                else break;
                start = input.IndexOf('<');
            }
            if (input.Contains('<') || input.Contains('>')) {
                int a = 3;
            }
            return input;
        }
        
        private string removeStuffWithinSuchBrackets(string input, char leftChar , char rightChar) {
            int lbracket = input.IndexOf(leftChar);
            while (lbracket != -1) {
                var rbracket = input.LastIndexOf(rightChar);
                if (rbracket != -1 && lbracket < rbracket) {
                    input = input.Remove(lbracket, rbracket - lbracket + 1);
                }
                else break;
                lbracket = input.IndexOf(leftChar);
            }
            return input;
        }
    }
}