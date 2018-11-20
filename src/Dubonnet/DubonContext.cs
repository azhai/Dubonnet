using System;
using System.Data;
using System.Data.Common;
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
        public Action<string> Log = sql => {
            Console.WriteLine(sql + ";");
        };

        public IDbConnection Conn => _connection;

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
            return _connection?.GetType().Namespace;
        }

        public DubonQuery<M> InitTable<M>(string name = "")
        {
            return new DubonQuery<M>(this, name);
        }

        public void InitAllTables()
        {
            /*
            Func<FieldInfo, bool> filter =
                f => f.FieldType.Name.StartsWith("DubonQuery") && f.GetValue(this) == null;
            foreach (var f in GetType().GetFields().Where(filter))
            {
            }
            */
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
