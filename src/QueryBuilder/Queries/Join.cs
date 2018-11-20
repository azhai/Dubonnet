using System;
using Dubonnet.QueryBuilder.Clauses;

namespace Dubonnet.QueryBuilder
{
    public partial class QueryFactory<Q>
    {

        private Q Join(Func<JoinBuilder<Q>, JoinBuilder<Q>> callback)
        {
            var join = callback.Invoke(new JoinBuilder<Q>().AsInner());

            return instance.AddComponent("join", new BaseJoin<Q>
            {
                Join = join
            });
        }

        public Q Join(
            string table,
            string first,
            string second,
            string op = "=",
            string type = "inner join"
        )
        {
            return instance.Join(j => j.JoinWith(table).WhereColumns(first, op, second).AsType(type));
        }

        public Q Join(string table, Func<JoinBuilder<Q>, JoinBuilder<Q>> callback, string type = "inner join")
        {
            return instance.Join(j => j.JoinWith(table).Where(callback).AsType(type));
        }

        public Q Join(JoinBuilder<Q> factory, Func<JoinBuilder<Q>, JoinBuilder<Q>> onCallback, string type = "inner join")
        {
            return instance.Join(j => j.JoinWith(factory).Where(onCallback).AsType(type));
        }

        public Q LeftJoin(string table, string first, string second, string op = "=")
        {
            return instance.Join(table, first, second, op, "left join");
        }

        public Q LeftJoin(string table, Func<JoinBuilder<Q>, JoinBuilder<Q>> callback)
        {
            return instance.Join(table, callback, "left join");
        }

        public Q LeftJoin(JoinBuilder<Q> factory, Func<JoinBuilder<Q>, JoinBuilder<Q>> onCallback)
        {
            return instance.Join(factory, onCallback, "left join");
        }

        public Q RightJoin(string table, string first, string second, string op = "=")
        {
            return instance.Join(table, first, second, op, "right join");
        }

        public Q RightJoin(string table, Func<JoinBuilder<Q>, JoinBuilder<Q>> callback)
        {
            return instance.Join(table, callback, "right join");
        }

        public Q RightJoin(JoinBuilder<Q> factory, Func<JoinBuilder<Q>, JoinBuilder<Q>> onCallback)
        {
            return instance.Join(factory, onCallback, "right join");
        }

        public Q CrossJoin(string table)
        {
            return instance.Join(j => j.JoinWith(table).AsCross());
        }

    }
}