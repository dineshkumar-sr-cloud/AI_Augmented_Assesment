namespace TaskBridge.Core.Interfaces;

/// <summary>
/// Generic repository interface for data access operations.
/// All repositories must enforce multi-tenant isolation.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Gets an entity by its ID, scoped to the organization.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <param name="organizationId">The organization ID for multi-tenant isolation.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    Task<T?> GetByIdAsync(string id, string organizationId);

    /// <summary>
    /// Creates a new entity in the organization.
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    /// <param name="organizationId">The organization ID for multi-tenant isolation.</param>
    /// <returns>The created entity.</returns>
    Task<T> CreateAsync(T entity, string organizationId);
}
