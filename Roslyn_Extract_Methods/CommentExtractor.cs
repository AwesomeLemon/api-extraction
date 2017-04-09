using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslyn_Extract_Methods {
    class CommentExtractor {
        public static Dictionary<MethodDeclarationSyntax, string> ExtractSummaryComments(
            List<MethodDeclarationSyntax> methodDeclarations) {
            Dictionary<MethodDeclarationSyntax, string> methodComments = new Dictionary<MethodDeclarationSyntax, string>();
            
            foreach (var method in methodDeclarations) {
                var xmlTrivia = method.GetLeadingTrivia()
                    .Select(i => i.GetStructure())
                    .OfType<DocumentationCommentTriviaSyntax>()
                    .FirstOrDefault();
                if (xmlTrivia == null) continue;

                var summary = xmlTrivia.ChildNodes()
                    .OfType<XmlElementSyntax>()
                    .FirstOrDefault(i => i.StartTag.Name.ToString().Equals("summary"));
                if (summary == null) continue;

                var stringComment = summary.Content.ToString();
                stringComment = Regex.Replace(stringComment, @"\s+", " ", RegexOptions.Multiline).Replace("///", "").Trim();
                stringComment = CleanUpTags(stringComment);
                stringComment = removeStuffWithinSuchBrackets(stringComment, '(', ')');
                stringComment = removeStuffWithinSuchBrackets(stringComment, '[', ']');
                stringComment = removeStuffWithinSuchBrackets(stringComment, '{', '}');
                //                var sb = new StringBuilder();
                //                foreach (var xmlNodeSyntax in summary.Content) {
                //                    if (xmlNodeSyntax is XmlElementSyntax) {
                //                        var elementSyntax = (XmlElementSyntax) xmlNodeSyntax;
                //                        var a = elementSyntax.Content;
                //                        
                //                    }
                //                }
                if (stringComment.Length < 3) {
                    
                    continue;
                }
                methodComments[method] = stringComment;
            }
            return methodComments;
        }

        public static string CleanUpTags(string input) {
            int start = input.IndexOf('<');
            //            string inputbackup = (string) input.Clone();
            while (start != -1) {
                int end = input.IndexOf('>');
                if (end != -1 && start < end) {
                    var tag = input.Substring(start, end - start + 1);
                    if (tag.Contains("see cref")) {
                        var startIndex = tag.IndexOf("cref=\"", StringComparison.Ordinal) + 6;
                        var lastIndexOf = tag.IndexOf("\"", startIndex, StringComparison.Ordinal);
                        if (lastIndexOf < startIndex) break;
                        input = input.Replace(tag, tag.Substring(startIndex, lastIndexOf - startIndex));
                    } else input = input.Remove(start, end - start + 1);
                } else break;

                start = input.IndexOf('<');
            }
            return input;
        }

        private static string removeStuffWithinSuchBrackets(string input, char leftChar, char rightChar) {
            int lbracket = input.IndexOf(leftChar);
            while (lbracket != -1) {
                var rbracket = input.LastIndexOf(rightChar);
                if (rbracket != -1 && lbracket < rbracket) {
                    input = input.Remove(lbracket, rbracket - lbracket + 1);
                } else break;

                lbracket = input.IndexOf(leftChar);
            }
            return input;
        }
    }
}
