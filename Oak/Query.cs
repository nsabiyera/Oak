using System;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace Oak
{
    [DebuggerNonUserCode]
    public static class Query
    {
        public static void ExecuteNonQuery(this string query, ConnectionProfile connectionProfile = null)
        {
            if (connectionProfile == null) connectionProfile = new ConnectionProfile();

            DbProviderFactory factory = DbProviderFactories.GetFactory(connectionProfile.ProviderName);
            DbConnection conn = factory.CreateConnection();
            conn.ConnectionString = connectionProfile.ConnectionString;
            DbCommand comm = factory.CreateCommand();
            comm.Connection = conn;
            comm.Connection.Open();
            comm.CommandText = String.Format(query);
            comm.ExecuteNonQuery();
            comm.Connection.Close();
        }

        public static object ExecuteScalar(this string query, ConnectionProfile connectionProfile = null)
        {
            if (connectionProfile == null) connectionProfile = new ConnectionProfile();

            DbProviderFactory factory = DbProviderFactories.GetFactory(connectionProfile.ProviderName);
            DbConnection conn = factory.CreateConnection();
            conn.ConnectionString = connectionProfile.ConnectionString;
            DbCommand comm = factory.CreateCommand();
            comm.Connection = conn;
            comm.Connection.Open();
            comm.CommandText = String.Format(query);
            var result = comm.ExecuteScalar();
            comm.Connection.Close();

            return result;
        }

        public static DbDataReader ExecuteReader(this string query, ConnectionProfile connectionProfile = null)
        {
            if (connectionProfile == null) connectionProfile = new ConnectionProfile();

            DbProviderFactory factory = DbProviderFactories.GetFactory(connectionProfile.ProviderName);
            DbConnection conn = factory.CreateConnection();
            conn.ConnectionString = connectionProfile.ConnectionString;
            DbCommand comm = factory.CreateCommand();
            comm.Connection = conn;
            comm.Connection.Open();
            comm.CommandText = String.Format(query);
            return comm.ExecuteReader(CommandBehavior.CloseConnection);
        }
    }
}
