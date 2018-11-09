using Dubonnet.Resolvers;
using Dapper.FluentMap.Configuration;

namespace Dubonnet.FluentMapping
{
    /// <summary>
    /// Defines methods for configured Dapper.FluentMap.Dubon.
    /// </summary>
    public static class FluentMapConfigurationExtensions
    {
        /// <summary>
        /// Configures the specified configuration for Dapper.FluentMap.Dubon.
        /// </summary>
        /// <param name="config">The Dapper.FluentMap configuration.</param>
        /// <returns>
        /// The Dapper.FluentMap configuration.
        /// </returns>
        public static FluentMapConfiguration ApplyToDubon(this FluentMapConfiguration config)
        {
            DubonMapper.SetColumnNameResolver(new DubonColumnNameResolver());
            DubonMapper.SetDataColumnResolver(new DubonDataColumnResolver());
            DubonMapper.SetKeyPropertyResolver(new DubonKeyPropertyResolver());
            DubonMapper.SetTableNameResolver(new DubonTableNameResolver());
            DubonMapper.SetPropertyResolver(new DubonPropertyResolver());
            return config;
        }
    }
}
