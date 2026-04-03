using System.ComponentModel;

namespace Rinzler78.NetExtension.Observable;

public interface IObservableObject : INotifyPropertyChanged
{
    string? NickName { get; }
}
