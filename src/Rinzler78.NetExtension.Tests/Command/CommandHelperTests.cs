using System.Windows.Input;
using Rinzler78.NetExtension.Command;

namespace Rinzler78.NetExtension.Tests.Command;

[Trait("Category", "Unit")]
public class CommandHelperTests
{
    [Fact]
    public void TryExecute_WhenCommandCanExecute_ShouldExecuteAndReturnTrue()
    {
        var command = new TestCommand(canExecute: true);

        var result = command.TryExecute("caller");

        result.Should().BeTrue();
        command.ExecuteCalls.Should().Be(1);
        command.LastParameter.Should().Be("caller");
    }

    [Fact]
    public void TryExecute_WhenCommandCannotExecute_ShouldReturnFalse()
    {
        var command = new TestCommand(canExecute: false);

        var result = command.TryExecute("caller");

        result.Should().BeFalse();
        command.ExecuteCalls.Should().Be(0);
    }

    [Fact]
    public void TryExecute_WithNullCommand_ShouldReturnFalse()
    {
        var result = CommandHelper.TryExecute(null!);

        result.Should().BeFalse();
    }

    private sealed class TestCommand : ICommand
    {
        private readonly bool _canExecute;

        public TestCommand(bool canExecute)
        {
            _canExecute = canExecute;
        }

        public int ExecuteCalls { get; private set; }

        public object? LastParameter { get; private set; }

#pragma warning disable CS0067
        public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

        public bool CanExecute(object? parameter) => _canExecute;

        public void Execute(object? parameter)
        {
            ExecuteCalls++;
            LastParameter = parameter;
        }
    }
}
