using System.Collections.Generic;

namespace Dubonnet.QueryBuilder.Clauses
{
    public abstract class AbstractInsertClause : AbstractClause
    {

    }

    public class InsertClause : AbstractInsertClause
    {
        public List<string> Columns { get; set; }
        public List<object> Values { get; set; }
        public bool ReturnId { get; set; } = false;

        public override AbstractClause Clone()
        {
            return new InsertClause
            {
                Engine = Engine,
                Component = Component,
                Columns = Columns,
                Values = Values,
                ReturnId = ReturnId,
            };
        }
    }

    public class InsertQueryClause<Q> : AbstractInsertClause where Q: QueryFactory<Q>
    {
        public List<string> Columns { get; set; }
        public Q Query { get; set; }

        public override AbstractClause Clone()
        {
            return new InsertQueryClause<Q>
            {
                Engine = Engine,
                Component = Component,
                Columns = Columns,
                Query = Query.Clone() as Q,
            };
        }
    }
}