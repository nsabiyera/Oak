using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;

namespace Oak
{
    public class Seed
    {
        public virtual ConnectionProfile ConnectionProfile { get; set; }

        public Seed()
            : this(null)
        {
        }

        public Seed(ConnectionProfile connectionProfile)
        {
            if (connectionProfile == null) connectionProfile = new ConnectionProfile();
            ConnectionProfile = connectionProfile;
        }

        public string CommandFor(string table, string action, dynamic[] schema)
        {
            string columns = "";

            var primaryKeyColumn = null as string;

            foreach (var entry in schema)
            {
                object definition = entry;

                var properties = definition.GetType().GetProperties();

                var column = properties.First();

                var nullable = properties.Has("Nullable", withValue: false, @in: definition);

                var identity = properties.Has("Identity", withValue: true, @in: definition);

                var defaultValue = properties.Get("Default", @in: definition);

                var primaryKeyValue = properties.Has("PrimaryKey", withValue: true, @in: definition);

                if (primaryKeyValue)
                {
                    primaryKeyColumn = column.Name;
                }

                columns += ColumnFor(column.Name, column.GetValue(definition, null).ToString(), nullable, defaultValue, identity);
            }

            return CreateTableCommand(table, columns, primaryKeyColumn);
        }

        public string ColumnFor(string name, string type, bool nullable, object defaultValue, bool identity)
        {
            string nullableString = nullable ? " NOT NULL" : "";

            string defaultAsString = defaultValue != null ? " DEFAULT('{0}')".With(defaultValue.ToString()) : "";

            string identityAsString = identity ? " IDENTITY(1,1)" : "";

            return "[{0}] {1}{2}{3}{4},".With(name, type, nullableString, defaultAsString, identityAsString);
        }

        public string CreateTableCommand(string table, string columns, string primaryKeyColumn)
        {
            var primaryKeyScript =
                " CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED ([{1}] ASC)".With(table, primaryKeyColumn ?? string.Empty);

            if (primaryKeyColumn == null)
            {
                primaryKeyScript = "";
            }

            return "CREATE TABLE [dbo].[{0}]({1}{2})".With(table, columns, primaryKeyScript);
        }

        public void PurgeDb()
        {
            DropAllForeignKeys();
            DropAllPrimaryKeys();
            DropAllTables();
        }

        private void DropAllForeignKeys()
        {
            var reader = "select name as constraint_name, object_name(parent_obj) as table_name from sysobjects where xtype = 'f'".ExecuteReader(ConnectionProfile);

            while (reader.Read())
            {
                "alter table {0} drop constraint {1} ".With(reader["table_name"], reader["constraint_name"]).ExecuteNonQuery(ConnectionProfile);
            }
        }

        private void DropAllPrimaryKeys()
        {
            var reader = "select name as constraint_name, object_name(parent_obj) as table_name from sysobjects where xtype = 'pk'".ExecuteReader(ConnectionProfile);

            while (reader.Read())
            {
                "alter table {0} drop constraint {1} ".With(reader["table_name"], reader["constraint_name"]).ExecuteNonQuery(ConnectionProfile);
            }
        }

        private void DropAllTables()
        {
            var reader = "select name as table_name from sysobjects where xtype = 'u'".ExecuteReader(ConnectionProfile);

            while (reader.Read())
            {
                "drop table {0} ".With(reader["table_name"]).ExecuteNonQuery(ConnectionProfile);
            }
        }
    }

    public static class Extensions
    {
        public static string With(this string s, params object[] args)
        {
            return string.Format(s, args);
        }

        public static bool Has(this PropertyInfo[] properties, string name, bool withValue, object @in)
        {
            return properties.Any(s => s.Name == name && Convert.ToBoolean(s.GetValue(@in, null)) == withValue);
        }

        public static object Get(this PropertyInfo[] properties, string name, object @in)
        {
            var property = properties.SingleOrDefault(s => s.Name == name);

            if (property == null) return null;

            return property.GetValue(@in, null);
        }
    }
}