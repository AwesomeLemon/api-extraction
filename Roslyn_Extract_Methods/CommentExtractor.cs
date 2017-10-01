using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslyn_Extract_Methods {
    static class CommentExtractor {
        public static Dictionary<MethodDeclarationSyntax, Tuple<string, string, bool>> ExtractSummaryComments(
            List<MethodDeclarationSyntax> methodDeclarations) {
            var methodComments =
                new Dictionary<MethodDeclarationSyntax, Tuple<string, string, bool>>();

            foreach (var method in methodDeclarations) {
                var xmlTrivia = method.GetLeadingTrivia()
                    .Select(i => i.GetStructure())
                    .OfType<DocumentationCommentTriviaSyntax>()
                    .FirstOrDefault();
                if (xmlTrivia == null) {
                    var fullString = method.GetLeadingTrivia().ToFullString();
                    if (!fullString.Contains("//")) continue;
                    fullString = CleanString(fullString).Replace("//", "");
                    methodComments[method] = new Tuple<string, string, bool>(fullString, null, false);
                    continue;
                }
                var fullComment = xmlTrivia.Content.ToString();
                if (fullComment.StartsWith(" <summary>\r\n\t\t/// Clean up any resources being used.")
                    || fullComment.StartsWith(
                        " <summary>\r\n\t\t/// Required method for Designer support - do not modify")) {
                    continue;
                }
                var summary = xmlTrivia.ChildNodes()
                    .OfType<XmlElementSyntax>()
                    .FirstOrDefault(i => i.StartTag.Name.ToString().Equals("summary"));
                if (summary == null) {
                    methodComments[method] = new Tuple<string, string, bool>(fullComment, null, true);
                    continue;
                }

                var stringComment = summary.Content.ToString();
                stringComment = CleanString(stringComment);
                if (stringComment.Length < 3) {
                    continue;
                }
                methodComments[method] = new Tuple<string, string, bool>(fullComment, stringComment, true);
            }
            return methodComments;
        }

        private static string CleanString(string stringComment) {
            stringComment = Regex.Replace(stringComment, @"\s+", " ", RegexOptions.Multiline).Replace("///", "")
                .Trim();
            stringComment = CleanUpTags(stringComment);
            stringComment = RemoveStuffWithinSuchBrackets(stringComment, '(', ')');
            stringComment = RemoveStuffWithinSuchBrackets(stringComment, '[', ']');
            stringComment = RemoveStuffWithinSuchBrackets(stringComment, '{', '}');
            return stringComment;
        }

        public static string CleanUpTags(string input) {
            int start = input.IndexOf('<');
            while (start != -1) {
                int end = input.IndexOf('>');
                if (end != -1 && start < end) {
                    var tag = input.Substring(start, end - start + 1);
                    if (tag.Contains("see cref")) {
                        var startIndex = tag.IndexOf("cref=\"", StringComparison.Ordinal) + 6;
                        var lastIndexOf = tag.IndexOf("\"", startIndex, StringComparison.Ordinal);
                        if (lastIndexOf < startIndex) break;
                        input = input.Replace(tag, tag.Substring(startIndex, lastIndexOf - startIndex));
                    }
                    else input = input.Remove(start, end - start + 1);
                }
                else break;

                start = input.IndexOf('<');
            }
            return input;
        }

        private static string RemoveStuffWithinSuchBrackets(string input, char leftChar, char rightChar) {
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