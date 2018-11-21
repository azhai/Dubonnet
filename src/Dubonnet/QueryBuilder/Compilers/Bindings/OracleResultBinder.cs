namespace Dubonnet.QueryBuilder.Compilers.Bindings
{
    public class OracleResultBinder<Q> : SqlResultBinder<Q> where Q : QueryBase<Q>
    {
        protected override string PrepareSql(string rawSql)
        {
            return Helper.ReplaceAll(rawSql, "?", x => ":p" + x);
        }
    }
}