using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Rinlzer78.NetExtension.Observable
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public class ObservablePropertyAttribute : Attribute
    {
        public ObservablePropertyAttribute([CallerMemberName] string propertyName = null)
        {

        }
    }

    public interface IObservableObject : INotifyPropertyChanged
    {
    }
}
