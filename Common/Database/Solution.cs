using System;
using System.Collections.Generic;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Common.Database {
    public class Solution {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [ForeignKey(typeof(Repo))]
        public int RepoId { get; set; }
        public string Path { get; set; }
        public DateTime? ProcessedTime { get; set; }
        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<Method> Methods { get; set; }

        public Solution() {
            
        }
    }
}