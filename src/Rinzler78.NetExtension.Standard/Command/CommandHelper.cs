using System.Windows.Input;

namespace Rinzler78.NetExtension.Command;

public static class CommandHelper
{
    public static bool TryExecute(this ICommand command, object? caller = null)
    {
        if (command?.CanExecute(caller) ?? false)
        {
            command.Execute(caller);
            return true;
        }

        return false;
    }
}
