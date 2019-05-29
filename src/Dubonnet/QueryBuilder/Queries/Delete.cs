namespace Dubonnet.QueryBuilder
{
    public partial class QueryFactory<Q>
    {
        public Q AsDelete()
        {
            instance.Method = "delete";
            return instance;
        }

    }
}