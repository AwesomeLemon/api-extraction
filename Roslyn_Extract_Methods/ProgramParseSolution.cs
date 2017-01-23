using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                    if (!File.Exists(document.FilePath)) continue;
                    Console.WriteLine("working with " + document.FilePath);
                    var rootNode = document.GetSyntaxRootAsync().Result;

                    var methodCollector = new MethodsCollector();
                    methodCollector.Visit(rootNode);
                    methodCollector.ExtractSummaryComments();
                    var curMethods = methodCollector.MethodDecls;

                    var model = document.GetSemanticModelAsync().Result;
                    try {
                        foreach (var method in curMethods) {
                            try {
                                var extractedApiSequences = ExtractApiSequence(method, model);
                                if (extractedApiSequences.Count == 0) continue;
                                res.Add(method,
                                    new Tuple<string, List<ApiCall>>(methodCollector.MethodComments[method],
                                        extractedApiSequences));
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

        private static void CleanUpTagTest() {
            var res = MethodsCollector.CleanUpTags("adsf, <see cref=\"abs\" > adslkfj <other \"\"  > <see cref=\"12\">");
        }

        private static void Main(string[] args) {
            var solutionPathSeasons = @"D:\Users\Alexander\Documents\GitHub\DesktopSeasons\DesktopSeasons.sln"; //
            var forRoslynTest =
                @"D:\Users\Alexander\Documents\visual studio 2015\Projects\ForRoslynTest\ForRoslynTest.sln";
            var octokit = @"D:\Users\Alexander\Documents\GitHub\octokitnet\Octokit.sln";
            var powershell = @"D:\DeepApiReps\PowerShell\PowerShell\powershell.sln";
            var outputFile = @"D:\DeepApiReps\res.txt";
            var outputFile2 = @"D:\DeepApiReps\res_2.txt";
            var mononet = @"D:\DeepApiReps\mono\mono\net_4_x.sln";
            var jsonnet = @"D:\DeepApiReps\Newtonsoft.Json\Src\Newtonsoft.Json.sln";
           // var extractMethodsFromSolution_test = ExtractMethodsFromSolution(jsonnet);
//            using (var writer = new StreamWriter(outputFile2, true)) {
//                foreach (var keyValuePair2 in extractMethodsFromSolution_test) {
//                    writer.WriteLine("//" + keyValuePair2.Key.Identifier);
//                    writer.WriteLine(keyValuePair2.Value.Item1);
//                    keyValuePair2.Value.Item2.ForEach(i => writer.Write(i + " "));
//                    writer.WriteLine();
//                }
//            }
//            return;
//            foreach (var keyValuePair in extractMethodsFromSolution_test) {
//                Console.WriteLine();
//                var methodDeclarationSyntax = keyValuePair.Key;
//                var value = keyValuePair.Value;
//                Console.WriteLine(keyValuePair.Key.Identifier);
//                Console.WriteLine(value.Item1);
//                value.Item2.ForEach(i => Console.Write(i + " "));
//                Console.WriteLine();
//
//            }
//            return;
            //Console.WriteLine(CheckIfSolutionBuilds(solutionPathOctocit));
            using (var reader = new StreamReader(@"D:\DeepApiReps\slns.txt"))
            {
                string slnPath;
                while ((slnPath = reader.ReadLine()) != null) {
                    if (!CheckIfSolutionBuilds(slnPath)) continue;
                    Console.WriteLine(slnPath);
                    var extractMethodsFromSolution = ExtractMethodsFromSolution(slnPath);
//                foreach (var keyValuePair in extractMethodsFromSolution) {
//                    Console.WriteLine();
//                    Console.WriteLine(keyValuePair.Key.Identifier);
//                    Console.WriteLine(keyValuePair.Value.Item1);
//                    keyValuePair.Value.Item2.ForEach(i => Console.Write(i + " "));
//                    Console.WriteLine();
//                }
                    using (var writer = new StreamWriter(outputFile, true)) {
                        writer.WriteLine("**" + slnPath);
                        foreach (var keyValuePair in extractMethodsFromSolution) {
                            writer.WriteLine("//" + keyValuePair.Key.Identifier);
                            writer.WriteLine(keyValuePair.Value.Item1);
                            keyValuePair.Value.Item2.ForEach(i => writer.Write(i + " "));
                            writer.WriteLine();
                        }
                    }
                }
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