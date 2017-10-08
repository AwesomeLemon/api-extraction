using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Database;
using DownloadRepositories.SlnWriters;
using DownloadRepositories.UrlProviders;
using NDesk.Options;
using SQLite;

namespace DownloadRepositories {
    class Program {
        private static string _fileWithUrls = @"D:\DeepApiReps\again_reps_max.txt";
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

//            IRepoUrlProvider repoUrlProvider = new RepoUrlProviderFromFile(_fileWithUrls, toSkipNum);
            //            ISlnWriter slnWriter = new SlnWriterToFile(_pathToSlnFile);
            var sqLiteConnection = new SQLiteConnection(@"D:\hubic\mydb");
            RepoUrlProviderFromDatabase repoUrlProvider = new RepoUrlProviderFromDatabase(sqLiteConnection);
            SlnWriterToDatabase slnWriter = new SlnWriterToDatabase(sqLiteConnection);
            
            var nextUrl = repoUrlProvider.GetNextUrl();
            try {
                while (nextUrl != null) {
                    try {
                        string curRepUrlClone = nextUrl;
                        var ownerAndName = ExtractNameAndOwnerFromUrl(curRepUrlClone);

                        var repPath = _pathForCloning + ownerAndName.Replace('/', '_');
                        CloneRepository(curRepUrlClone, repPath);

                        Task.Factory.StartNew(() => {
                            slnWriter.Write(Directory.EnumerateFiles(repPath, "*.sln", SearchOption.AllDirectories), repoUrlProvider.GetCurRepoId());
                        });
                    }
                    catch (Exception e) when (
                        e is Octokit.ApiException ||
                        e is AggregateException ||
                        e is DirectoryNotFoundException) {
                        Console.WriteLine(e + "\n" + e.StackTrace);
                        using (var logFile = new StreamWriter(logFilePath, true)) {
                            logFile.WriteLine(3 + "\n" + e);
                        }
                    }
                    nextUrl = repoUrlProvider.GetNextUrl();
                }
            }
            finally {
                repoUrlProvider.Dispose();
            }
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
            if (Directory.Exists(path)) return;
            Console.WriteLine($"Cloning {cloneUrl} ...");
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

        public static string ExtractNameAndOwnerFromUrl(string url = "https://api.github.com/repos/kizzx2/cmd-recycle") {
            var lastSlash = url.LastIndexOf('/');
            var secondToLastSlach = url.LastIndexOf('/', lastSlash - 1);
            var ownerNameAndDotGit = url.Substring(secondToLastSlach + 1);
            return ownerNameAndDotGit.Substring(0, ownerNameAndDotGit.Length - 4);
        }
    }
}