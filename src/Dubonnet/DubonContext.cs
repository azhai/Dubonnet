using System;
using System.Data;
using System.Linq;
using System.Reflection;
using Dubonnet.Abstractions;
using Dubonnet.Resolvers;

namespace Dubonnet
{
    /// <summary>
    /// Simple CRUD operations for Dapper.
    /// </summary>
    public class DubonContext : IDisposable
    {
        protected IDbTransaction _transaction;
        protected IDbConnection _connection;
        public ITableNameResolver NameResolver;
        public IColumnNameResolver ColumnResolver;

        public IDbConnection Conn => _connection;
        
        public Action<string> Log = sql => {
            Console.WriteLine(sql + ";");
        };
        
        /// <summary>
        /// The name of db driver.
        /// </summary>
        /// <returns>The driver name.</returns>
        public string DriverType
        {
            get
            {
                var driver = GetDriverName().ToLower();
                switch (true) // C# 7 的 when 语法
                {
                    case true when driver.Contains("mysql"): 
                    case true when driver.Contains("mariadb"):
                    case true when driver.Contains("percona"):
                    case true when driver.Contains("xtradb"):
                        return "mysql";
                    case true when driver.Contains("pgsql"):
                    case true when driver.Contains("postgres"):
                    case true when driver.Contains("postgresql"):
                        return "postgres";
                    case true when driver.Contains("sqlite"):
                        return "sqlite";
                    case true when driver.Contains("sqlclient"):
                    case true when driver.Contains("mssql"):
                    case true when driver.Contains("sqlsrv"):
                    case true when driver.Contains("sqlserver"):
                        return "sqlsrv";
                    case true when driver.Contains("oracle"):
                    case true when driver.Contains("oracle11g"):
                        return "oracle11g";
                    case true when driver.Contains("firebird"):
                        return "firebird";
                    default:
                        return "unknow";
                }
            }
        }

        public DubonContext(IDbConnection connection, ITableNameResolver resolver = null)
        {
            _connection = connection;
            _connection.Open();
            if (resolver == null)
            {
                resolver = new DefaultTableNameResolver();
            }
            NameResolver = resolver;
        }
        
        public void Dispose()
        {
            if (_connection.State != ConnectionState.Closed)
            {
                _transaction?.Rollback();
                _connection.Close();
                _connection = null;
                NameResolver = null;
            }
        }
        
        public string GetDriverName() {
            return _connection ?. GetType().Namespace;
        }

        public DubonQuery<M> InitTable<M>(string name = "")
        {
            return new DubonQuery<M>(this, name);
        }

        /// <summary>
        /// 找出未赋值的Model，补上InitTable()的值进行初始化
        /// </summary>
        public void InitAllTables()
        {
            Func<FieldInfo, bool> filter =
                f => f.FieldType.Name.StartsWith("DubonQuery") && f.GetValue(this) == null;
            foreach (var f in GetType().GetFields().Where(filter))
            {
                MethodInfo method = GetType().GetMethod("InitTable");
                method = method.MakeGenericMethod(f.FieldType.GenericTypeArguments);
                // Reflection中默认参数不可省略
                f.SetValue(this, method.Invoke(this, new object[]{""}));
            }
        }

        /// <summary>
        ///     Begins a transaction in this database.
        /// </summary>
        /// <param name="isolation">The isolation level to use.</param>
        public void BeginTxn(IsolationLevel isolation = IsolationLevel.ReadCommitted)
        {
            _transaction = _connection.BeginTransaction(isolation);
        }

        /// <summary>
        ///     Commits the current transaction in this database.
        /// </summary>
        public void CommitTxn()
        {
            _transaction.Commit();
            _transaction = null;
        }

        /// <summary>
        ///     Rolls back the current transaction in this database.
        /// </summary>
        public void RollbackTxn()
        {
            _transaction.Rollback();
            _transaction = null;
        }
    }
}
