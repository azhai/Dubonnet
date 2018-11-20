using System.Collections.Generic;

namespace Dubonnet.QueryBuilder.Compilers.Bindings
{
    public class SqlResultBinder<Q> : ISqlResultBinder<Q> where Q : QueryBase<Q>
    {
        protected virtual Dictionary<string, object> PrepareNamedBindings(List<object> bindings)
        {
            var namedParams = new Dictionary<string, object>();

            for (var i = 0; i < bindings.Count; i++)
            {
                namedParams["p" + i] = bindings[i];
            }

            return namedParams;
        }

        protected virtual string PrepareSql(string rawSql)
        {
            return Helper.ReplaceAll(rawSql, "?", x => "@p" + x);
        }
        
        public void BindNamedParameters(SqlResult<Q> sqlResult)
        {
            sqlResult.NamedBindings = PrepareNamedBindings(sqlResult.Bindings);
            sqlResult.Sql = PrepareSql(sqlResult.RawSql);
        }
    }
}