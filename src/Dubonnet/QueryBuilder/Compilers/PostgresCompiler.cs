using System;
using Dubonnet.QueryBuilder.Compilers.Bindings;
using Dubonnet.QueryBuilder.Clauses;

namespace Dubonnet.QueryBuilder.Compilers
{
    public class PostgresCompiler<Q> : Compiler<Q> where Q : QueryFactory<Q>
    {
        public PostgresCompiler() : base(
            new SqlResultBinder<Q>()
            )
        {
            LastId = "SELECT lastval()";
        }

        public override string EngineCode { get; } = "postgres";

        protected override string CompileBasicDateCondition(SqlResult<Q> ctx, BasicDateCondition condition)
        {
            var column = Wrap(condition.Column);

            string left;

            if (condition.Part == "time")
            {
                left = $"{column}::time";
            }
            else if (condition.Part == "date")
            {
                left = $"{column}::date";
            }
            else
            {
                left = $"DATE_PART('{condition.Part.ToUpper()}', {column})";
            }

            var sql = $"{left} {condition.Operator} {Parameter(ctx, condition.Value)}";

            if (condition.IsNot)
            {
                return $"NOT ({sql})";
            }

            return sql;
        }
    }
}