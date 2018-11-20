using System;
using System.Collections.Generic;
using Dapper;

namespace Dubonnet
{ 
    /// <summary>
    /// A database query of archives or monthly.
    /// </summary>
    public partial class DubonQuery<M>
    {
        protected Dictionary<string, long> tableCounts;
        public Func<string, bool> tableFilter { get; set; }

        protected List<string> filterShardingNames()
        {
            if (tableCounts == null)
            {
                var tableName = "";
                tableCounts = new Dictionary<string, long>();
                foreach (var schema in GetTables(CurrentName))
                {
                    tableName = schema.TABLE_NAME;
                    if (tableFilter == null || tableFilter(tableName))
                    {
                        tableCounts[tableName] = -1;
                    }
                }
            }
            return tableCounts.Keys.AsList();
        }
        
        protected long getShardingCount(string tableName, bool check = false)
        {
            if (tableCounts == null)
            {
                filterShardingNames();
            }
            if (check && !tableCounts.ContainsKey(tableName))
            {
                return 0;
            }
            if (tableCounts[tableName] < 0)
            {
                tableCounts[tableName] = Clone(tableName).Count();
            }
            return tableCounts[tableName];
        }

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
            if (size <= 0)
            {
                throw new ArgumentException("Param 'size' should be greater than 0", nameof(size));
            }
            var offset = (page - 1) * size;
            if (offset < 0)
            {
                offset += CountSharding();
            }
            if (offset < 0)
            {
                throw new ArgumentException("Param 'page' is out of range", nameof(page));
            }
            
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
