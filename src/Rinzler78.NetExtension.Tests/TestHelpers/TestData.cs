using System.Numerics;
using Rinzler78.NetExtension.Observable;

namespace Rinzler78.NetExtension.Tests.TestHelpers;

public static class TestData
{
    public static class Strings
    {
        public const string Empty = "";
        public const string SingleChar = "a";
        public const string Simple = "hello";
        public const string WithSpaces = "hello world";
        public const string Unicode = "héllo wörld 🌍";
        public const string LongString = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";
        public const string ValidEmail = "test@example.com";
        public const string InvalidEmail = "invalid-email";
        public const string JsonString = @"{""name"":""test"",""value"":42}";
        public const string MalformedJson = @"{""name"":""test"",""value"":";
    }

    public static class Numbers
    {
        public const int Zero = 0;
        public const int Positive = 42;
        public const int Negative = -42;
        public const int MaxInt = int.MaxValue;
        public const int MinInt = int.MinValue;
        
        public const decimal DecimalPositive = 42.5m;
        public const decimal DecimalNegative = -42.5m;
        public const decimal DecimalMax = decimal.MaxValue;
        public const decimal DecimalMin = decimal.MinValue;
        
        public static readonly BigInteger BigIntegerZero = BigInteger.Zero;
        public static readonly BigInteger BigIntegerPositive = new(42);
        public static readonly BigInteger BigIntegerNegative = new(-42);
        public static readonly BigInteger BigIntegerLarge = BigInteger.Parse("123456789012345678901234567890");
    }

    public static class Arrays
    {
        public static readonly ulong[] ULongArray = { 1UL, 2UL, 3UL, ulong.MaxValue };
        public static readonly uint[] UIntArray = { 1U, 2U, 3U, uint.MaxValue };
        public static readonly string[] StringArray = { "hello", "world", "test" };
        public static readonly double[] DoubleArray = { 1.5, 2.5, 3.5 };
        public static readonly double[][] DoubleArrays = { new double[] { 1.0, 2.0 }, new double[] { 3.0, 4.0 } };
        public static readonly int[] EmptyIntArray = new int[0];
        public static readonly string[] EmptyStringArray = new string[0];
    }

    public static class Collections
    {
        public static readonly List<int> IntList = new() { 1, 2, 3, 4, 5 };
        public static readonly List<string> StringList = new() { "a", "b", "c" };
        public static readonly List<int> EmptyIntList = new();
        public static readonly Dictionary<string, int> StringIntDict = new() { { "one", 1 }, { "two", 2 } };
    }

    public static class TestObjects
    {
        public record SimpleRecord(string Name, int Value);
        public record ComplexRecord(string Name, int Value, List<string> Items, Dictionary<string, object> Properties);
        
        public static readonly SimpleRecord SimpleRecordInstance = new("Test", 42);
        public static readonly ComplexRecord ComplexRecordInstance = new(
            "Complex", 
            100, 
            new List<string> { "item1", "item2" },
            new Dictionary<string, object> { { "key1", "value1" }, { "key2", 42 } }
        );
    }
}

public class TestObservableObject : ObservableObject
{
    private string _name = string.Empty;
    private int _value;
    private bool _isActive;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public int Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    // Property that depends on other properties
    public string DisplayName => $"{Name} ({Value})";
}

public class TestObservableObjectWithDependencies : ObservableObject
{
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;

    public string FirstName
    {
        get => _firstName;
        set => SetProperty(ref _firstName, value);
    }

    public string LastName
    {
        get => _lastName;
        set => SetProperty(ref _lastName, value);
    }

    public string FullName => $"{FirstName} {LastName}";
}