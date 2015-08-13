using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace Importers.DataLayer
{
    public class NonQueryCommandRunner : INonQueryCommandRunner
    {
        private const int timeout = 9001;
        public int Timeout { get { return timeout; } }
        public IDbConnection Connection { get; set; }

        public NonQueryCommandRunner()
        {
        }

        public int Execute(string commandText)
        {
            using (var command = CreateCommandWithTimeout(this.Connection))
            {
                command.CommandText = commandText;
                return command.ExecuteNonQuery();
            }
        }

        private static IDbCommand CreateCommandWithTimeout(IDbConnection connection)
        {
            var result = connection.CreateCommand();
            result.CommandTimeout = timeout;
            return result;
        }
    }
}
