using System.Windows.Input;

namespace Rinzler78.NetExtension.Command;

/// <summary>
/// Provides extension methods for <see cref="ICommand"/> execution.
/// </summary>
public static class CommandHelper
{
    /// <summary>
    /// Attempts to execute the command if it can execute, returning whether execution occurred.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="caller">An optional parameter passed to the command.</param>
    /// <returns><c>true</c> if the command was executed; otherwise <c>false</c>.</returns>
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
