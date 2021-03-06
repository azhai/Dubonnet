using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dubonnet.Abstractions
{

    /// <summary>
    /// Defines methods for resolving the properties of entities.
    /// </summary>
    public interface IPropertyResolver
    {
        /// <summary>
        /// Resolves the properties to be mapped for the specified type.
        /// </summary>
        /// <param name="type">The type to resolve the properties to be mapped for.</param>
        /// <returns>A collection of <see cref="PropertyInfo"/>'s of the <paramref name="type"/>.</returns>
        IEnumerable<DubonProperty> Resolve(Type type);
    }

}
