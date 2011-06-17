using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using Massive;

namespace Oak
{
    public static class Query
    {
        public static void ExecuteNonQuery(this string query, ConnectionProfile connectionProfile)
        {
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Connection = new SqlConnection(connectionProfile.ConnectionString);
            sqlCommand.Connection.Open();
            sqlCommand.CommandText = String.Format(query);
            sqlCommand.ExecuteNonQuery();
            sqlCommand.Connection.Close();
        }

        public static object ExecuteScalar(this string query, ConnectionProfile connectionProfile)
        {
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Connection = new SqlConnection(connectionProfile.ConnectionString);
            sqlCommand.Connection.Open();
            sqlCommand.CommandText = String.Format(query);
            var result = sqlCommand.ExecuteScalar();
            sqlCommand.Connection.Close();

            return result;
        }

        public static SqlDataReader ExecuteReader(this string query, ConnectionProfile connectionProfile)
        {
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Connection = new SqlConnection(connectionProfile.ConnectionString);
            sqlCommand.Connection.Open();
            sqlCommand.CommandText = String.Format(query);
            return sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public static void InsertInto(this object o, string table, ConnectionProfile connectionProfile)
        {
            DynamicModel dynamicModel = new DynamicModel(connectionProfile, table, "Id");

            dynamicModel.Insert(o);
        }
    }
}