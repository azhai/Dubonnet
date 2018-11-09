using System.Linq;
using Dubonnet.Abstractions;
using Dubonnet.FluentMapping;
using Dubonnet.Internal;
using Dapper.FluentMap;

namespace Dubonnet.Resolvers
{
    /// <summary>
    /// Implements the <see cref="IColumnNameResolver"/> interface by using the configured mapping.
    /// </summary>
    public class DubonColumnNameResolver : IColumnNameResolver
    {
        /// <inheritdoc/>
        public string Resolve(DubonProperty DubonProperty)
        {
            if (!FluentMapper.EntityMaps.TryGetValue(DubonProperty.Type, out var entityMap))
                return DefaultResolvers.ColumnNameResolver.Resolve(DubonProperty);

            if (!(entityMap is IDubonEntityMap))
                return DefaultResolvers.ColumnNameResolver.Resolve(DubonProperty);

            var propertyMaps = entityMap.PropertyMaps.Where(m => m.PropertyInfo.Name == DubonProperty.Name).ToList();

            return propertyMaps.Count == 1 ? propertyMaps[0].ColumnName : DefaultResolvers.ColumnNameResolver.Resolve(DubonProperty);
        }
    }
}
