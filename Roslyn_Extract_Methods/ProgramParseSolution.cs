using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using Configuration = LibGit2Sharp.Configuration;

namespace Roslyn_Extract_Methods {
    class ProgramParseSolution {
        static void ExtractMethodsFromSolution(string solutionPath) {
            var workspace = MSBuildWorkspace.Create();
            var solution = workspace.OpenSolutionAsync(solutionPath).Result;
            var methodCollector = new MethodsCollector();
            foreach (var project in solution.Projects) {
                foreach (var document in project.Documents) {
                    var rootNode = document.GetSyntaxRootAsync().Result;
                    methodCollector.Visit(rootNode);
                    
                }
            }
            methodCollector.ExtractAPISequence();

            //foreach (var methodComment in methodCollector.MethodComments) {
            //    Console.WriteLine(methodComment.Key.Identifier + "\n\t" + methodComment.Value);
            //}
        }

        public static bool CheckIfSolutionBuilds(string solutionPath) {
            var workspace = MSBuildWorkspace.Create();
            try {
                var solution = workspace.OpenSolutionAsync(solutionPath).Result;
//                foreach (var project in solution.Projects) {
//                    foreach (var document in project.Documents) {
//                        var a = document.GetSemanticModelAsync().Result;
//
//                    }
//                }
            }
            catch (Exception e) {
                return false;
            }
            return true;
        }

        static void Main(string[] args) {
            //var solutionPathSeasons = @"D:\Users\Alexander\Documents\GitHub\DesktopSeasons\DesktopSeasons.sln";//
            //var solutionPathOctocat = @"D:\Users\Alexander\Documents\GitHub\octokitnet\Octokit.sln";
           // var solutionPathRoslyn = @"D:\Users\Alexander\Documents\GitHub\octokitnet\Octokit.sln";
            //ExtractMethodsFromSolution(solutionPathOctocat);

            //
            //     ProgramDownloadFromGit.CloneRepsFromFile();
            //ProgramDownloadFromGit.GetFileListOfRep("");
//            ProgramDownloadFromGit.FileRepsAndSaveUrls();
//            ProgramDownloadFromGit.SelectNeededUrls();
            //Console.WriteLine(ProgramDownloadFromGit.GetProjectNameAndOwner(@"https://api.github.com/repos/dotnet/coreclr"));
            ProgramDownloadFromGit.CloneRepsWithSlnFromFile(7);
            //ProgramDownloadFromGit.GetProjectNameAndOwner(@"https://api.github.com/repos/dotnet/coreclr");
            //var path = @"D:\Users\Alexander\Documents\GitHub\DesktopSeasons\DesktopSeasons\TaskScheduler.cs";

            //SyntaxTree tree;
            //using (var stream = File.OpenRead(path)) {
            //    tree = CSharpSyntaxTree.ParseText(SourceText.From(stream), path: path);
            //}
            //var root = (CompilationUnitSyntax)tree.GetRoot();

            //var collector = new MethodsCollector();
            //collector.Visit(root);

            //foreach (var method in collector.MethodDecls) {
            //    Console.WriteLine(method.Identifier);
            //    var trivia = method.GetLeadingTrivia();
            //    Console.WriteLine(trivia.ElementAt(1));
            //}
        }
    }
}
