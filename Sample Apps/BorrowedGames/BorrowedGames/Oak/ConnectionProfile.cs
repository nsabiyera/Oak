using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace Oak
{
    public class ConnectionProfile
    {
        private string connectionString;
        public string ConnectionString
        {
            get
            {
                //feel free to change this and just return your connection string
                if (string.IsNullOrEmpty(connectionString))
                {
                    //get the first connection string that isn't LocalSqlServer
                    foreach (ConnectionStringSettings config in ConfigurationManager.ConnectionStrings)
                    {
                        if (config.Name != "LocalSqlServer")
                        {
                            return config.ConnectionString;
                        }
                    }
                }

                return connectionString;
            }
            set
            {
                connectionString = value;
            }
        }
    }
}