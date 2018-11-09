using Dubonnet.FluentMapping;
using Dubonnet.IntegrationTests.Entities;

namespace Dubonnet.IntegrationTests.Map
{
    public class LargeProductMap : DubonEntityMap<LargeProduct>
    {
        public LargeProductMap()
        {
            Map(x => x.ProductId).ToColumn("IDARTIKULLI");
            ToTable("T_ARTIKULLI");
        }
    }
}
