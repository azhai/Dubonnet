using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dubonnet.QueryBuilder.Clauses;

namespace Dubonnet.QueryBuilder
{
    public partial class QueryFactory<Q>
    {
        public Q AsInsert(object data, bool returnId = false)
        {
            var dictionary = new Dictionary<string, object>();

            var props = data.GetType().GetRuntimeProperties();

            foreach (var item in props)
            {
                dictionary.Add(item.Name, item.GetValue(data));
            }

            return instance.AsInsert(dictionary, returnId);
        }

        public Q AsInsert(IEnumerable<string> columns, IEnumerable<object> values)
        {
            var columnsList = columns?.ToList();
            var valuesList = values?.ToList();

            if ((columnsList?.Count ?? 0) == 0 || (valuesList?.Count ?? 0) == 0)
            {
                throw new InvalidOperationException("Columns and Values cannot be null or empty");
            }

            if (columnsList.Count != valuesList.Count)
            {
                throw new InvalidOperationException("Columns count should be equal to Values count");
            }

            instance.Method = "insert";

            instance.ClearComponent("insert").AddComponent("insert", new InsertClause
            {
                Columns = columnsList,
                Values = valuesList
            });

            return instance;
        }

        public Q AsInsert(IReadOnlyDictionary<string, object> data, bool returnId = false)
        {
            if (data == null || data.Count == 0)
            {
                throw new InvalidOperationException("Values dictionary cannot be null or empty");
            }

            instance.Method = "insert";

            instance.ClearComponent("insert").AddComponent("insert", new InsertClause
            {
                Columns = data.Keys.ToList(),
                Values = data.Values.ToList(),
                ReturnId = returnId,
            });

            return instance;
        }

        /// <summary>
        /// Produces insert multi records
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="valuesCollection"></param>
        /// <returns></returns>
        public Q AsInsert(IEnumerable<string> columns, IEnumerable<IEnumerable<object>> valuesCollection, bool returnId = false)
        {
            var columnsList = columns?.ToList();
            var valuesCollectionList = valuesCollection?.ToList();

            if ((columnsList?.Count ?? 0) == 0 || (valuesCollectionList?.Count ?? 0) == 0)
            {
                throw new InvalidOperationException("Columns and valuesCollection cannot be null or empty");
            }

            instance.Method = "insert";

            instance.ClearComponent("insert");

            foreach (var values in valuesCollectionList)
            {
                var valuesList = values.ToList();
                if (columnsList.Count != valuesList.Count)
                {
                    throw new InvalidOperationException("Columns count should be equal to each Values count");
                }

                instance.AddComponent("insert", new InsertClause
                {
                    Columns = columnsList,
                    Values = valuesList,
                    ReturnId = returnId,
                });
            }

            return instance;
        }

        /// <summary>
        /// Produces insert from subquery
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public Q AsInsert(IEnumerable<string> columns, Q factory)
        {
            instance.Method = "insert";

            instance.ClearComponent("insert").AddComponent("insert", new InsertQueryClause<Q>
            {
                Columns = columns.ToList(),
                Query = factory.Clone(),
            });

            return instance;
        }

    }
}