using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Roslyn_Extract_Methods.Properties;

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
            Solution solution;
            try {
                solution = workspace.OpenSolutionAsync(solutionPath).Result;
            }
            catch (Exception e) {
                using (var sw = new StreamWriter("exceptions.txt", true)) {
                    sw.WriteLine(0);
                    sw.WriteLine(e.ToString());
                }
                return new Dictionary<MethodDeclarationSyntax, Tuple<string, List<ApiCall>>>();
            }

            var res = new Dictionary<MethodDeclarationSyntax, Tuple<string, List<ApiCall>>>();
            foreach (var project in solution.Projects) {
                foreach (var document in project.Documents) {
                    if (!File.Exists(document.FilePath)) continue;
                    Console.WriteLine("working with " + document.FilePath);
                    var rootNode = document.GetSyntaxRootAsync().Result;

                    var methodCollector = new MethodsCollector();
                    methodCollector.Visit(rootNode);
//                    methodCollector.ExtractSummaryComments();
//                    var curMethods = methodCollector.MethodDecls;
                    var methodsAndComments = CommentExtractor.ExtractSummaryComments(methodCollector.MethodDecls);
                    var curMethods = methodsAndComments.Keys.ToList();
                    var model = document.GetSemanticModelAsync().Result;
                    foreach (var method in curMethods) {
                        var extractedApiSequences = ExtractApiSequence(method, model);
                        if (extractedApiSequences.Count == 0) continue;
                        res.Add(method,
                            new Tuple<string, List<ApiCall>>(methodsAndComments[method],
                                extractedApiSequences));
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
            var res = CommentExtractor.CleanUpTags("adsf, <see cref=\"abs\" > adslkfj <other \"\"  > <see cref=\"12\">");
        }

//        private static void CloneRepsWithSlnFromFile() {
//            ProgramDownloadFromGit.CloneRepsWithSlnFromFile();
//        }


        private static void Main(string[] args) {
            var solutionPathSeasons = @"D:\Users\Alexander\Documents\GitHub\DesktopSeasons\DesktopSeasons.sln"; //
            var forRoslynTest =
                @"D:\Users\Alexander\Documents\visual studio 2015\Projects\ForRoslynTest\ForRoslynTest.sln";
            var octokit = @"D:\Users\Alexander\Documents\GitHub\octokitnet\Octokit.sln";
            var powershell = @"D:\DeepApiReps\PowerShell\PowerShell\powershell.sln";
            var outputFile = @"D:\DeepApiReps\res.txt";
            var outputFile2 = @"D:\DeepApiReps\res_2.txt";
            var outputFile3 = @"D:\DeepApiReps\res_3.txt";
            var mononet = @"D:\DeepApiReps\mono\mono\net_4_x.sln";
            var jsonnet = @"D:\DeepApiReps\Newtonsoft.Json\Src\Newtonsoft.Json.sln";

//            CloneRepsWithSlnFromFile();
//            return;
//            var extractMethodsFromSolution_test = ExtractMethodsFromSolution(@"D:\DeepApiReps\drasticactions\GiphyDotNet\UnofficialGiphyUwp.sln");
//            using (var writer = new StreamWriter(@"D:\DeepApiReps\jsonnet.txt", true)) {
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
            int skippedCnt = 0;
            int processedNum = 23381;
            using (var sr = new StreamReader("sln_num.txt")) {
                processedNum = int.Parse(sr.ReadLine()?? "0");
            }
            var fs = File.Open(@"D:\DeepApiReps\slns.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using (var reader = new StreamReader(fs))
            {
                while (true) {
                    string slnPath;
                    while ((slnPath = reader.ReadLine()) != null) {
                        if (skippedCnt++ < processedNum) continue;
                        using (var sw = new StreamWriter("sln_num.txt")) {
                            sw.WriteLine(++processedNum);
                        }
//                        if (!CheckIfSolutionBuilds(slnPath)) continue;
                        Console.WriteLine(slnPath);
                        var extractMethodsFromSolution = ExtractMethodsFromSolution(slnPath);
//                foreach (var keyValuePair in extractMethodsFromSolution) {
//                    Console.WriteLine();
//                    Console.WriteLine(keyValuePair.Key.Identifier);
//                    Console.WriteLine(keyValuePair.Value.Item1);
//                    keyValuePair.Value.Item2.ForEach(i => Console.Write(i + " "));
//                    Console.WriteLine();
//                }
                        using (var writer = new StreamWriter(outputFile3, true)) {
                            writer.WriteLine("**" + slnPath);
                            foreach (var keyValuePair in extractMethodsFromSolution) {
                                writer.WriteLine("//" + keyValuePair.Key.Identifier);
                                writer.WriteLine(keyValuePair.Value.Item1);
                                keyValuePair.Value.Item2.ForEach(i => writer.Write(i + " "));
                                writer.WriteLine();
                            }
                        }
                      
                    }
                    Console.WriteLine("...Waiting...");
                    Thread.Sleep(100000);
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