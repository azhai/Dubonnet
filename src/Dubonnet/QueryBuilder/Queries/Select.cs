using System;
using System.Linq;
using Dubonnet.QueryBuilder.Clauses;

namespace Dubonnet.QueryBuilder
{
    public partial class QueryFactory<Q>
    {
        public Q Select(params string[] columns)
        {
            instance.Method = "select";

            columns = columns
                .Select(x => Helper.ExpandExpression(x))
                .SelectMany(x => x)
                .ToArray();


            foreach (var column in columns)
            {
                instance.AddComponent("select", new Column
                {
                    Name = column
                });
            }

            return instance;
        }

        /// <summary>
        /// Add a new "raw" select expression to the factory.
        /// </summary>
        /// <returns></returns>
        public Q SelectRaw(string expression, params object[] bindings)
        {
            instance.Method = "select";

            instance.AddComponent("select", new RawColumn
            {
                Expression = expression,
                Bindings = Helper.Flatten(bindings).ToArray()
            });

            return instance;
        }

        public Q Select(Q factory, string alias)
        {
            instance.Method = "select";

            factory = factory.Clone();

            instance.AddComponent("select", new QueryColumn<Q>
            {
                Query = factory.As(alias),
            });

            return instance;
        }

        public Q Select(Func<Q, Q> callback, string alias)
        {
            return instance.Select(callback.Invoke(instance.NewChild()), alias);
        }

        public Q WhereExists(Q factory)
        {
            if (!factory.HasComponent("from"))
            {
                throw new ArgumentException("'FromClause' cannot be empty if used inside a 'WhereExists' condition");
            }

            // remove unneeded components
            factory = factory.Clone().ClearComponent("select");
            factory = factory.SelectRaw("1").Limit(1);

            return AddComponent("where", new ExistsCondition<Q>
            {
                Query = factory,
                IsNot = GetNot(),
                IsOr = GetOr(),
            });
        }
        
        public Q WhereExists(Func<Q, Q> callback)
        {
            var childQuery = NewQuery().SetParent(instance);
            return WhereExists(callback.Invoke(childQuery));
        }

        public Q WhereNotExists(Q factory)
        {
            return Not().WhereExists(factory);
        }

        public Q WhereNotExists(Func<Q, Q> callback)
        {
            return Not().WhereExists(callback);
        }

        public Q OrWhereExists(Q factory)
        {
            return Or().WhereExists(factory);
        }
        public Q OrWhereExists(Func<Q, Q> callback)
        {
            return Or().WhereExists(callback);
        }
        public Q OrWhereNotExists(Q factory)
        {
            return Or().Not().WhereExists(factory);
        }
        public Q OrWhereNotExists(Func<Q, Q> callback)
        {
            return Or().Not().WhereExists(callback);
        }
    }
}