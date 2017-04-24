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
            catch (Exception e) {//this means that solution can't build for some reason.
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
                            new Tuple<string, List<ApiCall>>(methodsAndComments[method], extractedApiSequences));
                    }
                }
            }
            return res;
        }
        
        private static string _pathToSlnFile = @"D:\DeepApiReps\slns.txt";
        private static string _pathToExtractedDataFile = @"D:\DeepApiReps\res_3.txt";
        private static readonly string FileProcessedSlnsCount = "sln_num.txt";
        private static readonly string logFilePath = "exceptions.txt";
        private static void Main(string[] args) {
            int skippedCnt = 0;
            int processedNum;
            using (var sr = new StreamReader(FileProcessedSlnsCount)) {
                processedNum = int.Parse(sr.ReadLine()?? "0");
            }
            var slnFile = File.Open(_pathToSlnFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using (var slnFileReader = new StreamReader(slnFile)) {
                while (true) {
                    string slnPath;
                    while ((slnPath = slnFileReader.ReadLine()) != null) {
                        if (skippedCnt++ < processedNum) continue;
                        using (var sw = new StreamWriter(FileProcessedSlnsCount)) {
                            sw.WriteLine(++processedNum);
                        }
                        Console.WriteLine(slnPath);
                        var extractMethodsFromSolution = ExtractMethodsFromSolution(slnPath);
                        using (var extractedDataWriter = new StreamWriter(_pathToExtractedDataFile, true)) {
                            extractedDataWriter.WriteLine("**" + slnPath);
                            foreach (var keyValuePair in extractMethodsFromSolution) {
                                extractedDataWriter.WriteLine("//" + keyValuePair.Key.Identifier);
                                extractedDataWriter.WriteLine(keyValuePair.Value.Item1);
                                keyValuePair.Value.Item2.ForEach(i => extractedDataWriter.Write(i + " "));
                                extractedDataWriter.WriteLine();
                            }
                        }
                    }
                    Console.WriteLine("...Waiting for 100 seconds...");
                    Thread.Sleep(100000); //download is slower than extraction.
                }
            }
        }
    }
}