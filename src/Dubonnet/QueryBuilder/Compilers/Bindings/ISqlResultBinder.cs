namespace Dubonnet.QueryBuilder.Compilers.Bindings
{
    public interface ISqlResultBinder<Q> where Q : QueryBase<Q>
    {
        void BindNamedParameters(SqlResult<Q> sqlResult);
    }
}