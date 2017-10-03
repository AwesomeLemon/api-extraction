﻿using System.Collections;
using Common.Database;
using SQLite;

namespace Roslyn_Extract_Methods.SlnProviders {
    public class SlnProviderFromDatabase : ISlnProvider{
        private readonly SQLiteConnection _sqLiteConnection;

        public SlnProviderFromDatabase(string databaseName) {
            _sqLiteConnection = new SQLiteConnection(databaseName);
        }

        public void Dispose() {
            _sqLiteConnection.Dispose();
        }

        public bool MoveNext() {
            var curSolution = _sqLiteConnection.Table<Solution>().FirstOrDefault(sln => sln.ProcessedTime == null);
            if (curSolution == null) return false;
            Current = curSolution.Path;
            return true;
        }

        public void Reset() {
            throw new System.NotSupportedException();
        }

        public string Current { get; set;  }

        object IEnumerator.Current => Current;
    }
}