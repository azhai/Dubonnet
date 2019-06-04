using System;
using System.Collections.Generic;
using Dapper;

namespace Dubonnet
{
    public interface ITableCounter
    {
        void SetTableCount(string tableName, long count);
        long GetTableCount(string tableName);
        bool Contains(string tableName);
        List<string> GetNames();
    }

    public class TableCountDict : ITableCounter
    {
        private Dictionary<string, long> tableCounts;

        public TableCountDict()
        {
            tableCounts = new Dictionary<string, long>();
        }

        public void SetTableCount(string tableName, long count)
        {
            tableCounts[tableName] = count;
        }

        public long GetTableCount(string tableName)
        {
            return Contains(tableName) ? tableCounts[tableName] : -1;
        }

        public bool Contains(string tableName)
        {
            return tableCounts.ContainsKey(tableName);
        }

        public List<string> GetNames()
        {
            return tableCounts.Keys.AsList();
        }
    }

    /// <summary>
    /// A database query of archives or monthly.
    /// </summary>
    public partial class DubonQuery<M>
    {
        public const int COUMT_IS_EMPTY = -1;   // 尚未计数
        public const int COUMT_IS_DYNAMIC = -2; // 不缓存计数

        public ITableCounter tableCounter { get; set; }
        public Func<string, bool> tableFilter { get; set; }

        /// <summary>
        /// 获取符合条件的表名，并统计每张表符合条件的行数
        /// </summary>
        /// <param name="reload">重新读取符合条件的表名</param>
        /// <returns></returns>
        protected List<string> filterShardingNames(bool reload = false)
        {
            var tables = new List<string>();
            if (tableCounter == null)
            {
                reload = true;
                tableCounter = new TableCountDict();
            }
            if (reload)
            {
                foreach (var schema in GetTables(CurrentName))
                {
                    tables.Add(schema.TABLE_NAME);
                }
            }
            else
            {
                tables = tableCounter.GetNames();
            }
            
            foreach (var tableName in tables)
            {
                if (tableCounter.Contains(tableName))
                {
                    continue;
                }
                if (tableFilter == null || tableFilter(tableName))
                {
                    tableCounter.SetTableCount(tableName, COUMT_IS_EMPTY);
                }
            }
            return tables;
        }
        
        /// <summary>
        /// Count a table
        /// </summary>
        protected long getShardingCount(string tableName, bool check = false)
        {
            if (check && !tableCounter.Contains(tableName))
            {
                return 0;
            }
            var count = tableCounter.GetTableCount(tableName);
            if (count < 0)
            {
                var oldCount = count;
                count = Clone(tableName).Count();
                if (COUMT_IS_EMPTY == oldCount)
                {
                    tableCounter.SetTableCount(tableName, count);
                }
            }
            return count;
        }

        /// <summary>
        /// Count all
        /// </summary>
        public long CountSharding()
        {
            long count = 0;
            foreach (var tableName in filterShardingNames())
            {
                count += getShardingCount(tableName);
            }
            return count;
        }
        
        /// <summary>
        /// Select step by step.
        /// </summary>
        public List<M> PaginateSharding(long page = 1, long size = 100)
        {
            if (page <= 0)
            {
                throw new ArgumentException("Param 'page' is out of range", nameof(page));
            }
            if (size <= 0)
            {
                throw new ArgumentException("Param 'size' should be greater than 0", nameof(size));
            }
            var offset = (page - 1) * size;
            var result = new List<M>();
            foreach (var tableName in filterShardingNames())
            {
                From(tableName);
                offset -= getShardingCount(tableName);
                if (offset >= 0)
                {
                    continue;
                }
                var remain = size - result.Count;
                var rows = Clone().Limit((int)remain).All();
                result.AddRange(rows);
                if (result.Count >= size)
                {
                    break;
                }
            }
            return result;
        }
    }
}
