using System;
using System.Collections;
using Common.Database;
using SQLite;

namespace Roslyn_Extract_Methods.SlnProviders {
    public class SlnProviderFromDatabase : ISlnProvider{
        private readonly SQLiteConnection _sqLiteConnection;

        public SlnProviderFromDatabase(string databaseName) {
            _sqLiteConnection = new SQLiteConnection(databaseName) {BusyTimeout = TimeSpan.FromSeconds(60)};
            _sqLiteConnection.CreateTable<Solution>();
        }

        public void Dispose() {
            _sqLiteConnection.Dispose();
        }

        private Solution _curSolution;

        public bool MoveNext() {
            var curSolution = _sqLiteConnection.Table<Solution>().FirstOrDefault(sln => sln.ProcessedTime == null);
            if (curSolution == null) return false;
            Current = curSolution.Path;
            _curSolution = curSolution;
            return true;
        }

        public Solution GetCurSolution() {
            return _curSolution;
        }

        public void Reset() {
            throw new NotSupportedException();
        }

        public string Current { get; set;  }

        object IEnumerator.Current => Current;

        public void UpdateSolution(bool couldNotCompile) {
            _curSolution.ProcessedTime = DateTime.Now;
            if (couldNotCompile) {
                _curSolution.WasCompiled = false;
            }
            _sqLiteConnection.Update(_curSolution);
        }
    }
}