using Dapper.FluentMap.Mapping;

namespace Dubonnet.FluentMapping
{
    /// <inheritdoc />
    /// <summary>
    /// Represents a non-typed mapping of an entity for Dubon.
    /// </summary>
    public interface IDubonEntityMap : IEntityMap
    {
        /// <summary>
        /// Gets the table name for the current entity.
        /// </summary>
        string TableName { get; }
    }
}