using System;

namespace Dubonnet.Abstractions
{

    /// <summary>
    /// Defines methods for resolving table names of entities.
    /// </summary>
    public interface ITableNameResolver
    {
        /// <summary>
        /// Resolves the table name for the specified type.
        /// </summary>
        /// <param name="type">The type to resolve the table name for.</param>
        /// <returns>A string containing the resolved table name for for <paramref name="type"/>.</returns>
        string Resolve(Type type);
    }

}
