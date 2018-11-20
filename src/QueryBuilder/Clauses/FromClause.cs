using System;

namespace Dubonnet.QueryBuilder.Clauses
{
    public abstract class AbstractFrom : AbstractClause
    {
        protected string _alias;

        /// <summary>
        /// Try to extract the Alias for the current clause.
        /// </summary>
        /// <returns></returns>
        public virtual string Alias { get => _alias; set => _alias = value; }
    }

    /// <summary>
    /// Represents a "from" clause.
    /// </summary>
    public class FromClause : AbstractFrom
    {
        public string Table { get; set; }

        public override string Alias
        {
            get
            {
                if (Table.IndexOf(" as ", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var segments = Table.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    return segments[2];
                }

                return Table;
            }
        }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new FromClause
            {
                Alias = Alias,
                Table = Table,
                Component = Component,
            };
        }
    }

    /// <summary>
    /// Represents a "from subquery" clause.
    /// </summary>
    public class QueryFromClause<Q> : AbstractFrom where Q: QueryBase<Q>
    {
        public Q Query { get; set; }

        public override string Alias
        {
            get
            {
                return string.IsNullOrEmpty(_alias) ? Query.GetAlias() : _alias;
            }
        }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new QueryFromClause<Q>
            {
                Engine = Engine,
                Alias = Alias,
                Query = Query.Clone() as Q,
                Component = Component,
            };
        }
    }

    public class RawFromClause : AbstractFrom
    {
        public string Expression { get; set; }
        public object[] Bindings { set; get; }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new RawFromClause
            {
                Engine = Engine,
                Alias = Alias,
                Expression = Expression,
                Bindings = Bindings,
                Component = Component,
            };
        }
    }
}