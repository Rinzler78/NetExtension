using System.Linq;

namespace Rinzler78.NetExtension.Objects;

public static class UpdatablePropertyExtension
{
    public static async System.Threading.Tasks.Task GetAll(this UpdatableProperty[] updatableProperties)
    {
        await System.Threading.Tasks.Task.WhenAll(updatableProperties.Select(arg => arg.Get())).ConfigureAwait(false);
    }

    public static async System.Threading.Tasks.Task UpdateAll(this UpdatableProperty[] updatableProperties)
    {
        await System.Threading.Tasks.Task.WhenAll(updatableProperties.Select(arg => arg.Update())).ConfigureAwait(false);
    }
}