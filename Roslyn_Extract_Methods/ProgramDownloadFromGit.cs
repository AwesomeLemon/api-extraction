using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Octokit;
using ProductHeaderValue = Octokit.ProductHeaderValue;

namespace Roslyn_Extract_Methods {
    class ProgramDownloadFromGit {
        private static string fileName = "reps.txt";
        private static string fileRefactoredName = "reps_refactored.txt";
        private static string _pathForCloning = @"D:\DeepApiReps\";
        static GitHubClient client = new GitHubClient(new ProductHeaderValue("deep-api#"));
        private static readonly WebClient Wc;

        static ProgramDownloadFromGit() {
            var username = "***";//Must be valid!
            var password = "***";//Must be valid!

            client.Credentials = new Credentials(username, password);
            
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
            Wc = new WebClient {
                Headers = {
                    [HttpRequestHeader.Authorization] = $"Basic {credentials}",
                    [HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2"
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
                    writer.WriteLine(repository.CloneUrl + "\n" + repository.FullName +"\n" + repository.Url + "\n");
                }
            }
        }

        //You may wonder if second file is necessary. 
        //I introduced it so that if I needed some other urels\data, I wouldn't have to download it again.
        public static void SelectNeededUrls() {
            using (var reader = new StreamReader(fileName)) {
                using (var writer = new StreamWriter(fileRefactoredName)) {
                    while (reader.ReadLine() != null) {
                        var readLine = reader.ReadLine();
                        writer.WriteLine(readLine);
                        reader.ReadLine();
                        writer.WriteLine(reader.ReadLine());
                    }
                }
            }
        }

        private static void CloneRepsWithSlnFromFileHelper(string curRepUrlClone, string curRepUrlApi) {
            //    Console.WriteLine($"Working with {curRepUrlClone}. Total processed count is {cnt++}");
            Tuple<string, string> ownerAndName;
            try {
                ownerAndName = GetProjectNameAndOwner(curRepUrlApi);
            }
            catch (Exception e) {//from time to time github throws non-repeating nonsensical errors.
                return;
            }
            IEnumerable<string> slnFileNames =
                from filename in GetFileListOfRep(curRepUrlApi, ownerAndName)
                where filename.Contains(".sln")
                select filename;
            var fileNames = slnFileNames as IList<string> ?? slnFileNames.ToList();
            if (fileNames.Any()) {
                Console.WriteLine($"Working with {curRepUrlClone}.Contains sln. Cloning");
                var slnFileName = fileNames.Min();

                string repPath = _pathForCloning + ownerAndName.Item1 + "\\" + ownerAndName.Item2;
                CloneRepository(curRepUrlClone, repPath);
                if (ProgramParseSolution.CheckIfSolutionBuilds(repPath + "\\" + slnFileName)) {
                    //       goodSlnCount++;
                    Console.WriteLine(curRepUrlClone + " " + slnFileName);
                }
                else {
                    //DeleteDirectory(repPath);
                }
            }
        }

        public static void CloneRepsWithSlnFromFile(int skip = 0) {
            using (var reader = new StreamReader(fileRefactoredName)) {
                var taskList = new List<Task>();
               // int goodSlnCount = 0;
                //int cnt = 0;
                var curRepUrlClone = reader.ReadLine();
                var curRepUrlApi = reader.ReadLine();
                while (curRepUrlClone != null && curRepUrlApi != null) {
                    if (skip-- > 0) {
                        curRepUrlClone = reader.ReadLine();
                        curRepUrlApi = reader.ReadLine();
                        continue;
                    }
                    var clone = curRepUrlClone;
                    var api = curRepUrlApi;
                    taskList.Add(Task.Factory.StartNew(() => CloneRepsWithSlnFromFileHelper(clone, api)));
                    curRepUrlClone = reader.ReadLine();
                    curRepUrlApi = reader.ReadLine();
                    System.Threading.Thread.Sleep(10000);//so as not to overburden github api
                }
                Task.WaitAll(taskList.ToArray());
             //   Console.WriteLine(goodSlnCount);
            }
        }

        public static void CloneRepository(string cloneUrl, string path) {
            if (Directory.Exists(path)) DeleteDirectory(path);
            LibGit2Sharp.Repository.Clone(cloneUrl, path);
        }

        public static IEnumerable<string> GetFileListOfRep(string urlGithubApi, Tuple<string, string> ownerAndName) {
            var docs = client.Repository.Content.GetAllContents(ownerAndName.Item1, ownerAndName.Item2).Result;
            return docs.Select(i => i.Name);
        }

        public static Tuple<string, string> GetProjectNameAndOwner(string urlGithubApi) {
            string json = Wc.DownloadString(urlGithubApi);
            dynamic resJson = JsonConvert.DeserializeObject(json);
            string ownerAndName = resJson.full_name;
            string[] ownerAndNameArr = ownerAndName.Split('/');

            return new Tuple<string, string>(ownerAndNameArr[0], ownerAndNameArr[1]);
        }

        /// <summary>
        /// This is a dirty hack but unfortunately framework function Directory.Delete sometimes doesn't work.
        /// This solution was proposed on StackOverflow.
        /// </summary>
        private static void DeleteDirectory(string path) {
            System.Threading.Thread.Sleep(1000);
            Process.Start("cmd.exe", "/c " + @"rmdir /s/q " + path);
            System.Threading.Thread.Sleep(1000);
        }
    }
}
