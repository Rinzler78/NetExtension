using System.Linq;
using System.Threading.Tasks;

namespace Rinzler78.NetExtension.Objects;

/// <summary>
/// Provides batch operations for arrays of <see cref="UpdatableProperty"/>.
/// </summary>
public static class UpdatablePropertyExtension
{
    /// <summary>
    /// Asynchronously retrieves all updatable properties in parallel.
    /// </summary>
    /// <param name="updatableProperties">The array of updatable properties to retrieve.</param>
    /// <returns>A task representing the parallel retrieval operation.</returns>
    public static async Task GetAll(this UpdatableProperty[] updatableProperties)
    {
        await Task.WhenAll(updatableProperties.Select(arg => arg.Get())).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously updates all updatable properties in parallel.
    /// </summary>
    /// <param name="updatableProperties">The array of updatable properties to update.</param>
    /// <returns>A task representing the parallel update operation.</returns>
    public static async Task UpdateAll(this UpdatableProperty[] updatableProperties)
    {
        await Task.WhenAll(updatableProperties.Select(arg => arg.Update())).ConfigureAwait(false);
    }
}
