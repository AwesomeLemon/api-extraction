using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Roslyn_Extract_Methods.Database {
    public class MethodParameter {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [ForeignKey(typeof(Method))]
        public int MethodId { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }

        public MethodParameter(string type, string name) {
            Type = type;
            Name = name;
        }
    }
}