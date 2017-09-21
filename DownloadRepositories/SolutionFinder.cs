using System;
using System.IO;

namespace DownloadRepositories {
    public class SolutionFinder {
        public static void FindSolutionsInDirAndStoreInFile(string dir, string outFile) {
            var slnFile = File.Open(outFile, FileMode.Append, FileAccess.Write, FileShare.Read);
            using (var slnFileWriter = new StreamWriter(slnFile)) {
                slnFileWriter.AutoFlush = true;
                foreach (string file in Directory.EnumerateFiles(dir, "*.sln", SearchOption.AllDirectories)
                ) {
                    Console.WriteLine("Found .sln!");
                    slnFileWriter.WriteLine(file);
                }
            }
        } 
    }
}