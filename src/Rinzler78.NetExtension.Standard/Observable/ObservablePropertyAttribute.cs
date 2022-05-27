using System;
using System.Runtime.CompilerServices;

namespace Rinzler78.NetExtension.Observable;

[AttributeUsage(AttributeTargets.Property)]
public class ObservablePropertyAttribute : Attribute
{
    public ObservablePropertyAttribute([CallerMemberName] string propertyName = null)
    {
    }
}