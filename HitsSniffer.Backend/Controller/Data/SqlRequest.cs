using HitsSniffer.Controller.Enums;
using MySql.Data.MySqlClient;

namespace HitsSniffer.Controller.Data
{
    public class SqlRequest
    {
        public string Query { get; }
        public ExecuteQueryAs QueryAs { get; }
        public ExecuteMode QueryMode { get; }
        public MySqlParameter[] Params { get; }

        private SqlRequest()
        {
        }

        public SqlRequest(string query)
            : this(query, ExecuteQueryAs.NonQuery, ExecuteMode.Write)
        {
        }

        public SqlRequest(string query, params MySqlParameter[] @params)
            : this(query, ExecuteQueryAs.NonQuery, ExecuteMode.Write, @params)
        {
        }

        public SqlRequest(string query, ExecuteQueryAs queryAs, ExecuteMode queryMode, params MySqlParameter[] @params)
        {
            Query = query;
            QueryAs = queryAs;
            QueryMode = queryMode;
            Params = @params;
        }

        public static MySqlParameter CreateParameter(string name, object value)
        {
            return new MySqlParameter { ParameterName = name, Value = value };
        }
    }
}