using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Dapper;
using Dubonnet.QueryBuilder;
using Dubonnet.Attributes;

namespace Dubonnet
{
    public class TableSchema
    {
        public string DB_NAME;
        public string TABLE_NAME;
        public long TABLE_ROWS;
        public long AUTO_INCREMENT;
    }
    
    public class TableColumn
    {
        public string DB_NAME;
        public string TABLE_NAME;
        public string COLUMN_NAME;
        public string COLUMN_TYPE;
        public string DATA_TYPE;
        public string COLUMN_DEFAULT;
        public string IS_NULLABLE;
        public string COLUMN_KEY;
        public string EXTRA;
        public long ORDINAL_POSITION;
    }

    public partial class DubonQuery<M> : QueryFactory<DubonQuery<M>>
    {
        public static readonly ConcurrentDictionary<Type, List<string>> 
            paramNameCache = new ConcurrentDictionary<Type, List<string>>();
        public DubonContext db { get; set; }
        protected string name = "";
        protected string pkey = "";
        
        /// <summary>
        /// The name of table.
        /// </summary>
        /// <returns>The table name.</returns>
        public string CurrentName
        {
            get
            {
                if (name == "")
                {
                    name = db.NameResolver.Resolve(typeof(M));
                }
                return name;
            }
        }
        
        /// <summary>
        /// The name of db driver.
        /// </summary>
        /// <returns>The driver name.</returns>
        public string DriverType
        {
            get
            {
                var driver = db.GetDriverName().ToLower();
                if (driver.Contains("mysql")) {
                    return "mysql";
                } else if (driver.Contains("pgsql")) {
                    return "pgsql";
                } else if (driver.Contains("sqlite")) {
                    return "sqlite";
                } else if (driver.Contains("mssql")) {
                    return "sqlserver";
                } else if (driver.Contains("sqlserver")) {
                    return "sqlserver";
                } else if (driver.Contains("sqlclient")) {
                    return "sqlserver";
                } else {
                    return "unknow";
                }
            }
        }

        /// <summary>
        /// Creates a table in the specified database with a given name.
        /// </summary>
        /// <param name="dbCxt">The database.</param>
        /// <param name="tableName">The name for this table.</param>
        public DubonQuery(DubonContext dbCxt, string tableName = "") : base()
        {
            instance = this;
            db = dbCxt;
            EngineScope = DriverType.Split('.')[0];
            name = tableName;
            From(CurrentName);
        }
        
        public IEnumerable<TableSchema> GetTables(string tableName, bool isDesc = false)
        {
            var name = tableName + "%";
            var rawSql = "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE ? ORDER BY TABLE_NAME";
            switch (DriverType) {
                case "sqlserver":
                    rawSql = "SELECT TABLE_CATALOG as DB_NAME,TABLE_NAME"
                             + " FROM [INFORMATION_SCHEMA].TABLES"
                             + " WHERE TABLE_CATALOG=DB_NAME() AND TABLE_SCHEMA='dbo'"
                             + " AND TABLE_NAME LIKE ? ORDER BY TABLE_NAME";
                    break;
                case "mysql":
                    name = tableName.Replace("_", "\\_") + "%";
                    rawSql = "SELECT TABLE_SCHEMA as DB_NAME,TABLE_NAME,TABLE_ROWS,AUTO_INCREMENT"
                             + " FROM `information_schema`.TABLES"
                             + " WHERE TABLE_SCHEMA=DATABASE()"
                             + " AND TABLE_NAME LIKE ? ORDER BY TABLE_NAME";
                    break;
            }
            if (isDesc)
            {
                rawSql += " DESC";
            }
            var (sql, dict) = instance.CompileSql(rawSql, new object[]{name}, db.Log);
            return db.Conn.Query<TableSchema>(sql, dict);
        }
        
        public IEnumerable<TableColumn> GetColumns(string tableName)
        {
            var rawSql = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=?";
            switch (DriverType) {
                case "sqlserver":
                    rawSql = "SELECT TABLE_CATALOG as DB_NAME,TABLE_NAME,COLUMN_NAME,DATA_TYPE,"
                             + "COLUMN_DEFAULT,IS_NULLABLE,ORDINAL_POSITION"
                             + " FROM [INFORMATION_SCHEMA].COLUMNS"
                             + " WHERE TABLE_CATALOG=DB_NAME() AND TABLE_SCHEMA='dbo'"
                             + " AND TABLE_NAME=? ORDER BY ORDINAL_POSITION";
                    break;
                case "mysql":
                    rawSql = "SELECT TABLE_SCHEMA as DB_NAME,TABLE_NAME,COLUMN_NAME,COLUMN_TYPE,DATA_TYPE,"
                             + "COLUMN_DEFAULT,IS_NULLABLE,COLUMN_KEY,EXTRA,ORDINAL_POSITION"
                             + " FROM `information_schema`.COLUMNS"
                             + " WHERE TABLE_SCHEMA=DATABASE()"
                             + " AND TABLE_NAME=? ORDER BY ORDINAL_POSITION";
                    break;
            }
            var (sql, dict) = instance.CompileSql(rawSql, new object[]{tableName}, db.Log);
            return db.Conn.Query<TableColumn>(sql, dict);
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
