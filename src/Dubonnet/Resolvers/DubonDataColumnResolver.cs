using System;
using System.Linq;
using Dubonnet.Abstractions;
using Dubonnet.FluentMapping;
using Dapper.FluentMap;

namespace Dubonnet.Resolvers
{
    /// <inheritdoc />
    /// <summary>
    /// Implements the <see cref="T:Dubonnet.Abstractions.IDataColumnResolver" /> interface by using the configured mapping.
    /// </summary>
    public class DubonDataColumnResolver : IDataColumnResolver
    {
        /// <inheritdoc/>
        public Tuple<string, Type> ResolveDataColumn(DubonProperty propertyInfo)
        {
            if (propertyInfo.Type == null)
                return DubonMapper.Resolvers.Default.DataColumnResolver.ResolveDataColumn(propertyInfo);

            if (!FluentMapper.EntityMaps.TryGetValue(propertyInfo.Type, out var entityMap))
                return DubonMapper.Resolvers.Default.DataColumnResolver.ResolveDataColumn(propertyInfo);

            if (!(entityMap is IDubonEntityMap))
                return DubonMapper.Resolvers.Default.DataColumnResolver.ResolveDataColumn(propertyInfo);

            var propertyMaps = entityMap.PropertyMaps.Where(m => m.PropertyInfo.Name == propertyInfo.PropertyInfo.Name).ToList();

            return propertyMaps.Count == 1 ? new Tuple<string, Type>(propertyMaps[0].ColumnName, propertyMaps[0].PropertyInfo.PropertyType) :
                DubonMapper.Resolvers.Default.DataColumnResolver.ResolveDataColumn(propertyInfo);
        }
    }
}
