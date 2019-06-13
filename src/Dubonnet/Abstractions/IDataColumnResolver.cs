using System;
using System.Data;

namespace Dubonnet.Abstractions
{
   
        /// <summary>
        /// Defines methods for resolving data column for entities.
        /// </summary>
        public interface IDataColumnResolver
        {

        /// <summary>
        /// Resolves the data column.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns></returns>
        Tuple<string, Type> ResolveDataColumn(DubonProperty propertyInfo);
        }
    
}
