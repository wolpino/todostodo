using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace todostodo;

/// <summary>
/// Extension methods for DbSet&lt;T&gt; to provide additional functionality.
/// </summary>
public static class DbSetExtension
{
    /// <summary>
    /// Gets an existing item from the database or creates and adds a new one if not found.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="dbSet">The DbSet to query.</param>
    /// <param name="predicate">The predicate to search for the item.</param>
    /// <param name="defaultItem">The default item to add if not found. Must not be null.</param>
    /// <returns>The existing item or the newly added default item.</returns>
    public static async Task<T> GetOrCreate<T>(this DbSet<T> dbSet, Expression<Func<T, bool>> predicate, T defaultItem) where T : class
    {
        if (defaultItem == null)
            throw new ArgumentNullException(nameof(defaultItem), "Default item cannot be null");

        T? item = await dbSet.FirstOrDefaultAsync(predicate);

        if (item == null)
        {
            item = defaultItem;
            dbSet.Add(item);
        }

        return item;
    }
}