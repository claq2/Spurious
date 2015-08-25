using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace Importers.DataLayer
{
    public class NonQueryCommandRunner : INonQueryCommandRunner
    {
        private const int defaultTimeout = 9001;
        public int DefaultTimeout { get { return defaultTimeout; } }
        public IDbConnection Connection { get; set; }

        public NonQueryCommandRunner()
        {
        }

        public int Execute(string commandText)
        {
            using (var command = CreateCommandWithDefaultTimeout(commandText))
            {
                return command.ExecuteNonQuery();
            }
        }

        public IDbCommand CreateCommandWithDefaultTimeout(string commandText)
        {
            var result = this.Connection.CreateCommand();
            result.CommandTimeout = this.DefaultTimeout;
            result.CommandText = commandText;
            return result;
        }
    }
}
