using System;
using System.Collections.Generic;
using Dapper;

namespace Dubonnet
{
    public class InsertGetIdRow<T>
    {
        public T Id { get; set; }
    }

    /// <summary>
    /// A database query of type <typeparamref name="M"/>.
    /// </summary>
    /// <typeparam name="M">The type of object in this table.</typeparam>
    public partial class DubonQuery<M>
    {
        /// <summary>
        /// The primary key of table.
        /// </summary>
        /// <returns>The primary key name.</returns>
        public string PrimaryKey
        {
            get
            {
                if (pkey == "")
                {
                    pkey = "id";
                }
                return pkey;
            }
        }
        
        /// <summary>
        /// Create another Q for Clone()
        /// </summary>
        /// <returns></returns>
        public override DubonQuery<M> NewQuery()
        {
            return new DubonQuery<M>(db, name);
        }

        /// <summary>
        /// Clone this
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public DubonQuery<M> Clone(string tableName = "")
        {
            var query = base.Clone();
            query.db = db;
            query.tableCounts = tableCounts;
            query.tableFilter = tableFilter;
            if (tableName != "")
            {
                query.From(tableName);
            }
            foreach (var c in query.GetComponents("where"))
            {
                query.AddComponent("where", c);
            }
            foreach (var c in query.GetComponents("select"))
            {
                query.AddComponent("select", c);
            }
            return query;
        }
        
        /// <summary>
        /// Gets the all rows from this table.
        /// </summary>
        /// <returns>Data from all table rows.</returns>
        public IEnumerable<M> All()
        {
            var (sql, dict) = instance.CompileSql(db.Log);
            return db.Conn.Query<M>(sql, dict);
        }

        /// <summary>
        /// QueryFirstOrDefault Select.
        /// </summary>
        /// <returns>The first row or default row of table.</returns>
        public M Get<K>(K id)
        {
            if (id != null)
            {
                Where(PrimaryKey, id);
            }
            var (sql, dict) = instance.CompileSql(db.Log);
            return db.Conn.QueryFirstOrDefault<M>(sql, dict);
        }
        
        /// <summary>
        /// QueryFirstOrDefault when pkey is int
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The first row or default row of factory.</returns>
        public M Get(int id)
        {
            return Get<int>(id);
        }
        
        /// <summary>
        /// QueryFirstOrDefault when pkey is string
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The first row or default row of factory.</returns>
        public M Get(string id = null)
        {
            return Get<string>(id);
        }

        /// <summary>
        /// QueryFirst Select.
        /// </summary>
        /// <returns>The first row of table.</returns>
        public M First(string order = "")
        {
            OrderBy(order != "" ? order : PrimaryKey).Limit(1);
            var (sql, dict) = instance.CompileSql(db.Log);
            return db.Conn.QueryFirst<M>(sql, dict);
        }

        /// <summary>
        /// QueryFirst Select.
        /// </summary>
        /// <returns>The first row of table.</returns>
        public M Last(string order = "")
        {
            OrderByDesc(order != "" ? order : PrimaryKey).Limit(1);
            var (sql, dict) = instance.CompileSql(db.Log);
            return db.Conn.QueryFirst<M>(sql, dict);
        }
        
        /// <summary>
        /// Select Count(*) or Select Count(...).
        /// </summary>
        public long Count(params string[] args)
        {
            AsCount(args);
            var (sql, dict) = instance.CompileSql(db.Log);
            return db.Conn.ExecuteScalar<long>(sql, dict);
        }
        
        /// <summary>
        /// Select X(y, ...), for example: Sum(), Max(), Min(), AVG() etc.
        /// </summary>
        public M Agger(string name, params string[] args)
        {
            AsAggregate(name, args);
            var (sql, dict) = instance.CompileSql(db.Log);
            return db.Conn.ExecuteScalar<M>(sql, dict);
        }
        
        /// <summary>
        /// Select step by step.
        /// </summary>
        public int Chunk(int size, Func<IEnumerable<M>, int, bool> func)
        {
            if (size <= 0)
            {
                throw new ArgumentException("Param 'size' should be greater than 0", nameof(size));
            }
            var count = Clone().Count();
            Limit(size);
            var (i, page) = (0, 0);
            while (i < count)
            {
                var rows = Clone().Offset(i).All();
                page ++;
                i += size;
                if (!func(rows, page))
                {
                    break;
                }
            }
            return page; // Max page no
        }

        /// <summary>
        /// Insert into.
        /// </summary>
        public T InsertGetId<T>(object data)
        {
            AsInsert(data);
            var (sql, dict) = instance.CompileSql(db.Log);
            var row = db.Conn.QueryFirst<InsertGetIdRow<T>>(sql, dict);
            return row.Id;
        }

        /// <summary>
        /// Insert into.
        /// </summary>
        public int Insert(object data)
        {
            AsInsert(data);
            var (sql, dict) = instance.CompileSql(db.Log);
            return db.Conn.ExecuteScalar<int>(sql, dict);
        }

        /// <summary>
        /// Insert into.
        /// </summary>
        public int Insert(IReadOnlyDictionary<string, object> values)
        {
            AsInsert(values);
            var (sql, dict) = instance.CompileSql(db.Log);
            return db.Conn.ExecuteScalar<int>(sql, dict);
        }

        /// <summary>
        /// Insert into.
        /// </summary>
        public int Insert(IEnumerable<string> columns, IEnumerable<IEnumerable<object>> valuesCollection)
        {
            AsInsert(columns, valuesCollection);
            var (sql, dict) = instance.CompileSql(db.Log);
            return db.Conn.ExecuteScalar<int>(sql, dict);
        }

        /// <summary>
        /// Insert into.
        /// </summary>
        public int Insert(IEnumerable<string> columns, DubonQuery<M> fromQuery)
        {
            AsInsert(columns, fromQuery);
            var (sql, dict) = instance.CompileSql(db.Log);
            return db.Conn.ExecuteScalar<int>(sql, dict);
        }

        /// <summary>
        /// Update.
        /// </summary>
        public int Update(object data)
        {
            AsUpdate(data);
            var (sql, dict) = instance.CompileSql(db.Log);
            return db.Conn.ExecuteScalar<int>(sql, dict);
        }

        /// <summary>
        /// Update.
        /// </summary>
        public int Update(IReadOnlyDictionary<string, object> values)
        {
            AsUpdate(values);
            var (sql, dict) = instance.CompileSql(db.Log);
            return db.Conn.ExecuteScalar<int>(sql, dict);
        }

        /// <summary>
        /// Delete.
        /// </summary>
        public int Delete()
        {
            AsDelete();
            var (sql, dict) = instance.CompileSql(db.Log);
            return db.Conn.ExecuteScalar<int>(sql, dict);
        }
    }
}
