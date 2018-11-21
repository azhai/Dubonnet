using System;
using System.Linq;
using Dubonnet.QueryBuilder.Compilers.Bindings;

// ReSharper disable InconsistentNaming

namespace Dubonnet.QueryBuilder.Compilers
{
    public sealed class Oracle11gCompiler<Q> : Compiler<Q> where Q : QueryFactory<Q>
    {
        public Oracle11gCompiler() : base(
            new OracleResultBinder<Q>()
            )
        {
            ColumnAsKeyword = "";
            TableAsKeyword = "";
        }

        public override string EngineCode { get; } = "oracle11g";

        protected override SqlResult<Q> CompileSelectQuery(Q factory)
        {
            var ctx = new SqlResult<Q>
            {
                Query = factory.Clone() as Q,
            };
            
            var results = new[] {
                    CompileColumns(ctx),
                    CompileFrom(ctx),
                    CompileJoins(ctx),
                    CompileWheres(ctx),
                    CompileGroups(ctx),
                    CompileHaving(ctx),
                    CompileOrders(ctx),
                    CompileUnion(ctx)
                }
                .Where(x => x != null)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();

            var sql = string.Join(" ", results);
            ctx.RawSql = sql;

            ApplyLimit(ctx);

            return ctx;
        }

        public override string CompileLimit(SqlResult<Q> ctx)
        {
            throw new NotSupportedException();
        }
        
        internal void ApplyLimit(SqlResult<Q> ctx)
        {
            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit == 0 && offset == 0)
            {
                return;
            }
            
            //@todo replace with alias generator
            var alias1 = WrapValue("QueryBuilder_A__");
            var alias2 = WrapValue("QueryBuilder_B__"); 

            string newSql;
            if (limit == 0)
            {
                newSql = $"SELECT * FROM (SELECT {alias1}.*, ROWNUM {alias2} FROM ({ctx.RawSql}) {alias1}) WHERE {alias2} > ?";
                ctx.Bindings.Add(offset);
            } else if (offset == 0)
            {
                newSql = $"SELECT * FROM ({ctx.RawSql}) WHERE ROWNUM <= ?";
                ctx.Bindings.Add(limit);
            }
            else
            {
                newSql = $"SELECT * FROM (SELECT {alias1}.*, ROWNUM {alias2} FROM ({ctx.RawSql}) {alias1} WHERE ROWNUM <= ?) WHERE {alias2} > ?";
                ctx.Bindings.Add(limit +  offset);
                ctx.Bindings.Add(offset);
            }

            ctx.RawSql = newSql;
        }
    }
}
