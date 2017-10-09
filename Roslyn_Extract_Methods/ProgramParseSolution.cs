using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Common;
using Common.Database;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using NDesk.Options;
using Roslyn_Extract_Methods.SlnProviders;
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;
using Solution = Microsoft.CodeAnalysis.Solution;
using System.Configuration;

namespace Roslyn_Extract_Methods {
    internal class ProgramParseSolution {
        public static Tuple<string, List<ApiCall>, List<MethodParameter>> ExtractApiSequence(
            MethodDeclarationSyntax method, SemanticModel model) {
            var extractor = new ApiSequenceExtractor(model);
            extractor.Visit(method);
            return new Tuple<string, List<ApiCall>, List<MethodParameter>>(extractor.GetFullMethodName(method),
                extractor.Calls, extractor.MethodParameters);
        }

        private static List<Tuple<string, MethodCommentInfo, List<ApiCall>, List<MethodParameter>>>
            ExtractMethodsFromSolution(
                string solutionPath) {
            Solution solution = BuildSolution(solutionPath);
            if (solution == null) {
                return null;
            }
            Console.WriteLine("Solution was build");
            var res = new List<Tuple<string, MethodCommentInfo, List<ApiCall>, List<MethodParameter>>>();
            foreach (var project in solution.Projects) {
                foreach (var document in project.Documents) {
                    if (!File.Exists(document.FilePath)) {
                        Console.WriteLine("File doesn't exist: ");
                        Console.WriteLine(document.FilePath);
                        continue;
                    }
                    Console.WriteLine("Working with " + document.FilePath);

                    var rootNode = document.GetSyntaxRootAsync().Result;
                    var methodCollector = new MethodsCollector();
                    methodCollector.Visit(rootNode);

                    var methodsAndComments = CommentExtractor.ExtractSummaryComments(methodCollector.MethodDecls);
                    var curMethods = methodsAndComments.Keys.ToList();

                    var model = document.GetSemanticModelAsync().Result;
                    foreach (var method in curMethods) {
                        var methodNameAndCalls = ExtractApiSequence(method, model);
                        if (methodNameAndCalls.Item2.Count == 0) continue;
                        res.Add(new Tuple<string, MethodCommentInfo, List<ApiCall>, List<MethodParameter>>(
                            methodNameAndCalls.Item1, methodsAndComments[method], methodNameAndCalls.Item2,
                            methodNameAndCalls.Item3));
                    }
                }
            }
            return res;
        }

        private static Solution BuildSolution(string solutionPath) {
            var workspace = MSBuildWorkspace.Create();
            Solution solution;
            try {
                solution = workspace.OpenSolutionAsync(solutionPath).Result;
            }
            catch (Exception e) {
                //this means that solution can't build for some reason.
                Console.WriteLine("sln is dead.");
                using (var sw = new StreamWriter(LogFilePath, true)) {
                    sw.WriteLine(0);
                    sw.WriteLine(e.ToString());
                }
                return null;
            }
            return solution;
        }

        private static string _pathToSlnFile = ConfigurationManager.AppSettings["pathToSlnFile"];
        private static string _pathToExtractedDataFile = ConfigurationManager.AppSettings["pathToExtractedDataFile"];

        private static readonly string FileProcessedSlnsCount = ConfigurationManager.AppSettings["fileProcessedSlnsCount"];
        private static readonly string LogFilePath = ConfigurationManager.AppSettings["logFilePath"];
        private static readonly string DatabasePath = ConfigurationManager.AppSettings["databasePath"];

        private static void Main(string[] args) {
            if (!ParseArgs(args)) return;
            var sqLiteConnection = new SQLiteAsyncConnection(DatabasePath);
//            string configvalue1 = ConfigurationManager.AppSettings["key1"];
//            Console.WriteLine(configvalue1);
//            
//            return;
            //            var testSln = "D:\\DeepApiReps\\jediwhale_fitsharp\\fitSharp.sln";
            //            RestorePackages(testSln);
            //            var extractedMethods = ExtractMethodsFromSolution(testSln);
            //            return;
            //            var solution = sqLiteConnection
            //                .GetAllWithChildrenAsync<Common.Database.Solution>(sln => sln.ProcessedTime != null).Result;
            //            return;
            var resultWriter =
                new ResultWriters.ResultWriterToDatabase(sqLiteConnection);
//            new ResultWriters.ResultWriterToFile(_pathToExtractedDataFile);
//            using (var slnProvider = new SlnProviderFromFile(_pathToSlnFile, FileProcessedSlnsCount)) {
            using (var slnProvider = new SlnProviderFromDatabase(DatabasePath)) {
                while (true) {
                    while (slnProvider.MoveNext()) {
                        var slnPath = slnProvider.Current;
                        RestorePackages(slnPath);
                        var extractedMethodsFromSolution = ExtractMethodsFromSolution(slnPath);
                        slnProvider.UpdateSolution(extractedMethodsFromSolution == null);
                        if (extractedMethodsFromSolution != null) {
                            resultWriter.Write(extractedMethodsFromSolution, slnPath, slnProvider.GetCurSolution());
                        }
                    }
                    Console.WriteLine("...Waiting for 30 seconds...");
                    Thread.Sleep(30000); //download is slower than extraction.
                }
            }
        }

        private static void RestorePackages(string slnPath) {
            Console.WriteLine("Restoring packages");
            Console.WriteLine(slnPath);
            var process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "nuget.exe",
                    Arguments = "restore " + slnPath,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
            process.Start();
            process.WaitForExit();
            Console.WriteLine("Packages restored");
        }


        private static bool ParseArgs(string[] args) {
            var p = new OptionSet() {
                {"output=", "Path to output file with comments and api calls", x => _pathToExtractedDataFile = x},
                {"slns=", "Path to input file with paths of .sln files", x => _pathToSlnFile = x},
            };

            try {
                p.Parse(args);
            }
            catch (OptionException e) {
                Console.WriteLine("Error when parsing input arguments");
                Console.WriteLine(e.ToString());
                return false;
            }
            return true;
        }
    }
}