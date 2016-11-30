using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Octokit;
using Repository = LibGit2Sharp.Repository;

namespace Roslyn_Extract_Methods {
    internal class ProgramDownloadFromGit {
        private static readonly string fileName = "reps.txt";
        private static readonly string fileRefactoredName = "reps_refactored.txt";
        private static readonly string _pathForCloning = @"D:\DeepApiReps\";
        private static readonly GitHubClient client = new GitHubClient(new ProductHeaderValue("deep-api#"));
        private static readonly string username = "***"; //Must be valid!
        private static readonly string password = "***"; //Must be valid!
        private static readonly WebClient Wc;

        static ProgramDownloadFromGit() {
            client.Credentials = new Credentials(username, password);
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
                Stars = Range.GreaterThan(500)
            };
            var result = client.Search.SearchRepo(request).Result;
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
                if (ProgramParseSolution.CheckIfSolutionBuilds(repPath + "\\" + slnFileName)) {
                    Console.WriteLine(curRepUrlClone + " " + slnFileName);
                }
            }
        }

        public static void CloneRepsWithSlnFromFile(int skip = 0) {
            using (var reader = new StreamReader(fileRefactoredName)) {
                var curRepUrlClone = reader.ReadLine();
                var ownerAndName = reader.ReadLine();
                var curRepUrlApi = reader.ReadLine();
                while (curRepUrlClone != null && curRepUrlApi != null) {
                    if (skip-- > 0) {
                        curRepUrlClone = reader.ReadLine();
                        ownerAndName = reader.ReadLine();
                        curRepUrlApi = reader.ReadLine();
                        continue;
                    }
                    var clone = curRepUrlClone;
                    var api = curRepUrlApi;
                    CloneRepsWithSlnFromFileHelper(clone, api, ownerAndName);
                    curRepUrlClone = reader.ReadLine();
                    ownerAndName = reader.ReadLine();
                    curRepUrlApi = reader.ReadLine();
                }
            }
        }

        public static void CloneRepository(string cloneUrl, string path) {
            if (Directory.Exists(path)) Directory.Delete(path);
            Repository.Clone(cloneUrl, path);
        }

        public static IEnumerable<string> GetFileListOfRep(string urlGithubApi, string[] ownerAndName) {
            var docs = client.Repository.Content.GetAllContents(ownerAndName[0], ownerAndName[1]).Result;
            return docs.Select(i => i.Name);
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
    }
}