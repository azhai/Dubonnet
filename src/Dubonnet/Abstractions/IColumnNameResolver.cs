namespace Dubonnet.Abstractions
{
   
        /// <summary>
        /// Defines methods for resolving column names for entities.
        /// </summary>
        public interface IColumnNameResolver
        {
            /// <summary>
            /// Resolves the column name for the specified property.
            /// </summary>
            /// <param name="propertyInfo">The property of the entity.</param>
            /// <returns>The column name for the property.</returns>
            string Resolve(DubonProperty propertyInfo);
        }
    
}
