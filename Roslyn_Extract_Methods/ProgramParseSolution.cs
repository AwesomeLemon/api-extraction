using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using NDesk.Options;
using Roslyn_Extract_Methods.SlnProviders;

namespace Roslyn_Extract_Methods {
    internal class ProgramParseSolution {
        public static List<ApiCall> ExtractApiSequence(MethodDeclarationSyntax method, SemanticModel model) {
            var extractor = new ApiSequenceExtractor(model);
            extractor.Visit(method);
            return extractor.Calls;
        }

        private static Dictionary<MethodDeclarationSyntax, Tuple<Tuple<string, string>, List<ApiCall>>> ExtractMethodsFromSolution(
            string solutionPath) {
            Solution solution = BuildSolution(solutionPath);
            if (solution == null) {
                return new Dictionary<MethodDeclarationSyntax, Tuple<Tuple<string, string>, List<ApiCall>>>();
            }
            Console.WriteLine("Solution was build");
            var res = new Dictionary<MethodDeclarationSyntax, Tuple<Tuple<string, string>, List<ApiCall>>>();
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
                        var extractedApiSequences = ExtractApiSequence(method, model);
                        if (extractedApiSequences.Count == 0) continue;
                        res.Add(method,
                            new Tuple<Tuple<string, string>, List<ApiCall>>(methodsAndComments[method], extractedApiSequences));
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

        private static string _pathToSlnFile = @"D:\DeepApiReps\slns22.txt";
        private static string _pathToExtractedDataFile = @"D:\DeepApiReps\res_3.txt";

        private static readonly string FileProcessedSlnsCount = "sln_num.txt";
        private static readonly string LogFilePath = "exceptions.txt";

        private static void Main(string[] args) {
            if (!ParseArgs(args)) return;
            
            var resultWriterToFile = new ResultWriters.ResultWriterToFile(_pathToExtractedDataFile);
            using (var slnProvider = new SlnProviderFromFile(_pathToSlnFile, FileProcessedSlnsCount)) {
                while (true) {
                    while (slnProvider.MoveNext()) {
                        var slnPath = slnProvider.Current;
                        RestorePackages(slnPath);
                        var extractMethodsFromSolution = ExtractMethodsFromSolution(slnPath);
                        resultWriterToFile.Write(extractMethodsFromSolution, slnPath);
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