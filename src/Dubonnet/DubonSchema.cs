using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Dapper;
using Dubonnet.Attributes;

namespace Dubonnet
{
    public class BaseSchema
    {
        public string TABLE_CATALOG;
        public string TABLE_SCHEMA;
        public string TABLE_NAME;

        public string Engine()
        {
            if ("dbo" == TABLE_SCHEMA)
            {
                return "sqlsrv";
            }
            if ("def" == TABLE_CATALOG || "" == TABLE_CATALOG)
            {
                return "mysql";
            }
            return "";
        }

        public string DbName(string engine = "")
        {
            if (string.IsNullOrEmpty(engine))
            {
                engine = Engine();
            }
            if ("mysql" == engine)
            {
                return TABLE_SCHEMA;
            }
            return TABLE_CATALOG;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(TABLE_NAME))
            {
                return "";
            }
            string fullname = TABLE_NAME;
            if (!string.IsNullOrEmpty(TABLE_SCHEMA))
            {
                fullname = TABLE_SCHEMA + "." + fullname;
            }
            if (!string.IsNullOrEmpty(TABLE_CATALOG) &&　TABLE_CATALOG != "def")
            {
                fullname = TABLE_CATALOG + "." + fullname;
            }
            return fullname;
        }
    }
    
    public class TableSchema : BaseSchema
    {
        public long TABLE_ROWS;
        public long AUTO_INCREMENT;
    }
    
    public class ColumnSchema : BaseSchema
    {
        public string COLUMN_NAME;
        public string COLUMN_TYPE;
        public string DATA_TYPE;
        public string COLUMN_DEFAULT;
        public string IS_NULLABLE;
        public string COLUMN_KEY;
        public string EXTRA;
        public long ORDINAL_POSITION;
    }

    public partial class DubonQuery<M>
    {
        public static readonly ConcurrentDictionary<string, List<(string table, string dbname)>>
            tableNameCache = new ConcurrentDictionary<string, List<(string table, string dbname)>>();
        public static readonly ConcurrentDictionary<Type, List<string>> 
            paramNameCache = new ConcurrentDictionary<Type, List<string>>();

        public static string GetDbCond(string engine, string dbName = "")
        {
            if ("" == dbName)
            {
                return "";
            }
            if (false == dbName.EndsWith("()"))
            {
                dbName = "'" + dbName + "'";
            }
            switch (engine) {
                case "sqlsrv":
                    return " TABLE_CATALOG=" + dbName + " AND TABLE_SCHEMA='dbo' AND";
                case "mysql":
                    return " TABLE_SCHEMA=" + dbName + " AND ";
                default:
                    return " (TABLE_CATALOG=" + dbName + " OR TABLE_SCHEMA=" + dbName + ") AND";
            }
        }

        public string GetDbName(bool onlyFunc = false)
        {
            var funcName = "";
            switch (db.DriverType) {
                case "sqlsrv":
                    funcName = "DB_NAME()";
                    break;
                case "mysql":
                    funcName = "DATABASE()";
                    break;
                case "pgsql":
                case "firebird":
                    funcName = "CURRENT_DATABASE()";
                    break;
            }
            if (onlyFunc && "" != funcName)
            {
                return funcName;
            }
            var rawSql = "SELECT " + funcName;
            switch (db.DriverType) {
                case "sqlite":
                    rawSql = "PRAGMA database_list";
                    break;
                case "oracle":
                    rawSql = "SELECT name FROM V$database";
                    break;
            }
            instance.Log(rawSql);
            return db.Conn.ExecuteScalar<string>(rawSql);
        }

        public IEnumerable<TableSchema> GetTables(string tableName, string dbName = "", bool isDesc = false)
        {
            tableName = tableName + "%";
            var rawSql = "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE ? ORDER BY TABLE_NAME";
            switch (db.DriverType) {
                case "sqlsrv":
                    rawSql = "SELECT TABLE_CATALOG,TABLE_SCHEMA,TABLE_NAME"
                             + " FROM [INFORMATION_SCHEMA].TABLES"
                             + " WHERE" + GetDbCond(db.DriverType, dbName)
                             + " TABLE_NAME LIKE ? ORDER BY TABLE_NAME";
                    break;
                case "mysql":
                    tableName = tableName.Replace("_", "\\_");
                    rawSql = "SELECT TABLE_CATALOG,TABLE_SCHEMA,TABLE_NAME,TABLE_ROWS,AUTO_INCREMENT"
                             + " FROM `information_schema`.TABLES"
                             + " WHERE" + GetDbCond(db.DriverType, dbName)
                             + " TABLE_NAME LIKE ? ORDER BY TABLE_NAME";
                    break;
            }
            if (isDesc)
            {
                rawSql += " DESC";
            }
            var (sql, dict) = instance.CompileSql(rawSql, new object[]{tableName});
            return db.Conn.Query<TableSchema>(sql, dict);
        }
        
        public IEnumerable<ColumnSchema> GetColumns(string tableName, string dbName = "")
        {
            var rawSql = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=?";
            switch (db.DriverType) {
                case "sqlsrv":
                    rawSql = "SELECT TABLE_CATALOG,TABLE_SCHEMA,TABLE_NAME,COLUMN_NAME,DATA_TYPE,"
                             + "COLUMN_DEFAULT,IS_NULLABLE,ORDINAL_POSITION"
                             + " FROM [INFORMATION_SCHEMA].COLUMNS"
                             + " WHERE" + GetDbCond(db.DriverType, dbName)
                             + " TABLE_NAME=? ORDER BY ORDINAL_POSITION";
                    break;
                case "mysql":
                    rawSql = "SELECT TABLE_CATALOG,TABLE_SCHEMA,TABLE_NAME,COLUMN_NAME,COLUMN_TYPE,DATA_TYPE,"
                             + "COLUMN_DEFAULT,IS_NULLABLE,COLUMN_KEY,EXTRA,ORDINAL_POSITION"
                             + " FROM `information_schema`.COLUMNS"
                             + " WHERE" + GetDbCond(db.DriverType, dbName)
                             + " TABLE_NAME=? ORDER BY ORDINAL_POSITION";
                    break;
            }
            var (sql, dict) = instance.CompileSql(rawSql, new object[]{tableName});
            return db.Conn.Query<ColumnSchema>(sql, dict);
        }

        public List<(string table, string dbname)> ListTable(string name, bool refresh = false)
        {
            if (refresh || !tableNameCache.TryGetValue(name, out List<(string table, string dbname)> tables))
            {
                tables = new List<(string table, string dbname)>();
                foreach (var s in GetTables(name, GetDbName(true)))
                {
                    string dbname = s.DbName(s.Engine());
                    tables.Add((table:s.TABLE_NAME, dbname));
                }
                tableNameCache[name] = tables;
            }
            return tables;
        }

        /// <summary>
        /// Get and cache the column's names
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        internal static List<string> GetParamNames(object o)
        {
            if (o is DynamicParameters parameters)
            {
                return parameters.ParameterNames.ToList();
            }

            if (!paramNameCache.TryGetValue(o.GetType(), out List<string> paramNames))
            {
                paramNames = new List<string>();
                var flags = BindingFlags.Instance | BindingFlags.Public;
                Func<PropertyInfo, bool> filter = p => p.GetGetMethod(false) != null;
                foreach (var prop in o.GetType().GetProperties(flags).Where(filter))
                {
                    var attribs = prop.GetCustomAttributes(typeof(IgnorePropertyAttribute), true);
                    var attr = attribs.FirstOrDefault() as IgnorePropertyAttribute;
                    if (attr == null || (!attr.Value))
                    {
                        paramNames.Add(prop.Name);
                    }
                }
                paramNameCache[o.GetType()] = paramNames;
            }
            return paramNames;
        }
    }
}
