using System;
using SQLite;

namespace Common.Database {
    public class Repo {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Url { get; set; }
        public int Stars { get; set; }
        public int Forks { get; set; }
        public int Watchers { get; set; }
        public DateTime? ProcessedTime { get; set; }

        public Repo() {
        }

        public Repo(string url, int stars, int forks, int watchers) {
            Url = url;
            Stars = stars;
            Forks = forks;
            Watchers = watchers;
            ProcessedTime = null;
        }
    }
}