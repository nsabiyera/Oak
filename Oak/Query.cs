using System;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Data.SqlServerCe;

namespace Oak
{
    [DebuggerNonUserCode]
    public static class Query
    {
        public static void ExecuteNonQuery(this string query, ConnectionProfile connectionProfile = null)
        {
            if (connectionProfile == null) connectionProfile = new ConnectionProfile();

            using (SqlCeCommand sqlCommand = new SqlCeCommand())
            {
                using (sqlCommand.Connection = new SqlCeConnection(connectionProfile.ConnectionString))
                {
                    sqlCommand.Connection.Open();
                    sqlCommand.CommandText = String.Format(query);
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Connection.Close();
                }
            }
        }

        public static object ExecuteScalar(this string query, ConnectionProfile connectionProfile = null)
        {
            if (connectionProfile == null) connectionProfile = new ConnectionProfile();

            using (SqlCeCommand sqlCommand = new SqlCeCommand())
            {
                using (sqlCommand.Connection = new SqlCeConnection(connectionProfile.ConnectionString))
                {
                    sqlCommand.Connection.Open();
                    sqlCommand.CommandText = String.Format(query);
                    var result = sqlCommand.ExecuteScalar();
                    sqlCommand.Connection.Close();

                    return result;
                }
            }
        }

        public static SqlCeDataReader ExecuteReader(this string query, ConnectionProfile connectionProfile = null)
        {
            if (connectionProfile == null) connectionProfile = new ConnectionProfile();

            SqlCeCommand sqlCommand = new SqlCeCommand();
            sqlCommand.Connection = new SqlCeConnection(connectionProfile.ConnectionString);
            sqlCommand.Connection.Open();
            sqlCommand.CommandText = String.Format(query);
            return sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
        }
    }
}
