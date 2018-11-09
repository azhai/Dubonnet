using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Dubonnet.Abstractions;

namespace Dubonnet.Resolvers
{
    
        /// <summary>
        /// Implements the <see cref="IKeyPropertyResolver"/> interface by resolving key properties
        /// with the [Key] attribute or with the name 'Id'.
        /// </summary>
        public class DefaultKeyPropertyResolver : IKeyPropertyResolver
        {
            /// <summary>
            /// Finds the key property by looking for a property with the [Key] attribute or with the name 'Id'.
            /// </summary>
            public virtual DubonProperty Resolve(Type type) => Resolve(type, out _);

            /// <summary>
            /// Finds the key property by looking for a property with the [Key] attribute or with the name 'Id'.
            /// </summary>
            public DubonProperty Resolve(Type type, out bool isIdentity)
            {
                var keyProps =DubonMapper.Resolvers.Properties(type)
                       .Where(p => p.PropertyInfo.GetCustomAttribute<KeyAttribute>() != null || p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                       .ToArray();

                if (keyProps.Length == 0)
                {
                    throw new InvalidOperationException($"Could not find the key property for type '{type.FullName}'.");
                }

                if (keyProps.Length > 1)
                {
                    throw new InvalidOperationException($"Multiple key properties were found for type '{type.FullName}'.");
                }

                isIdentity = true;
                return keyProps[0];
            }
        }
    }

