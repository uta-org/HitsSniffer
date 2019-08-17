using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using HitsSniffer.Model.Attrs;
using HitsSniffer.Model.Interfaces;

namespace HitsSniffer.Controller
{
    public static class SqlWorker
    {
        private const string DBName = "hitssnifer";
        private const string DBUser = "root";
        private const string DBPass = "";

        public static void DoQuery<T>(this T instance)
            where T : IPrimaryKey
        {
            // TODO: Get connectionString from ENV
            string connectionString =
                "Server=localhost;" +
                $"Database={DBName};" +
                $"User ID={DBUser};" +
                $"Password={DBPass};" +
                "Pooling=false";

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    string tableName = SqlHelper.GetTableNameFromInstance<T>();
                    var values = instance.GetValuesFromInstance();

                    cmd.InsertInto<T>(tableName, true, values);

                    //string sql =
                    //    "SELECT * FROM gamasproducto";

                    //dbcmd.CommandText = sql;
                    //using (IDataReader reader = dbcmd.ExecuteReader())
                    //{
                    //    while (reader.Read())
                    //    {
                    //        Console.WriteLine("Gama: {0}", reader["Gama"].ToString());
                    //        Console.WriteLine("Descripción: {0}", reader["DescripcionTexto"].ToString());
                    //    }
                    //}
                }
            }
        }
    }

    public static class SqlHelper
    {
        public static void InsertInto<T>(this SqlCommand cmd, string tableName, bool executeQuery, params object[] objs)
            where T : IPrimaryKey
        {
            string columnHeader = GetColumns<T>(out int columnCount);

            if (objs.Length != columnCount)
                throw new ArgumentException("Number of params must be equal!", nameof(objs));

            string valuesHeader = string.Join(",", objs.Select((o, index) => $"@val{index}"));

            string query = $"INSERT INTO {tableName}({columnHeader}) VALUES ({valuesHeader})";

            cmd.Parameters.AddRange(objs);
            cmd.CommandText = query;
            cmd.CommandType = CommandType.Text;

            if (executeQuery)
                cmd.ExecuteNonQuery();
        }

        private static string GetColumns<T>(out int columnCount)
            where T : IPrimaryKey
        {
            var list = typeof(T).GetProperties()
                .Select(prop => prop.GetCustomAttributes(true).First()).ToList();

            columnCount = list.Count;

            return string.Join(",", list.Cast<DbColumnNameAttribute>().Select(attr => attr?.Name));
        }

        public static string GetTableNameFromInstance<T>()
            where T : IPrimaryKey
        {
            return (typeof(T).GetCustomAttributes(true).First() as DbColumnNameAttribute)?.Name;
        }

        public static object[] GetValuesFromInstance<T>(this T instance)
            where T : IPrimaryKey
        {
            return typeof(T).GetProperties().Select(prop => prop.GetValue(instance)).ToArray();
        }
    }
}