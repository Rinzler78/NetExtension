using System;

namespace Rinzler78.NetExtension.Observable;

/// <summary>
/// Marks a private backing field to automatically generate a public observable property
/// that raises <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/>
/// via <c>SetProperty</c> when its value changes.
/// </summary>
/// <remarks>
/// <para>
/// Apply this attribute to a private field inside a <c>partial</c> class that inherits
/// from <see cref="ObservableObject"/>. The source generator will emit a corresponding
/// public property in a generated partial file.
/// </para>
/// <para>Supported field naming rules:</para>
/// <list type="bullet">
///   <item><c>_name</c> → <c>Name</c></item>
///   <item><c>m_name</c> → <c>Name</c></item>
///   <item><c>name</c> → <c>Name</c></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// public partial class PersonViewModel : ObservableObject
/// {
///     [ObservableProperty]
///     private string _name = string.Empty;
///
///     [ObservableProperty]
///     private int _age;
/// }
/// // Generated:
/// // public string Name { get => _name; set => SetProperty(ref _name, value); }
/// // public int    Age  { get => _age;  set => SetProperty(ref _age,  value);  }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class ObservablePropertyAttribute : Attribute
{
    /// <summary>
    /// Gets the explicit property name override, or <see langword="null"/> to derive
    /// the name from the field name.
    /// </summary>
    public string? PropertyName { get; }

    /// <summary>Initialises a new instance; derives the property name from the field name.</summary>
    public ObservablePropertyAttribute() { }

    /// <summary>Initialises a new instance with an explicit property name.</summary>
    /// <param name="propertyName">The name to use for the generated property.</param>
    public ObservablePropertyAttribute(string propertyName)
    {
        PropertyName = propertyName;
    }
}
