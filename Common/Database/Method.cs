using System;
using SQLite;

namespace Common.Database {
    public class Method {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string FullComment { get; set; }
        public string FirstSummarySentence { get; set; }
        public string ApiCalls { get; set; }
        public bool CommentIsXml { get; set; }
        public DateTime SampledAt { get; set; }

        public Method( string name, string apiCalls, MethodCommentInfo methodCommentInfo) {
            Name = name;
            FullComment = methodCommentInfo.FullComment;
            FirstSummarySentence = methodCommentInfo.FirstSentence;
            ApiCalls = apiCalls;
            CommentIsXml = methodCommentInfo.IsXml;
            SampledAt = DateTime.Now;
        }
    }
}