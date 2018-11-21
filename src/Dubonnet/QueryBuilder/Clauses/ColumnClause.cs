namespace Dubonnet.QueryBuilder.Clauses
{
    public abstract class AbstractColumn : AbstractClause
    {
    }

    /// <summary>
    /// Represents "column" or "column as alias" clause.
    /// </summary>
    /// <seealso cref="AbstractColumn" />
    public class Column : AbstractColumn
    {
        /// <summary>
        /// Gets or sets the column name. Can be "columnName" or "columnName as columnAlias".
        /// </summary>
        /// <value>
        /// The column name.
        /// </value>
        public string Name { get; set; }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new Column
            {
                Engine = Engine,
                Name = Name,
                Component = Component,
            };
        }
    }

    /// <summary>
    /// Represents column clause calculated using factory.
    /// </summary>
    /// <seealso cref="AbstractColumn" />
    public class QueryColumn<Q> : AbstractColumn where Q: QueryFactory<Q>
    {
        /// <summary>
        /// Gets or sets the factory that will be used for column value calculation.
        /// </summary>
        /// <value>
        /// The factory for column value calculation.
        /// </value>
        public Q Query { get; set; }
        public override AbstractClause Clone()
        {
            return new QueryColumn<Q>
            {
                Engine = Engine,
                Query = Query.Clone() as Q,
                Component = Component,
            };
        }
    }

    public class RawColumn : AbstractColumn
    {
        /// <summary>
        /// Gets or sets the RAW expression.
        /// </summary>
        /// <value>
        /// The RAW expression.
        /// </value>
        public string Expression { get; set; }
        public object[] Bindings { set; get; }

        /// <inheritdoc />
        public override AbstractClause Clone()
        {
            return new RawColumn
            {
                Engine = Engine,
                Expression = Expression,
                Bindings = Bindings,
                Component = Component,
            };
        }
    }
}