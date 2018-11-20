using System;
using Dubonnet.QueryBuilder.Compilers.Bindings;
using Dubonnet.QueryBuilder.Clauses;

namespace Dubonnet.QueryBuilder.Compilers
{
    public class SqlServerCompiler<Q> : Compiler<Q> where Q : QueryFactory<Q>
    {
        public SqlServerCompiler() : base(
            new SqlResultBinder<Q>()
            )

        {
            OpeningIdentifier = "[";
            ClosingIdentifier = "]";
            LastId = "SELECT scope_identity() as Id";
        }

        public override string EngineCode { get; } = "sqlsrv";
        public bool UseLegacyPagination { get; set; } = true;

        protected override SqlResult<Q> CompileSelectQuery(Q factory)
        {
            if (!UseLegacyPagination || !factory.HasOffset())
            {
                return base.CompileSelectQuery(factory);
            }

            factory = factory.Clone() as Q;

            var ctx = new SqlResult<Q>
            {
                Query = factory,
            };

            var limit = factory.GetLimit(EngineCode);
            var offset = factory.GetOffset(EngineCode);


            if (!factory.HasComponent("select"))
            {
                factory.Select("*");
            }
            var order = CompileOrders(ctx) ?? "ORDER BY (SELECT 0)";
            factory.SelectRaw($"ROW_NUMBER() OVER ({order}) AS [row_num]", ctx.Bindings);

            factory.ClearComponent("order");


            var result = base.CompileSelectQuery(factory);

            if (limit == 0)
            {
                result.RawSql = $"SELECT * FROM ({result.RawSql}) AS [results_wrapper] WHERE [row_num] >= ?";
                result.Bindings.Add(offset + 1);
            }
            else
            {
                result.RawSql = $"SELECT * FROM ({result.RawSql}) AS [results_wrapper] WHERE [row_num] BETWEEN ? AND ?";
                result.Bindings.Add(offset + 1);
                result.Bindings.Add(limit + offset);
            }

            return result;
        }

        protected override string CompileColumns(SqlResult<Q> ctx)
        {
            var compiled = base.CompileColumns(ctx);

            if (!UseLegacyPagination)
            {
                return compiled;
            }

            // If there is a limit on the factory, but not an offset, we will add the top
            // clause to the factory, which serves as a "limit" type clause within the
            // SQL Server system similar to the limit keywords available in MySQL.
            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit > 0 && offset == 0)
            {
                // top bindings should be inserted first
                ctx.Bindings.Insert(0, limit);

                ctx.Query.ClearComponent("limit");

                // handle distinct
                if (compiled.IndexOf("SELECT DISTINCT") == 0)
                {
                    return "SELECT DISTINCT TOP (?)" + compiled.Substring(15);
                }

                return "SELECT TOP (?)" + compiled.Substring(6);
            }

            return compiled;
        }

        public override string CompileLimit(SqlResult<Q> ctx)
        {
            if (UseLegacyPagination)
            {
                // in legacy versions of Sql Server, limit is handled by TOP
                // and ROW_NUMBER techniques
                return null;
            }

            var limit = ctx.Query.GetLimit(EngineCode);
            var offset = ctx.Query.GetOffset(EngineCode);

            if (limit == 0 && offset == 0)
            {
                return null;
            }

            var safeOrder = "";
            if (!ctx.Query.HasComponent("order"))
            {
                safeOrder = "ORDER BY (SELECT 0) ";
            }

            if (limit == 0)
            {
                ctx.Bindings.Add(offset);
                return $"{safeOrder}OFFSET ? ROWS";
            }

            ctx.Bindings.Add(offset);
            ctx.Bindings.Add(limit);

            return $"{safeOrder}OFFSET ? ROWS FETCH NEXT ? ROWS ONLY";
        }

        public override string CompileRandom(string seed)
        {
            return "NEWID()";
        }

        public override string CompileTrue()
        {
            return "cast(1 as bit)";
        }

        public override string CompileFalse()
        {
            return "cast(0 as bit)";
        }

        protected override string CompileBasicDateCondition(SqlResult<Q> ctx, BasicDateCondition condition)
        {
            var column = Wrap(condition.Column);

            string left;

            if (condition.Part == "time")
            {
                left = $"CAST({column} as time)";
            }
            else if (condition.Part == "date")
            {
                left = $"CAST({column} as date)";
            }
            else
            {
                left = $"DATEPART({condition.Part.ToUpper()}, {column})";
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
