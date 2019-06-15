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
        private SortedDictionary<string, long> tableCounts;

        public TableCountDict()
        {
            tableCounts = new SortedDictionary<string, long>();
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

        public string DbNameRange = "";
        public bool IsTableNameDesc = false;
        public ITableCounter tableCounter { get; set; }
        public Func<string, string, bool> tableFilter { get; set; }

        /// <summary>
        /// 获取符合条件的表名，并统计每张表符合条件的行数
        /// </summary>
        /// <param name="reload">重新读取符合条件的表名</param>
        /// <returns></returns>
        protected List<string> filterShardingNames(bool reload = false)
        {
            List<string> tables;
            if (reload || tableCounter == null)
            {
                tableCounter = new TableCountDict();
                tables = new List<string>();
                foreach (var table in ListTable(CurrentName, DbNameRange))
                {
                    if (tableFilter == null || tableFilter(table.TABLE_NAME, table.DbName()))
                    {
                        var tableName = table.ToString();
                        tables.Add(tableName);
                        tableCounter.SetTableCount(tableName, COUMT_IS_EMPTY);
                    }
                }
            }
            else
            {
                tables = tableCounter.GetNames();
            }
            if (IsTableNameDesc)
            {
                tables.Reverse();
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
        public List<M> PaginateSharding(int page = 1, int size = 100, int oriention = 0)
        {
            if (page <= 0)
            {
                throw new ArgumentException("Param 'page' is out of range", nameof(page));
            }
            if (size <= 0)
            {
                throw new ArgumentException("Param 'size' should be greater than 0", nameof(size));
            }
            if (0 != oriention)
            {
                IsTableNameDesc = oriention < 0;
            }

            long offset = (page - 1) * size;
            var result = new List<M>();
            foreach (var tableName in filterShardingNames())
            {
                var count = getShardingCount(tableName);
                if (offset >= count)
                {
                    offset -= count;
                    continue;
                }
                var remain = size - result.Count;
                var query = Clone(tableName).Limit(remain);
                if (offset > 0)
                {
                    query.Offset((int)offset);
                }
                offset = 0; // 后续查询不需要偏移了
                result.AddRange(query.All());
                if (result.Count >= size)
                {
                    break;
                }
            }
            return result;
        }
    }
}
