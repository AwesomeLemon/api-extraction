using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DownloadRepositories.SlnWriters;
using DownloadRepositories.UrlProviders;
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
        private static string _pathToSlnFile = @"D:\DeepApiReps\slns2.txt";

        private static readonly string FileProcessedRepsCount = "rep_num.txt";
        private static readonly string logFilePath = "exceptions.txt";
        private static int _foldersToLeave = 500;
        private static bool _autoDeleteEnabled = true;

        static void Main(string[] args) {
            var parsedArgsSuccessful = ParseArgs(args);
            if (!parsedArgsSuccessful) return;

            if (_autoDeleteEnabled) {
                StartPeriodicAutomaticDeletion();
            }

            int toSkipNum = GetAlreadyProcessedNum();
            IRepoUrlProvider repoUrlProvider = new RepoUrlProvider(_fileWithUrls, toSkipNum);
            ISlnWriter slnWriter = new SlnWriterToFile(_pathToSlnFile);
            
            var nextUrlAndOwner = repoUrlProvider.GetNextUrlAndOwner();
            while (nextUrlAndOwner != null) {
                try {
                    string curRepUrlClone = nextUrlAndOwner.Item1;
                    var ownerAndName = nextUrlAndOwner.Item2.Split('/');

                    var repPath = _pathForCloning + ownerAndName[0] + "_" + ownerAndName[1];
                    CloneRepository(curRepUrlClone, repPath);

                    Task.Factory.StartNew(() => {
                        slnWriter.Write(Directory.EnumerateFiles(repPath, "*.sln", SearchOption.AllDirectories));
                    });
                }
                catch (Exception e) when (
                    e is LibGit2Sharp.LibGit2SharpException ||
                    e is Octokit.ApiException ||
                    e is AggregateException ||
                    e is DirectoryNotFoundException) {
                    Console.WriteLine(e + "\n" + e.StackTrace);
                    using (var logFile = new StreamWriter(logFilePath, true)) {
                        logFile.WriteLine(3 + "\n" + e);
                    }
                }
                finally {
                    using (var processedRepsFile = new StreamWriter(FileProcessedRepsCount)) {
                        processedRepsFile.WriteLine(++toSkipNum);
                    }
                }

                nextUrlAndOwner = repoUrlProvider.GetNextUrlAndOwner();
            }
        }

        private static int GetAlreadyProcessedNum() {
            int toSkipNum;
            if (!File.Exists(FileProcessedRepsCount)) File.Create(FileProcessedRepsCount).Close();
            using (var sr = new StreamReader(FileProcessedRepsCount)) {
                toSkipNum = int.Parse(sr.ReadLine() ?? "0");
            }
            return toSkipNum;
        }

        private static bool ParseArgs(string[] args) {
            var p = new OptionSet() {
                {"urls=", "Path to input file with urls of repos", x => _fileWithUrls = x},
                {"clonepath=", "Path for cloning", x => _pathForCloning = x},
                {"slns=", "Path to output file with paths of .sln files", x => _pathToSlnFile = x},
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

        private static void StartPeriodicAutomaticDeletion() {
            Task.Factory.StartNew(() => {
                while (true) {
                    DeleteAllButNewestDirectories(_pathForCloning);
                    int HUNDRED_MINUTES_IN_MILLIS = 6000000;
                    Thread.Sleep(HUNDRED_MINUTES_IN_MILLIS);
                }
            });
        }

        private static void CloneRepository(string cloneUrl, string path) {
            Console.WriteLine($"Cloning {cloneUrl} ...");
            if (Directory.Exists(path)) return;
            //Repository.Clone(cloneUrl, path);
            var process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "git",
                    Arguments = "clone --depth 1 " + cloneUrl + " " + path,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };

            process.Start();
            process.WaitForExit();
        }

        private static void DeleteAllButNewestDirectories(string path) {
            var directoryInfo = new DirectoryInfo(path);
            var subdirs = directoryInfo.EnumerateDirectories()
                .OrderByDescending(d => d.LastAccessTime)
                .Skip(_foldersToLeave)
                .Select(d => d.Name)
                .ToList();
            Console.WriteLine("Deleting dirs:");
            subdirs.ForEach(Console.WriteLine);
            Console.WriteLine();
            subdirs.ForEach(s => DeleteDirectory(directoryInfo.FullName + "\\" + s));
        }

        public static void DeleteDirectory(string path) {
            var process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "cmd.exe",
                    Arguments = "/c @rmdir /s /q " + path,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };

            process.Start();
            process.WaitForExit();
        }
    }
}