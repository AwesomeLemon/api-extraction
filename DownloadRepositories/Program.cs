using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Octokit;
using FileMode = System.IO.FileMode;
using ProductHeaderValue = Octokit.ProductHeaderValue;
using Repository = LibGit2Sharp.Repository;

namespace DownloadRepositories {
    class Program {
        private static readonly string fileName = "reps_new.txt";
        private static readonly string fileRefactoredName = @"D:\DeepApiReps\reps_refactored_new.txt";
        private static readonly string _pathForCloning = @"D:\DeepApiReps\";
        private static readonly GitHubClient client = new GitHubClient(new ProductHeaderValue("deep-api#"));
        private static readonly string username = "AwesomeLemon"; //Must be valid!
        private static readonly string password = "****"; //Must be valid!
        private static readonly WebClient Wc;

        static Program() {
            client.Credentials = new Credentials("fc0c7bdf1370deeaef7fa099a6f1a069e9ae95b2");//new Credentials(username, password);
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
            Wc = new WebClient {
                Headers = {
                    [HttpRequestHeader.Authorization] = $"Basic {credentials}"
                    //[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2"
                }
            };
        }


        public static void FileRepsAndSaveUrls() {
            var request = new SearchRepositoriesRequest {
                Language = Language.CSharp,
                Stars = Range.GreaterThan(100)
            };
            var options = new ApiOptions() {
                PageCount = 3,
                StartPage = 1,
                PageSize = 50
            };
            var result = client.Search.SearchRepo(request).Result;
            Console.WriteLine(result.IncompleteResults);
            using (var writer = new StreamWriter(fileName)) {
                writer.WriteLine();
                foreach (var repository in result.Items) {
                    writer.WriteLine(repository.CloneUrl + "\n" + repository.FullName + "\n" + repository.Url + "\n");
                }
            }
        }

        //You may wonder if second file is necessary. 
        //I introduced it so that if I needed some other urls\data, I wouldn't have to download them again.
        public static void SelectNeededUrls() {
            using (var reader = new StreamReader(fileName)) {
                using (var writer = new StreamWriter(fileRefactoredName)) {
                    while (reader.ReadLine() != null) {
                        var readLine = reader.ReadLine();
                        writer.WriteLine(readLine);
                        writer.WriteLine(reader.ReadLine());
                        writer.WriteLine(reader.ReadLine());
                    }
                }
            }
        }

        private static void CloneRepsWithSlnFromFileHelper(string curRepUrlClone, string curRepUrlApi,
            string ownerAndNameStr) {
            //    Console.WriteLine($"Working with {curRepUrlClone}. Total processed count is {cnt++}");
            var ownerAndName = ownerAndNameStr.Split('/');
            var slnFileNames =
                from filename in GetFileListOfRep(curRepUrlApi, ownerAndName)
                where filename.Contains(".sln")
                select filename;
            var fileNames = slnFileNames as IList<string> ?? slnFileNames.ToList();
            if (fileNames.Any()) {
                Console.WriteLine($"Working with {curRepUrlClone}.Contains sln. Cloning");

                var slnFileName = fileNames.Min();
                var repPath = _pathForCloning + ownerAndName[0] + "\\" + ownerAndName[1];
                CloneRepository(curRepUrlClone, repPath);
                var fs = File.Open(@"D:\DeepApiReps\slns.txt", FileMode.Append, FileAccess.Write, FileShare.Read);
                using (var writer = new StreamWriter(fs)) {
                    writer.AutoFlush = true;
                    foreach (var sln in fileNames) {
                        writer.WriteLine(repPath + "\\" + sln);
                    }
                }

                //todo: this part was for deleting downloaded data
                //                if (ProgramParseSolution.CheckIfSolutionBuilds(repPath + "\\" + slnFileName)) {
                //                    Console.WriteLine(curRepUrlClone + " " + slnFileName);
                //                }
            }
        }

        public static void CloneRepsWithSlnFromFile() {
            int skip;
            int cnt = 0;
            using (var sr = new StreamReader("rep_num.txt")) {
                skip = int.Parse(sr.ReadLine() ?? "0");
            }
            using (var reader = new StreamReader(fileRefactoredName)) {
                var curRepUrlClone = reader.ReadLine();
                var ownerAndName = reader.ReadLine();
                var curRepUrlApi = reader.ReadLine();
                while (curRepUrlClone != null && curRepUrlApi != null) {
                    if (cnt++ < skip) {
                        curRepUrlClone = reader.ReadLine();
                        ownerAndName = reader.ReadLine();
                        curRepUrlApi = reader.ReadLine();
                        continue;
                    }
                    var clone = curRepUrlClone;
                    var api = curRepUrlApi;
                    try {
                        CloneRepsWithSlnFromFileHelper(clone, api, ownerAndName);
                    }
                    catch (Exception e) when (
                        e is LibGit2Sharp.LibGit2SharpException ||
                        e is Octokit.ApiException ||
                        e is AggregateException) {
                        Console.WriteLine(e.ToString() + "\n" + e.StackTrace);
                        using (var sw = new StreamWriter("exceptions.txt", true)) {
                            sw.WriteLine(3);
                            sw.WriteLine(e.ToString());
                        }
                    }
                    finally {
                        using (var sw = new StreamWriter("rep_num.txt")) {
                            sw.WriteLine(++skip);
                        }
                    }
                    curRepUrlClone = reader.ReadLine();
                    ownerAndName = reader.ReadLine();
                    curRepUrlApi = reader.ReadLine();
                }
            }
        }

        public static void CloneRepository(string cloneUrl, string path) {
            if (Directory.Exists(path)) return;
            Repository.Clone(cloneUrl, path);
        }

        public static IEnumerable<string> GetFileListOfRep(string urlGithubApi, string[] ownerAndName) {
            var docs = client.Repository.Content.GetAllContents(ownerAndName[0], ownerAndName[1]).Result;
            return docs.Select(i => i.Path);
        }

        public static Tuple<string, string> GetProjectNameAndOwner(string urlGithubApi) {
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
            var json = Wc.DownloadString(urlGithubApi);
            dynamic resJson = JsonConvert.DeserializeObject(json);
            string ownerAndName = resJson.full_name;
            var ownerAndNameArr = ownerAndName.Split('/');

            return new Tuple<string, string>(ownerAndNameArr[0], ownerAndNameArr[1]);
        }

        public static List<string> GetGoodDirNames() {
            var repNames = new List<string>();
            using (var reader = new StreamReader("cloning-reps-res.txt")) {
                string line;
                while ((line = reader.ReadLine()) != null) {
                    if (line.StartsWith("Working with ")) continue;
                    repNames.Add(line);
                }
            }
            return repNames;
        }
        static void Main(string[] args) {
            CloneRepsWithSlnFromFile();
        }
    }
}
