using Dubonnet.FluentMapping;
using Dubonnet.IntegrationTests.Entities;

namespace Dubonnet.IntegrationTests.Map
{
   public class ProductMap: DubonEntityMap<Product2>
    {
        public ProductMap()
        {
            ToTable("Products");
            Map(p => p.ProductId).ToColumn("Id")
                                 .IsKey()
                                 .IsIdentity();
            Map(p => p.ProductName).ToColumn("Name");
        }
    }
}
