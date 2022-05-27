using System.Linq;
using System.Threading.Tasks;

namespace Rinzler78.NetExtension.Objects;

public static class UpdatablePropertyExtension
{
    public static async Task GetAll(this UpdatableProperty[] updatableProperties)
    {
        await Task.WhenAll(updatableProperties.Select(arg => arg.Get())).ConfigureAwait(false);
    }

    public static async Task UpdateAll(this UpdatableProperty[] updatableProperties)
    {
        await Task.WhenAll(updatableProperties.Select(arg => arg.Update())).ConfigureAwait(false);
    }
}