using System.ComponentModel;

namespace Rinzler78.NetExtension.Observable;

/// <summary>
/// Represents an observable object that notifies listeners of property changes and exposes an optional nickname.
/// </summary>
public interface IObservableObject : INotifyPropertyChanged
{
    /// <summary>
    /// Gets the optional display nickname for this observable object.
    /// </summary>
    string? NickName { get; }
}
