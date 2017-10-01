using System;
using SQLite;

namespace Roslyn_Extract_Methods.Database {
    public class Method {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string FullComment { get; set; }
        public string FirstSummarySentence { get; set; }
        public string ApiCalls { get; set; }
        public bool CommentIsXml { get; set; }
        public DateTime SampledAt { get; set; }

        public Method( string name, string fullComment, string firstSummarySentence, string apiCalls, bool commentIsXml) {
            Name = name;
            FullComment = fullComment;
            FirstSummarySentence = firstSummarySentence;
            ApiCalls = apiCalls;
            CommentIsXml = commentIsXml;
            SampledAt = DateTime.Now;
        }
    }
}