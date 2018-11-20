using System;
using Dubonnet.QueryBuilder.Compilers.Bindings;

namespace Dubonnet.QueryBuilder.Compilers
{
    public class MySqlCompiler<Q> : Compiler<Q> where Q : QueryFactory<Q>
    {
        public MySqlCompiler() : base(
            new SqlResultBinder<Q>()
            )
        {
            OpeningIdentifier = ClosingIdentifier = "`";
            LastId = "SELECT last_insert_id() as Id";
        }

        public override string EngineCode { get; } = "mysql";

        public override string CompileLimit(SqlResult<Q> ctx)
        {
            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);


            if (offset == 0 && limit == 0)
            {
                return null;
            }

            if (offset == 0)
            {
                ctx.Bindings.Add(limit);
                return "LIMIT ?";
            }

            if (limit == 0)
            {

                // MySql will not accept offset without limit, so we will put a large number
                // to avoid this error.

                ctx.Bindings.Add(offset);
                return "LIMIT 18446744073709551615 OFFSET ?";
            }

            // We have both values

            ctx.Bindings.Add(limit);
            ctx.Bindings.Add(offset);

            return "LIMIT ? OFFSET ?";

        }
    }
}