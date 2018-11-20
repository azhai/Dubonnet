using System;
using System.Linq;
using Dubonnet.QueryBuilder.Clauses;

namespace Dubonnet.QueryBuilder
{
    public partial class QueryFactory<Q> 
    {

        public Q Combine(string operation, bool all, Q factory)
        {
            if (instance.Method != "select" || factory.Method != "select")
            {
                throw new InvalidOperationException("Only select queries can be combined.");
            }

            return instance.AddComponent("combine", new Combine<Q>
            {
                Query = factory,
                Operation = operation,
                All = all,
            });
        }

        public Q CombineRaw(string sql, params object[] bindings)
        {
            if (instance.Method != "select")
            {
                throw new InvalidOperationException("Only select queries can be combined.");
            }

            return instance.AddComponent("combine", new RawCombine
            {
                Expression = sql,
                Bindings = Helper.Flatten(bindings).ToArray(),
            });
        }

        public Q Union(Q factory, bool all = false)
        {
            return instance.Combine("union", all, factory);
        }

        public Q UnionAll(Q factory)
        {
            return instance.Union(factory, true);
        }

        public Q Union(Func<Q, Q> callback, bool all = false)
        {
            var query = callback.Invoke(NewQuery());
            return instance.Union(query, all);
        }

        public Q UnionAll(Func<Q, Q> callback)
        {
            return instance.Union(callback, true);
        }

        public Q UnionRaw(string sql, params object[] bindings) => instance.CombineRaw(sql, bindings);

        public Q Except(Q factory, bool all = false)
        {
            return instance.Combine("except", all, factory);
        }

        public Q ExceptAll(Q factory)
        {
            return instance.Except(factory, true);
        }

        public Q Except(Func<Q, Q> callback, bool all = false)
        {
            var query = callback.Invoke(NewQuery());
            return instance.Except(query, all);
        }

        public Q ExceptAll(Func<Q, Q> callback)
        {
            return instance.Except(callback, true);
        }
        public Q ExceptRaw(string sql, params object[] bindings) => instance.CombineRaw(sql, bindings);

        public Q Intersect(Q factory, bool all = false)
        {
            return instance.Combine("intersect", all, factory);
        }

        public Q IntersectAll(Q factory)
        {
            return instance.Intersect(factory, true);
        }

        public Q Intersect(Func<Q, Q> callback, bool all = false)
        {
            var query = callback.Invoke(NewQuery());
            return instance.Intersect(query, all);
        }

        public Q IntersectAll(Func<Q, Q> callback)
        {
            return instance.Intersect(callback, true);
        }
        public Q IntersectRaw(string sql, params object[] bindings) => instance.CombineRaw(sql, bindings);

    }
}