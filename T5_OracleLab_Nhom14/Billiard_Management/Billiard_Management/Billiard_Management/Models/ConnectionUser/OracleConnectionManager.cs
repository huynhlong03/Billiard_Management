using Oracle.ManagedDataAccess.Client;

namespace Billiard_Management.Models.ConnectionUser
{
    public class OracleConnectionManager
    {
        private OracleConnection _connection;

        public OracleConnectionManager()
        {
        }

        public async Task OpenConnectionAsync(string connectionString)
        {
            if (_connection == null)
            {
                _connection = new OracleConnection(connectionString);
            }

            if (_connection.State != System.Data.ConnectionState.Open)
            {
                await _connection.OpenAsync();
            }
        }

        public async Task CloseConnectionAsync()
        {
            if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
            {
                await _connection.CloseAsync();
            }
        }

        public OracleConnection GetConnection()
        {
            if (_connection == null || _connection.State != System.Data.ConnectionState.Open)
            {
                throw new InvalidOperationException("Connection is not open.");
            }

            return _connection;
        }
    }
}
