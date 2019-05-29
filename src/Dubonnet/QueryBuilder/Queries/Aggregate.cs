using System.Linq;
using Dubonnet.QueryBuilder.Clauses;

namespace Dubonnet.QueryBuilder
{
    public partial class QueryFactory<Q>
    {
        public Q AsAggregate(string type, params string[] columns)
        {
            instance.Method = "aggregate";

            instance.ClearComponent("aggregate")
            .AddComponent("aggregate", new AggregateClause
            {
                Type = type,
                Columns = columns.ToList()
            });

            return instance;
        }

        public Q AsCount(params string[] columns)
        {
            var cols = columns.ToList();

            if (!cols.Any())
            {
                cols.Add("*");
            }

            return instance.AsAggregate("count", cols.ToArray());
        }

        public Q AsAvg(string column)
        {
            return instance.AsAggregate("avg", column);
        }
        
        public Q AsAverage(string column)
        {
            return instance.AsAvg(column);
        }

        public Q AsSum(string column)
        {
            return instance.AsAggregate("sum", column);
        }

        public Q AsMax(string column)
        {
            return instance.AsAggregate("max", column);
        }

        public Q AsMin(string column)
        {
            return instance.AsAggregate("min", column);
        }
    }
}