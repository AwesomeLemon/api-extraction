using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace Roslyn_Extract_Methods {
    internal class ProgramParseSolution {
        public static List<ApiCall> ExtractApiSequence(MethodDeclarationSyntax method, SemanticModel model) {
            var extractor = new ApiSequenceExtractor(model);
            extractor.Visit(method);
            return extractor.Calls;
        }

        private static Dictionary<MethodDeclarationSyntax, Tuple<string, List<ApiCall>>> ExtractMethodsFromSolution(
            string solutionPath) {
            var workspace = MSBuildWorkspace.Create();
            var solution = workspace.OpenSolutionAsync(solutionPath).Result;

            var res = new Dictionary<MethodDeclarationSyntax, Tuple<string, List<ApiCall>>>();
            foreach (var project in solution.Projects) {
                foreach (var document in project.Documents) {
                    var rootNode = document.GetSyntaxRootAsync().Result;

                    var methodCollector = new MethodsCollector();
                    methodCollector.Visit(rootNode);
                    methodCollector.ExtractSummaryComments();
                    var curMethods = methodCollector.MethodDecls;

                    var model = document.GetSemanticModelAsync().Result;
                    try {
                        foreach (var method in curMethods) {
                            try {
                                res.Add(method,
                                    new Tuple<string, List<ApiCall>>(methodCollector.MethodComments[method],
                                        ExtractApiSequence(method, model)));
                            }
                            catch (KeyNotFoundException e) {
                                Console.WriteLine("Oops");
                                //return new Dictionary<MethodDeclarationSyntax, Tuple<string, List<ApiCall>>>();
                            }
                        }
                    }
                    catch (Exception e) {
                        Console.WriteLine("Ooops - outer");
                    }
                }
            }
            return res;
        }

        public static bool CheckIfSolutionBuilds(string solutionPath) {
            var workspace = MSBuildWorkspace.Create();
            try {
                var solution = workspace.OpenSolutionAsync(solutionPath).Result;
            }
            catch (Exception e) {
                return false;
            }
            return true;
        }

        public static void PrintRepsWithGoodSlnsUnder(string rootPath) {
            using (var reader = new StreamReader("slns_list.txt")) {
                var solutionPath = reader.ReadLine();
                while (solutionPath != null) {
                    if (CheckIfSolutionBuilds(solutionPath)) Console.WriteLine(solutionPath);
                    solutionPath = reader.ReadLine();
                }
            }
        }

        private static void Main(string[] args) {
            var solutionPathSeasons = @"D:\Users\Alexander\Documents\GitHub\DesktopSeasons\DesktopSeasons.sln"; //
            var solutionTest =
                @"D:\Users\Alexander\Documents\visual studio 2015\Projects\ForRoslynTest\ForRoslynTest.sln";
            var octokit = @"D:\Users\Alexander\Documents\GitHub\octokitnet\Octokit.sln";
            //Console.WriteLine(CheckIfSolutionBuilds(solutionPathOctocit));
            var extractMethodsFromSolution = ExtractMethodsFromSolution(octokit);
            foreach (var keyValuePair in extractMethodsFromSolution) {
                Console.WriteLine();
                Console.WriteLine(keyValuePair.Key.Identifier);
                Console.WriteLine(keyValuePair.Value.Item1);
                keyValuePair.Value.Item2.ForEach(i => Console.Write(i + " "));
                Console.WriteLine();
            }

            //ProgramDownloadFromGit.SelectNeededUrls();
            //     ProgramDownloadFromGit.CloneRepsFromFile();
            //ProgramDownloadFromGit.GetFileListOfRep("");
//            ProgramDownloadFromGit.FileRepsAndSaveUrls();
//            ProgramDownloadFromGit.SelectNeededUrls();
            //Console.WriteLine(ProgramDownloadFromGit.GetProjectNameAndOwner(@"https://api.github.com/repos/dotnet/coreclr"));
            // PrintRepsWithGoodSlnsUnder("");
            //ProgramDownloadFromGit.CloneRepsWithSlnFromFile(20);
            // ProgramDownloadFromGit.GetGoodDirNames().ForEach(Console.WriteLine);
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