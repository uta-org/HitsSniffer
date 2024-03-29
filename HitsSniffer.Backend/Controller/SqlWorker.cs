﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HitsSniffer.Model;
using HitsSniffer.Model.Attrs;
using HitsSniffer.Model.Interfaces;
using MySql.Data.MySqlClient;
using uzLib.Lite.Extensions;
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

        private static bool IsBeginTransaction;

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
                                      $"database={DBName};"; // +
                                                             // "MultipleActiveResultSets=True;";

            Connection = new MySqlConnection(connectionString);
            Task.Factory.StartNew(Connection.Open);

            IsOpen = true;
        }

        public static void DoQuery<T>(this T instance)
            where T : IPrimaryKey
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            while (Connection.State != ConnectionState.Open)
                Thread.Sleep(100);

            using (var cmd = Connection.CreateCommand())
            {
                string tableName = SqlHelper.GetTableNameFromInstance<T>();

                var list = instance.GetColumnData();
                cmd.InsertInto(list, tableName, true);
            }
        }

        public static int DoExists<T>(this T instance)
            where T : IData
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            while (Connection.State != ConnectionState.Open)
                Thread.Sleep(100);

            using (var cmd = Connection.CreateCommand())
            {
                string tableName = SqlHelper.GetTableNameFromInstance<T>(true);

                string query = $"SELECT id FROM {tableName} WHERE name = @name";

                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@name", instance.Name);

                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        return int.Parse(reader["id"].ToString());
            }

            return -1;
        }

        public static void DoSelect<T>(Action<MySqlDataReader> readerCallback,
            params Tuple<string, object>[] @params)
            where T : IPrimaryKey
            => DoSelect<T>(readerCallback, "*", string.Empty, string.Empty, string.Empty, false, @params);

        public static void DoSelect<T>(Action<MySqlDataReader> readerCallback, string columns = "*",
            params Tuple<string, object>[] @params)
            where T : IPrimaryKey
            => DoSelect<T>(readerCallback, columns, string.Empty, string.Empty, string.Empty, false, @params);

        public static void DoSelect<T>(Action<MySqlDataReader> readerCallback, string columns = "*", bool mainTable = true,
            params Tuple<string, object>[] @params)
            where T : IPrimaryKey
            => DoSelect<T>(readerCallback, columns, string.Empty, string.Empty, string.Empty, mainTable, @params);

        public static void DoSelect<T>(Action<MySqlDataReader> readerCallback, string columns = "*", string whereClause = "", bool mainTable = true, params Tuple<string, object>[] @params)
            where T : IPrimaryKey
            => DoSelect<T>(readerCallback, columns, whereClause, string.Empty, string.Empty, mainTable, @params);

        public static void DoSelect<T>(Action<MySqlDataReader> readerCallback, string columns = "*", string whereClause = "", string orderBy = "", bool mainTable = true, params Tuple<string, object>[] @params)
            where T : IPrimaryKey
            => DoSelect<T>(readerCallback, columns, whereClause, orderBy, string.Empty, mainTable, @params);

        public static void DoSelect<T>(Action<MySqlDataReader> readerCallback, string columns = "*", string whereClause = "", string orderBy = "", string limitClause = "", bool mainTable = true, params Tuple<string, object>[] @params)
            where T : IPrimaryKey
        {
            if (readerCallback == null)
                throw new ArgumentNullException(nameof(readerCallback));

            while (Connection.State != ConnectionState.Open)
                Thread.Sleep(100);

            using (var cmd = Connection.CreateCommand())
            {
                string tableName = SqlHelper.GetTableNameFromInstance<T>(mainTable);

                string query = $"SELECT {columns} FROM {tableName}";

                if (!string.IsNullOrEmpty(whereClause))
                    query += $"WHERE {whereClause}";

                if (!string.IsNullOrEmpty(orderBy))
                    query += $"ORDER BY {orderBy}";

                if (!string.IsNullOrEmpty(limitClause))
                    query += $"LIMIT {limitClause}";

                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;

                if (!@params.IsNullOrEmpty())
                    foreach (var param in @params)
                        cmd.Parameters.AddWithValue(param.Item1, param.Item2);

                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        readerCallback(reader);
                    }
            }
        }

        public static int? DoExistsTransaction<T>(string name)
            where T : IPrimaryKey
            => BeginExistsTransaction<T>(name, false, false);

        public static bool DoExistsTransaction(string name, Type type)
            => InternalBeginExistsTransaction(type, name, true, false).HasValue;

        public static int? BeginExistsTransaction<T>(string name, bool mainTable = true, bool trackTransaction = true)
            where T : IPrimaryKey
            => InternalBeginExistsTransaction(typeof(T), name, mainTable, trackTransaction);

        private static int? InternalBeginExistsTransaction(Type type, string name, bool mainTable = true, bool trackTransaction = true)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            while (Connection.State != ConnectionState.Open)
                Thread.Sleep(100);

            using (var cmd = Connection.CreateCommand())
            {
                string tableName = SqlHelper.GetTableNameFromInstance(type, mainTable);

                string query = $"SELECT id FROM {tableName} WHERE name = @name ORDER BY date";

                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@name", name);

                using (var reader = cmd.ExecuteReader())
                    while (reader.Read()) // Return first record ordered by date
                        return int.Parse(reader["id"].ToString());
            }

            if (trackTransaction)
                IsBeginTransaction = true;

            return null;
        }

        public static bool EndExistsTransaction()
        {
            if (!IsBeginTransaction)
                return false;

            IsBeginTransaction = false;
            return true;
        }

        public static int RegisterTableValue(this object instance, Type ownerType)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            while (Connection.State != ConnectionState.Open)
                Thread.Sleep(100);

            bool isRepo = ownerType != null;
            // Sometimes we could see that a repo has a null ownereType, in this case the FK associated with it will not be saved

            using (var cmd = Connection.CreateCommand())
            {
                string tableName = SqlHelper.GetTableNameFromInstance(instance.GetType(), true);

                string query = $"INSERT INTO {tableName}(id,name,date) VALUES(@id, @name, @date)";

                if (isRepo)
                {
                    var repoData = instance as RepoData;
                    if (ownerType == typeof(OrgData) || ownerType == typeof(UserData))
                    {
                        IPrimaryKey dependant = DefineDependantInstance(repoData, ownerType);

                        if (dependant != null)
                        {
                            cmd.Parameters.AddWithValue("@dep_id", dependant.Id);
                            query = $"INSERT INTO {tableName}(id,{(ownerType == typeof(OrgData) ? "org_id" : "user_id")},name,date) VALUES(@id, @dep_id, @name, @date)";
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }

                cmd.Parameters.AddWithValue("@id", 0);
                cmd.Parameters.AddWithValue("@name", ((dynamic)instance).Name);
                cmd.Parameters.AddWithValue("@date", SqlHelper.GetNow());

                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;

                cmd.ExecuteNonQuery();
                return (int)cmd.LastInsertedId;
            }
        }

        public static IPrimaryKey DefineDependantInstance(RepoData repoData, Type dependantType)
        {
            if (!repoData.IsDependant)
                throw new InvalidOperationException();

            bool dependantExists = DoExistsTransaction(repoData.OwnerName, dependantType);

            IPrimaryKey dependantInstance;
            dependantInstance = !dependantExists
                ? RegisterTableValueAsPrimary(repoData.OwnerName, dependantType)
                : GetDependant(repoData.OwnerName, dependantType);

            return dependantInstance;
        }

        public static IPrimaryKey GetDependant(string name, Type dependantType)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            while (Connection.State != ConnectionState.Open)
                Thread.Sleep(100);

            using (var cmd = Connection.CreateCommand())
            {
                string tableName = SqlHelper.GetTableNameFromInstance(dependantType, true);

                string query = $"SELECT id, date FROM {tableName} WHERE name = @name ORDER BY date";

                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@name", name);

                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        int id = int.Parse(reader["id"].ToString());
                        var date = reader["date"].ToString().CastFromSQLDate();

                        return (IPrimaryKey)Activator.CreateInstance(dependantType, id, name, date);
                    }
            }

            return default;
        }

        private static IPrimaryKey RegisterTableValueAsPrimary(string name, Type type)
        {
            // This method is private because it's call its ensured to be called checking that the org or the user doesn't exists

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            while (Connection.State != ConnectionState.Open)
                Thread.Sleep(100);

            IPrimaryKey retValue;

            bool isOrg = type == typeof(OrgData);
            if (isOrg)
            {
                bool isValid = BlacklistWorker.IsValid(name, BlacklistWorker.Type.Organization);

                // In theory, this will never be called
                if (!isValid)
                    return null;

                if (!BlacklistWorker.IsOrgListable(name))
                    return null;
            }
            else
            {
                bool isValid = BlacklistWorker.IsValid(name, BlacklistWorker.Type.User);

                // In theory, this will never be called
                if (!isValid)
                    return null;

                if (!BlacklistWorker.IsUserListable(name))
                    return null;
            }

            using (var cmd = Connection.CreateCommand())
            {
                DisableForeignKeyChecks(cmd);
                {
                    string tableName = SqlHelper.GetTableNameFromInstance(type, true);

                    string query = $"INSERT INTO {tableName}(id, name, date) VALUES(@id, @name, @date)";

                    DateTime date = DateTime.Now;

                    cmd.Parameters.AddWithValue("@id", 0);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@date", date.ToSQLDate());

                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;

                    cmd.ExecuteNonQuery();
                    retValue = (IPrimaryKey)Activator.CreateInstance(type, (int)cmd.LastInsertedId, name, date);
                }
                EnableForeignKeyChecks(cmd);
            }

            return retValue;
        }

        public static void EnableForeignKeyChecks(MySqlCommand cmd)
            => InternalToggleForeignKeyChecks(cmd, true);

        public static void DisableForeignKeyChecks(MySqlCommand cmd)
            => InternalToggleForeignKeyChecks(cmd, false);

        private static void InternalToggleForeignKeyChecks(MySqlCommand cmd, bool enable)
        {
            string query = "SET FOREIGN_KEY_CHECKS={0}";
            query = string.Format(query, enable ? 1 : 0);

            cmd.CommandText = query;
            cmd.CommandType = CommandType.Text;

            cmd.ExecuteNonQuery();
        }

        public static void IterateRecords<T>(Action<MySqlDataReader> readerCallback)
            where T : IPrimaryKey
        {
            while (Connection.State != ConnectionState.Open)
                Thread.Sleep(100);

            using (var cmd = Connection.CreateCommand())
            {
                string tableName = SqlHelper.GetTableNameFromInstance<T>(true);

                string query = $"SELECT * FROM {tableName}";

                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;

                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        readerCallback?.Invoke(reader);
                    }
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
        public static string DateHandle => "yyyy-MM-dd H:mm:ss";

        public static void InsertInto(this MySqlCommand cmd, List<ColumnData> list, string tableName, bool executeQuery)
        {
            string columnHeader = GetColumns(list);
            string valuesHeader = string.Join(",", list.Select((o, index) => $"@val{index}"));

            string query = $"INSERT INTO {tableName} ({columnHeader}) VALUES ({valuesHeader})";

            {
                int index = 0;
                foreach (var item in list)
                {
                    if (item.MySqlDbType.HasValue && item.DbType.HasValue)
                        cmd.Parameters.Add(new MySqlParameter { ParameterName = $"@val{index}", Value = item.Value, MySqlDbType = item.MySqlDbType.Value, DbType = item.DbType.Value });
                    else
                        cmd.Parameters.Add(new MySqlParameter { ParameterName = $"@val{index}", Value = item.Value });

                    ++index;
                }
            }

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
                    Console.WriteLine($"Query:\n{GetQuery(query, list.Select(i => i.Value).ToArray())}", Color.Yellow);
                }
            }
        }

        private static string GetQuery(string query, params object[] objs)
        {
            for (var index = 0; index < objs.Length; index++)
            {
                var o = objs[index];
                string handle = $"@val{index}";

                query = query.Replace(handle, o == null ? "NULL" : o.ToString());
            }

            return query;
        }

        private static string GetColumns(List<ColumnData> list)
        {
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

        public static string GetTableNameFromInstance<T>(bool mainTable = false)
            where T : IPrimaryKey
        {
            return GetTableNameFromInstance(typeof(T), mainTable);
        }

        public static string GetTableNameFromInstance(Type type, bool mainTable = false)
        {
            return type
                .GetCustomAttributes(typeof(DbTableNameAttribute), false)
                .Cast<DbTableNameAttribute>()
                .First(attr => attr?.MainTable == mainTable)?.Name;
        }

        public static string ToSQLDate(this DateTime date)
        {
            return date.ToString(DateHandle);
        }

        public static DateTime CastFromSQLDate(this string date)
        {
            try
            {
                return DateTime.ParseExact(date, DateHandle, CultureInfo.InvariantCulture);
            }
            catch
            {
                return DateTime.Parse(date);
            }
        }

        public static string GetNow()
        {
            return DateTime.Now.ToSQLDate();
        }

        public class ColumnData
        {
            public int Order { get; }
            public string Name { get; }
            public object Value { get; }
            public MySqlDbType? MySqlDbType { get; }
            public DbType? DbType { get; }

            private ColumnData()
            {
            }

            public ColumnData(int order, string name, object value, MySqlDbType? mySqlDbType, DbType? dbType)
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