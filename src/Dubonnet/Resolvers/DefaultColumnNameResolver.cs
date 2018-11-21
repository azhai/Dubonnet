using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Dubonnet.Abstractions;

namespace Dubonnet.Resolvers
{
     /// <summary>
        /// Implements the <see cref="IKeyPropertyResolver"/>.
        /// </summary>
        public class DefaultColumnNameResolver : IColumnNameResolver
        {
            /// <summary>
            /// Resolves the column name for the property.
            /// Looks for the [Column] attribute. Otherwise it's just the name of the property.
            /// </summary>
            public virtual string Resolve(DubonProperty propertyInfo)
            {
                var columnAttr = propertyInfo.PropertyInfo.GetCustomAttribute<ColumnAttribute>();
                if (columnAttr != null)
                {
                    return columnAttr.Name;
                }

                return propertyInfo.Name;
            }
        }
    
}
