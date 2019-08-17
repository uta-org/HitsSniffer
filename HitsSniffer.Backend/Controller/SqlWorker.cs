using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HitsSniffer.Model.Attrs;
using HitsSniffer.Model.Interfaces;
using MySql.Data.MySqlClient;
using Console = Colorful.Console;

namespace HitsSniffer.Controller
{
    public static class SqlWorker
    {
        private const string DBName = "hitssniffer";
        private const string DBUser = "usuario";
        private const string DBPass = "usuario";

        private static bool IsOpen { get; set; }

        private static MySqlConnection Connection { get; set; }

        // Thanks to: https://ourcodeworld.com/articles/read/218/how-to-connect-to-mysql-with-c-sharp-winforms-and-xampp
        public static void OpenConnection()
        {
            if (IsOpen)
                throw new Exception("DB connection is already opened!");

            // TODO: Get connectionString from ENV
            string connectionString = "datasource=localhost;" +
                                      "port=3306;" +
                                      $"username={DBUser};" +
                                      $"password={DBPass};" +
                                      $"database={DBName};";

            Connection = new MySqlConnection(connectionString);
            Task.Factory.StartNew(Connection.Open);

            IsOpen = true;
        }

        //public static async void DoQueryAsync<T>(this T instance)
        //    where T : IPrimaryKey
        //{
        //    while (Connection.State != ConnectionState.Open)
        //        await Task.Delay(100);

        //    using (var cmd = Connection.CreateCommand())
        //    {
        //        string tableName = SqlHelper.GetTableNameFromInstance<T>();
        //        var values = instance.GetValuesFromInstance();

        //        cmd.InsertInto<T>(tableName, true, values);
        //    }
        //}

        public static void DoQuery<T>(this T instance)
            where T : IPrimaryKey
        {
            while (Connection.State != ConnectionState.Open)
                Thread.Sleep(100);

            using (var cmd = Connection.CreateCommand())
            {
                string tableName = SqlHelper.GetTableNameFromInstance<T>();

                var list = instance.GetColumnData();
                var values = SqlHelper.GetValuesFromInstance(list);

                cmd.InsertInto<T>(list, tableName, true, values);
            }
        }

        public static void Release()
        {
            if (!IsOpen)
                throw new Exception("Can't release a connection that isn't open!");

            Connection.Close();
            Connection.Dispose();

            IsOpen = false;
        }
    }

    public static class SqlHelper
    {
        public static void InsertInto<T>(this MySqlCommand cmd, List<ColumnData> list, string tableName, bool executeQuery, params object[] objs)
            where T : IPrimaryKey
        {
            string columnHeader = GetColumns<T>(out int columnCount, list);

            if (objs.Length != columnCount)
                throw new ArgumentException("Number of params must be equal!", nameof(objs));

            string valuesHeader = string.Join(",", objs.Select((o, index) => $"@val{index}"));

            string query = $"INSERT INTO {tableName}({columnHeader}) VALUES ({valuesHeader})";

            {
                int index = 0;
                foreach (var o in objs)
                {
                    //cmd.Parameters.AddWithValue($"@val{index}", o);

                    // new MySqlParameter { ParameterName = $"@val{index}", Value = o }
                    cmd.Parameters.Add(new MySqlParameter { ParameterName = $"@val{index}", Value = o });
                    ++index;
                }
            }

            //cmd.Parameters.AddRange(objs);

            cmd.CommandText = query;
            cmd.CommandType = CommandType.Text;

            if (executeQuery)
            {
                try
                {
                    int queryResult = cmd.ExecuteNonQuery();
                    Console.WriteLine($"Query result: {queryResult}");
                }
                catch (MySqlException e)
                {
                    // TODO: Fix
                    Console.WriteLine(e, Color.Red);
                }
            }
        }

        private static string GetColumns<T>(out int columnCount, List<ColumnData> list)
            where T : IPrimaryKey
        {
            columnCount = list.Count;

            return string.Join(",", list.Select(attr => attr?.Name).Where(attr => !string.IsNullOrEmpty(attr)));
        }

        public static List<ColumnData> GetColumnData<T>(this T instance)
            where T : IPrimaryKey
        {
            return typeof(T).GetProperties()
                .Select(prop =>
                {
                    var attrs = prop.GetCustomAttributes(typeof(DbColumnNameAttribute), false);

                    // Some props doesn't have attrs (ie, RawData from HitData)
                    if (attrs.Length > 0)
                    {
                        if (attrs.First() is DbColumnNameAttribute attr)
                            return new ColumnData(attr.Order, attr.Name, prop.GetValue(instance), attr.MySqlDbType, attr.DbType);

                        return null;
                    }

                    return null;
                })
                .Where(item => item != null)
                .OrderBy(item => item.Order)
                .ToList();
        }

        public static string GetTableNameFromInstance<T>()
            where T : IPrimaryKey
        {
            return (typeof(T).GetCustomAttributes(true).First() as DbColumnNameAttribute)?.Name;
        }

        public static object[] GetValuesFromInstance(List<ColumnData> list)
        {
            return list.Select(item => item.Value).ToArray();

            // return typeof(T).GetProperties().Select(prop => prop.GetValue(instance)).ToArray();
        }

        public class ColumnData
        {
            public int Order { get; }
            public string Name { get; }
            public object Value { get; }
            public MySqlDbType MySqlDbType { get; }
            public DbType DbType { get; }

            private ColumnData()
            {
            }

            public ColumnData(int order, string name, object value, MySqlDbType mySqlDbType, DbType dbType)
            {
                Order = order;
                Name = name;
                Value = value;
                MySqlDbType = mySqlDbType;
                DbType = dbType;
            }
        }
    }
}