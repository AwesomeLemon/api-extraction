namespace Roslyn_Extract_Methods {
    public class MethodCommentInfo {
        public string FullComment { get; }
        public string FirstSentence { get; }
        public bool IsXml { get; }

        public MethodCommentInfo(string fullComment, string firstSentence, bool isXml) {
            FullComment = fullComment;
            FirstSentence = firstSentence;
            IsXml = isXml;
        }
    }
}