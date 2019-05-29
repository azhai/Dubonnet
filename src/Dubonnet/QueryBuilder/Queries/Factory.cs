using System;
using System.Collections.Generic;
using System.Linq;
using Dubonnet.QueryBuilder.Clauses;
using Dubonnet.QueryBuilder.Compilers;

namespace Dubonnet.QueryBuilder
{
    public partial class QueryFactory<Q> : QueryBase<Q> where Q: QueryFactory<Q>
    {
        protected Compiler<Q> _compiler;
        public bool IsDistinct { get; set; } = false;
        public string QueryAlias { get; set; }
        public string Method { get; set; } = "select";

        protected List<string> operators = new List<string> {
            "=", "<", ">", "<=", ">=", "<>", "!=", "<=>",
            "like", "like binary", "not like", "ilike",
            "&", "|", "^", "<<", ">>",
            "rlike", "regexp", "not regexp",
            "~", "~*", "!~", "!~*", "similar to",
            "not similar to", "not ilike", "~~*", "!~~*",
        };

        public QueryFactory() : base()
        {
        }

        public QueryFactory(string table) : base()
        {
            instance.From(table);
        }

        public bool HasOffset(string engineCode = null)
        {
            var limitOffset = instance.GetOneComponent<LimitOffset>("limit", engineCode);

            return limitOffset?.HasOffset() ?? false;
        }

        public bool HasLimit(string engineCode = null)
        {
            var limitOffset = instance.GetOneComponent<LimitOffset>("limit", engineCode);

            return limitOffset?.HasLimit() ?? false;
        }

        internal int GetOffset(string engineCode = null)
        {
            var limitOffset = instance.GetOneComponent<LimitOffset>("limit", engineCode);

            return limitOffset?.Offset ?? 0;
        }

        internal int GetLimit(string engineCode = null)
        {
            var limitOffset = instance.GetOneComponent<LimitOffset>("limit", engineCode);

            return limitOffset?.Limit ?? 0;
        }

        public override Q Clone()
        {
            var instance = base.Clone();
            instance.QueryAlias = QueryAlias;
            instance.IsDistinct = IsDistinct;
            instance.Method = Method;
            return instance;
        }

        public Q As(string alias)
        {
            instance.QueryAlias = alias;
            return instance;
        }

        public Q With(Q factory)
        {
            // Clear factory alias and add it to the containing clause
            if (string.IsNullOrWhiteSpace(factory.QueryAlias))
            {
                throw new InvalidOperationException("No Alias found for the CTE factory");
            }

            factory = factory.Clone();

            var alias = factory.QueryAlias.Trim();

            // clear the factory alias
            factory.QueryAlias = null;

            return AddComponent("cte", new QueryFromClause<Q>
            {
                Query = factory,
                Alias = alias,
            });
        }

        public Q With(string alias, Q factory)
        {
            return With(factory.As(alias));
        }

        public Q With(Func<Q, Q> fn)
        {
            var query = NewQuery();
            return With(fn.Invoke(query));
        }

        public Q With(string alias, Func<Q, Q> fn)
        {
            var query = NewQuery();
            return With(alias, fn.Invoke(query));
        }

        public Q WithRaw(string alias, string sql, params object[] bindings)
        {
            return AddComponent("cte", new RawFromClause
            {
                Alias = alias,
                Expression = sql,
                Bindings = Helper.Flatten(bindings).ToArray(),
            });
        }

        public Q Limit(int value)
        {
            var clause = instance.GetOneComponent("limit", EngineScope) as LimitOffset;

            if (clause != null)
            {
                clause.Limit = value;
                return instance;
            }

            return instance.AddComponent("limit", new LimitOffset
            {
                Limit = value
            });
        }

        public Q Offset(int value)
        {
            var clause = instance.GetOneComponent("limit", EngineScope) as LimitOffset;

            if (clause != null)
            {
                clause.Offset = value;
                return instance;
            }

            return instance.AddComponent("limit", new LimitOffset
            {
                Offset = value
            });
        }

        /// <summary>
        /// Alias for Limit
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public Q Take(int limit)
        {
            return Limit(limit);
        }

        /// <summary>
        /// Alias for Offset
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public Q Skip(int offset)
        {
            return Offset(offset);
        }

        /// <summary>
        /// Set the limit and offset for a given page.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="perPage"></param>
        /// <returns></returns>
        public Q ForPage(int page, int perPage = 15)
        {
            return Skip((page - 1) * perPage).Take(perPage);
        }

        public Q Distinct()
        {
            instance.IsDistinct = true;
            return instance;
        }

        /// <summary>
        /// Apply the callback's factory changes if the given "condition" is true.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Q When(bool condition, Func<Q, Q> callback)
        {
            if (condition)
            {
                return callback.Invoke(instance);
            }

            return instance;
        }

        /// <summary>
        /// Apply the callback's factory changes if the given "condition" is false.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Q WhenNot(bool condition, Func<Q, Q> callback)
        {
            if (!condition)
            {
                return callback.Invoke(instance);
            }

            return instance;
        }

        public Q OrderBy(params string[] columns)
        {
            foreach (var column in columns)
            {
                instance.AddComponent("order", new OrderBy
                {
                    Column = column,
                    Ascending = true
                });
            }

            return instance;
        }

        public Q OrderByDesc(params string[] columns)
        {
            foreach (var column in columns)
            {
                instance.AddComponent("order", new OrderBy
                {
                    Column = column,
                    Ascending = false
                });
            }

            return instance;
        }

        public Q OrderByRaw(string expression, params object[] bindings)
        {
            return instance.AddComponent("order", new RawOrderBy
            {
                Expression = expression,
                Bindings = Helper.Flatten(bindings).ToArray()
            });
        }

        public Q OrderByRandom(string seed)
        {
            return instance.AddComponent("order", new OrderByRandom { });
        }

        public Q GroupBy(params string[] columns)
        {
            foreach (var column in columns)
            {
                instance.AddComponent("group", new Column
                {
                    Name = column
                });
            }

            return instance;
        }

        public Q GroupByRaw(string expression, params object[] bindings)
        {
            instance.AddComponent("group", new RawColumn
            {
                Expression = expression,
                Bindings = bindings,
            });

            return instance;
        }

        public Q From(Q factory, string alias)
        {
            if (alias != null)
            {
                factory.As(alias);
            }
            return From(factory);
        }

        public Q From(Func<Q, Q> callback, string alias)
        {
            var query = NewQuery();
            query.SetParent(instance);
            return From(callback.Invoke(query), alias);
        }

        public override Q NewQuery()
        {
            return new QueryFactory<Q>() as Q;
        }

        public override string GetAlias()
        {
            return instance.QueryAlias;
        }
    }
}
