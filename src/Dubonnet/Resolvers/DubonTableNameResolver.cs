using System;
using Dubonnet.Abstractions;
using Dubonnet.FluentMapping;
using Dubonnet.Internal;
using Dapper.FluentMap;

namespace Dubonnet.Resolvers
{
    /// <inheritdoc />
    /// <summary>
    /// Implements the <see cref="T:Dubonnet.Abstractions.ITableNameResolver" /> interface by using the configured mapping.
    /// </summary>
    public class DubonTableNameResolver : ITableNameResolver
    {
        /// <summary>
        /// Resolves the table name for the specified type.
        /// </summary>
        /// <param name="type">The type to resolve the table name for.</param>
        /// <returns>
        /// A string containing the resolved table name for for <paramref name="type" />.
        /// </returns>
        /// <inheritdoc />
        public string Resolve(Type type)
        {
            if (!FluentMapper.EntityMaps.TryGetValue(type, out var entityMap))
                return DubonMapper.Resolvers.Default.TableNameResolver.Resolve(type);
            if (entityMap is IDubonEntityMap mapping)
            {
                return mapping.TableName;
            }

            return DefaultResolvers.TableNameResolver.Resolve(type);
        }
    }
}
