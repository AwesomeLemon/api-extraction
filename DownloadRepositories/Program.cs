using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using NDesk.Options;
using Newtonsoft.Json;
using Octokit;
using FileMode = System.IO.FileMode;
using ProductHeaderValue = Octokit.ProductHeaderValue;
using Repository = LibGit2Sharp.Repository;

namespace DownloadRepositories {
    class Program {
        private static string _fileWithUrls = @"D:\DeepApiReps\reps_refactored_new.txt";
        private static string _pathForCloning = @"D:\DeepApiReps\";
        private static string _pathToSlnFile = @"D:\DeepApiReps\slns.txt";

        private static readonly GitHubClient Client = new GitHubClient(new ProductHeaderValue("deep-api#"));
        private static readonly string FileProcessedRepsCount = "rep_num.txt";
        private static readonly string logFilePath = "exceptions.txt";

        static void Main(string[] args) {
            var p = new OptionSet() {
                { "urls=", "Path to input file with urls of repos", x => _fileWithUrls = x },
                { "clonepath=", "Path for cloning", x => _pathForCloning = x },
                { "slns=", "Path to output file with paths of .sln files", x => _pathToSlnFile = x },
            };
            
            try {
                p.Parse(args);
            }
            catch (OptionException e) {
                Console.WriteLine("Error when parsing input arguments");
                Console.WriteLine(e.ToString());
                return;
            }
            int toSkipNum;
            int skippedCounter = 0;
            if (!File.Exists(FileProcessedRepsCount)) File.Create(FileProcessedRepsCount).Close();
            using (var sr = new StreamReader(FileProcessedRepsCount)) {
                toSkipNum = int.Parse(sr.ReadLine() ?? "0");
            }
            
            using (var urlFileReader = new StreamReader(_fileWithUrls)) {
                var curRepUrlClone = urlFileReader.ReadLine();
                var ownerAndNameStr = urlFileReader.ReadLine();
                var curRepUrlApi = urlFileReader.ReadLine();

                while (curRepUrlClone != null && curRepUrlApi != null) {
                    if (skippedCounter++ < toSkipNum) {
                        curRepUrlClone = urlFileReader.ReadLine();
                        ownerAndNameStr = urlFileReader.ReadLine();
                        curRepUrlApi = urlFileReader.ReadLine();
                        continue;
                    }

                    try {
                        var ownerAndName = ownerAndNameStr.Split('/');
                        //var slnFileNames =
                        //    from filename in GetFileListOfRep(ownerAndName)
                        //    where filename.Contains(".sln")
                        //    select filename;
                        //var fileNames = slnFileNames as IList<string> ?? slnFileNames.ToList();

                        //if (fileNames.Any()) {
                        Console.WriteLine($"Working with {curRepUrlClone}. Contains sln file. Cloning...");
                        var repPath = _pathForCloning + ownerAndName[0] + "_" + ownerAndName[1];
                        CloneRepository(curRepUrlClone, repPath);

                            
                        var slnFile = File.Open(_pathToSlnFile, FileMode.Append, FileAccess.Write, FileShare.Read);
                        using (var slnFileWriter = new StreamWriter(slnFile)) {
                            slnFileWriter.AutoFlush = true;
                            foreach (string file in Directory.EnumerateFiles(repPath, "*.sln", SearchOption.AllDirectories))
                                {
                                Console.WriteLine("ssmth");
                                slnFileWriter.WriteLine(file);
                                //Console.WriteLine(file);
                                }
                            //foreach (var sln in fileNames) {
                            //    slnFileWriter.WriteLine(repPath + "\\" + sln);
                            //}
                         //   }
                        }
                    }
                    catch (Exception e) when (
                        e is LibGit2Sharp.LibGit2SharpException ||
                        e is Octokit.ApiException ||
                        e is AggregateException) {
                        Console.WriteLine(e + "\n" + e.StackTrace);
                        
                        using (var logFile = new StreamWriter(logFilePath, true))
                            logFile.WriteLine(3 + "\n" + e);
                    }
                    finally {
                        using (var processedRepsFile = new StreamWriter(FileProcessedRepsCount)) {
                            processedRepsFile.WriteLine(++toSkipNum);
                        }
                    }

                    curRepUrlClone = urlFileReader.ReadLine();
                    ownerAndNameStr = urlFileReader.ReadLine();
                    curRepUrlApi = urlFileReader.ReadLine();
                }
            }
        }

        private static void CloneRepository(string cloneUrl, string path) {
            if (Directory.Exists(path)) return;
            Repository.Clone(cloneUrl, path);
        }

        private static IEnumerable<string> GetFileListOfRep(string[] ownerAndName) {
            var docs = Client.Repository.Content.GetAllContents(ownerAndName[0], ownerAndName[1]).Result;
            return docs.Select(i => i.Path);
        }
    }
}
