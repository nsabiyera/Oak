using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using Massive;
using System.Diagnostics;

namespace Oak
{
    [DebuggerNonUserCode]
    public static class Query
    {
        public static void ExecuteNonQuery(this string query, ConnectionProfile connectionProfile = null)
        {
            if (connectionProfile == null) connectionProfile = new ConnectionProfile();

            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Connection = new SqlConnection(connectionProfile.ConnectionString);
            sqlCommand.Connection.Open();
            sqlCommand.CommandText = String.Format(query);
            sqlCommand.ExecuteNonQuery();
            sqlCommand.Connection.Close();
        }

        public static object ExecuteScalar(this string query, ConnectionProfile connectionProfile = null)
        {
            if (connectionProfile == null) connectionProfile = new ConnectionProfile();

            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Connection = new SqlConnection(connectionProfile.ConnectionString);
            sqlCommand.Connection.Open();
            sqlCommand.CommandText = String.Format(query);
            var result = sqlCommand.ExecuteScalar();
            sqlCommand.Connection.Close();

            return result;
        }

        public static SqlDataReader ExecuteReader(this string query, ConnectionProfile connectionProfile = null)
        {
            if (connectionProfile == null) connectionProfile = new ConnectionProfile();

            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Connection = new SqlConnection(connectionProfile.ConnectionString);
            sqlCommand.Connection.Open();
            sqlCommand.CommandText = String.Format(query);
            return sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public static void InsertInto(this object o, string table, ConnectionProfile connectionProfile = null)
        {
            if (connectionProfile == null) connectionProfile = new ConnectionProfile();

            DynamicRepository dynamicModel = new DynamicRepository(connectionProfile, table, "Id");

            dynamicModel.Insert(o);
        }
    }
}