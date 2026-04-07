using System.Collections.ObjectModel;
using System.Numerics;
using System.Text;
using System.Text.Json;
using BigRat = System.Numerics.BigRational;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rinzler78.NetExtension.Json.Converters;

namespace Rinzler78.NetExtension.Tests.Json;

[Trait("Category", "Unit")]
public class JsonConvertersTests
{
    [Fact]
    public void BigIntegerConverter_ShouldReadWriteAndValidate()
    {
        AssertNewtonsoftConverter(BigIntegerConverter.Singleton, new BigInteger(123), "\"123\"");
        BigIntegerConverter.Singleton.CanConvert(typeof(BigInteger?)).Should().BeTrue();
        AssertNullReadAndWrite(BigIntegerConverter.Singleton, typeof(BigInteger));
    }

    [Fact]
    public void BigRationalConverter_ShouldReadWriteAndValidate()
    {
        AssertNewtonsoftConverter(BigRationalConverter.Singleton, BigRat.Parse("123/4"), "\"30.75\"");
        BigRationalConverter.Singleton.CanConvert(typeof(BigRat?)).Should().BeTrue();
        AssertNullReadAndWrite(BigRationalConverter.Singleton, typeof(BigRat));

        // Deserializing a JSON null string (not token-null) should throw
        var emptyReader = new JsonTextReader(new StringReader("\"\""));
        emptyReader.Read();
        Action emptyAct = () => _ = BigRationalConverter.Singleton.ReadJson(emptyReader, typeof(BigRat), null, Newtonsoft.Json.JsonSerializer.Create());
        emptyAct.Should().Throw<JsonSerializationException>();
    }

    [Fact]
    public void DecimalConverter_ShouldReadWriteAndValidate()
    {
        AssertNewtonsoftConverter(DecimalConverter.Singleton, 12.5m, "\"12.5\"");
        DecimalConverter.Singleton.CanConvert(typeof(decimal?)).Should().BeTrue();
        AssertNullReadAndWrite(DecimalConverter.Singleton, typeof(decimal));
    }

    [Fact]
    public void LongConverter_ShouldReadWriteAndValidate()
    {
        AssertNewtonsoftConverter(LongConverter.Singleton, 42L, "\"42\"");
        LongConverter.Singleton.CanConvert(typeof(long?)).Should().BeTrue();
        AssertNullReadAndWrite(LongConverter.Singleton, typeof(long));
    }

    [Fact]
    public void ULongConverter_ShouldReadWriteAndValidate()
    {
        AssertNewtonsoftConverter(ULongConverter.Singleton, 42UL, "\"42\"");
        ULongConverter.Singleton.CanConvert(typeof(ulong?)).Should().BeTrue();
        AssertNullReadAndWrite(ULongConverter.Singleton, typeof(ulong));
    }

    [Fact]
    public void DoubleConverter_ShouldReadWriteAndValidate()
    {
        AssertNewtonsoftConverter(DoubleConverter.Singleton, 12.5d, "\"12.5\"");
        DoubleConverter.Singleton.CanConvert(typeof(double?)).Should().BeTrue();

        // Comma-as-decimal-separator is not valid JSON — must throw
        var commaReader = new JsonTextReader(new StringReader("\"12,5\""));
        commaReader.Read();
        Action commaAct = () => _ = DoubleConverter.Singleton.ReadJson(commaReader, typeof(double), null, Newtonsoft.Json.JsonSerializer.Create());
        commaAct.Should().Throw<JsonSerializationException>();

        // Non-numeric string must throw
        var invalidReader = new JsonTextReader(new StringReader("\"nope\""));
        invalidReader.Read();
        Action act = () => _ = DoubleConverter.Singleton.ReadJson(invalidReader, typeof(double), null, Newtonsoft.Json.JsonSerializer.Create());
        act.Should().Throw<JsonSerializationException>();

        AssertNullReadAndWrite(DoubleConverter.Singleton, typeof(double));
    }

    [Fact]
    public void ULongArrayConverter_ShouldReadWriteAndRejectInvalidArrays()
    {
        var reader = new JsonTextReader(new StringReader("[\"1\",\"2\"]"));
        reader.Read();

        var value = (ulong[]?)ULongArrayConverter.Singleton.ReadJson(reader, typeof(ulong[]), null, Newtonsoft.Json.JsonSerializer.Create());

        value.Should().Equal(1UL, 2UL);
        ULongArrayConverter.Singleton.CanConvert(typeof(ulong[])).Should().BeTrue();

        var stringWriter = new StringWriter();
        var writer = new JsonTextWriter(stringWriter);
        ULongArrayConverter.Singleton.WriteJson(writer, new ulong[] { 1, 2 }, Newtonsoft.Json.JsonSerializer.Create());
        writer.Flush();
        stringWriter.ToString().Should().Be("[\"1\",\"2\"]");

        var nullStringWriter = new StringWriter();
        var nullWriter = new JsonTextWriter(nullStringWriter);
        ULongArrayConverter.Singleton.WriteJson(nullWriter, null, Newtonsoft.Json.JsonSerializer.Create());
        nullWriter.Flush();
        nullStringWriter.ToString().Should().Be("null");

        ULongArrayConverter.Singleton.CanConvert(typeof(string[])).Should().BeFalse();

        var invalidReader = new JsonTextReader(new StringReader("[\"bad\"]"));
        invalidReader.Read();
        Action invalid = () => _ = ULongArrayConverter.Singleton.ReadJson(invalidReader, typeof(ulong[]), null, Newtonsoft.Json.JsonSerializer.Create());
        invalid.Should().Throw<JsonSerializationException>();

        var overflowReader = new JsonTextReader(new StringReader("[\"18446744073709551616\"]"));
        overflowReader.Read();
        Action overflow = () => _ = ULongArrayConverter.Singleton.ReadJson(overflowReader, typeof(ulong[]), null, Newtonsoft.Json.JsonSerializer.Create());
        overflow.Should().Throw<JsonSerializationException>();

        var nullReader = new JsonTextReader(new StringReader("null"));
        nullReader.Read();
        ULongArrayConverter.Singleton.ReadJson(nullReader, typeof(ulong[]), null, Newtonsoft.Json.JsonSerializer.Create()).Should().BeNull();

        var objectReader = new JsonTextReader(new StringReader("{}"));
        objectReader.Read();
        Action wrongShape = () => _ = ULongArrayConverter.Singleton.ReadJson(objectReader, typeof(ulong[]), null, Newtonsoft.Json.JsonSerializer.Create());
        wrongShape.Should().Throw<JsonSerializationException>();

        var emptyArrayReader = new JsonTextReader(new StringReader("[]"));
        emptyArrayReader.Read();
        var emptyArray = (ulong[]?)ULongArrayConverter.Singleton.ReadJson(emptyArrayReader, typeof(ulong[]), null, Newtonsoft.Json.JsonSerializer.Create());
        emptyArray.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ULongArrayConverter_WriteJson_ShouldSerializeAsStringArray()
    {
        // Arrange
        var sb = new StringBuilder();
        using var sw = new StringWriter(sb);
        using var writer = new JsonTextWriter(sw);
        var serializer = new Newtonsoft.Json.JsonSerializer();
        serializer.Converters.Add(ULongArrayConverter.Singleton);
        var value = new ulong[] { 1UL, 18446744073709551615UL }; // 1 and ulong.MaxValue

        // Act
        serializer.Serialize(writer, value);

        // Assert
        var json = sb.ToString();
        json.Should().Be("[\"1\",\"18446744073709551615\"]");
    }

    [Fact]
    public void DateTimeOffsetConverter_ShouldHandleValidAndInvalidInput()
    {
        var converter = new DateTimeOffsetConverter();
        converter.DateTimeStyles.Should().Be(System.Globalization.DateTimeStyles.AssumeUniversal);

        var settings = new Newtonsoft.Json.JsonSerializerSettings { Converters = new List<Newtonsoft.Json.JsonConverter> { converter } };
        var valid = Newtonsoft.Json.JsonConvert.DeserializeObject<DateTimeOffset>("\"2024-01-02T03:04:05Z\"", settings);
        valid.Should().Be(new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero));

        var invalid = Newtonsoft.Json.JsonConvert.DeserializeObject<DateTimeOffset?>("\"not-a-date\"", settings);
        invalid.Should().BeNull();

        // Non-nullable DateTimeOffset should throw on invalid format instead of returning null
        Action nonNullableAct = () => Newtonsoft.Json.JsonConvert.DeserializeObject<DateTimeOffset>("\"not-a-date\"", settings);
        nonNullableAct.Should().Throw<FormatException>();
    }

    [Fact]
    public void SystemTextJsonConverters_ShouldCreateAndUseInterfaceConverters()
    {
        var collectionFactory = new CollectionInterfaceConverterFactory<int>();
        collectionFactory.CanConvert(typeof(ICollection<int>)).Should().BeTrue();
        collectionFactory.CanConvert(typeof(ICollection<string>)).Should().BeFalse();
        collectionFactory.CreateConverter(typeof(ICollection<int>), new JsonSerializerOptions()).Should().BeOfType<CollectionConverter<int>>();

        var options = new JsonSerializerOptions();
        options.Converters.Add(collectionFactory);
        var collection = System.Text.Json.JsonSerializer.Deserialize<ICollection<int>>("[1,2,3]", options);
        collection.Should().BeEquivalentTo(new[] { 1, 2, 3 });

        var interfaceFactory = new InterfaceConverterFactory<ITestContract, TestImplementation>();
        interfaceFactory.CanConvert(typeof(ITestContract)).Should().BeTrue();
        interfaceFactory.CanConvert(typeof(TestImplementation)).Should().BeFalse();
        interfaceFactory.CreateConverter(typeof(ITestContract), new JsonSerializerOptions()).Should().BeOfType<InterfaceConverter<TestImplementation, ITestContract>>();

        var interfaceOptions = new JsonSerializerOptions();
        interfaceOptions.Converters.Add(interfaceFactory);
        var contract = System.Text.Json.JsonSerializer.Deserialize<ITestContract>("{\"Value\":42}", interfaceOptions);
        contract.Should().BeOfType<TestImplementation>();
        contract!.Value.Should().Be(42);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void InterfaceConverter_Write_ShouldSerializeAsImplementationType()
    {
        // Arrange
        var options = new JsonSerializerOptions();
        options.Converters.Add(new InterfaceConverterFactory<ITestContract, TestImplementation>());
        var value = new TestImplementation { Value = 42 };

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize<ITestContract>(value, options);

        // Assert
        json.Should().Contain("\"Value\":42");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CollectionConverter_Write_ShouldSerializeCollection()
    {
        // Arrange
        var options = new JsonSerializerOptions();
        options.Converters.Add(new CollectionInterfaceConverterFactory<int>());
        ICollection<int> values = new List<int> { 1, 2, 3 };

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(values, options);

        // Assert
        json.Should().Be("[1,2,3]");
    }


    // ── Phase 6 — Invalid input ──────────────────────────────────────────────────

    // 6.1 CanConvert(typeof(string)) → false
    [Theory]
    [InlineData(typeof(BigIntegerConverter))]
    [InlineData(typeof(BigRationalConverter))]
    [InlineData(typeof(DecimalConverter))]
    [InlineData(typeof(LongConverter))]
    [InlineData(typeof(ULongConverter))]
    public void CanConvert_WithStringType_ReturnsFalse(Type converterType)
    {
        var converter = (Newtonsoft.Json.JsonConverter)Activator.CreateInstance(converterType)!;
        converter.CanConvert(typeof(string)).Should().BeFalse();
    }

    // 6.2 ReadJson invalid string → JsonSerializationException
    [Theory]
    [InlineData(typeof(BigIntegerConverter), typeof(BigInteger))]
    [InlineData(typeof(DecimalConverter), typeof(decimal))]
    [InlineData(typeof(LongConverter), typeof(long))]
    [InlineData(typeof(ULongConverter), typeof(ulong))]
    public void ReadJson_WithInvalidString_ThrowsJsonSerializationException(
        Type converterType, Type targetType)
    {
        var converter = (Newtonsoft.Json.JsonConverter)Activator.CreateInstance(converterType)!;
        var settings = new Newtonsoft.Json.JsonSerializerSettings { Converters = { converter } };

        var json = $"\"{(targetType == typeof(decimal) ? "not-a-decimal" : "abc")}\"";
        var act = () => Newtonsoft.Json.JsonConvert.DeserializeObject(json, targetType, settings);
        act.Should().Throw<JsonSerializationException>();
    }

    // 6.3 Overflow → JsonSerializationException
    [Fact]
    public void LongConverter_ReadJson_WithOverflowValue_ThrowsJsonSerializationException()
    {
        var settings = new Newtonsoft.Json.JsonSerializerSettings { Converters = { new LongConverter() } };
        var act = () => Newtonsoft.Json.JsonConvert.DeserializeObject<long>("\"9999999999999999999999\"", settings);
        act.Should().Throw<JsonSerializationException>();
    }

    [Fact]
    public void ULongConverter_ReadJson_WithOverflowValue_ThrowsJsonSerializationException()
    {
        var settings = new Newtonsoft.Json.JsonSerializerSettings { Converters = { new ULongConverter() } };
        // ulong.MaxValue + 1 = 18446744073709551616
        var act = () => Newtonsoft.Json.JsonConvert.DeserializeObject<ulong>("\"18446744073709551616\"", settings);
        act.Should().Throw<JsonSerializationException>();
    }

    private static void AssertNewtonsoftConverter(Newtonsoft.Json.JsonConverter converter, object value, string expectedJson)
    {
        var settings = new Newtonsoft.Json.JsonSerializerSettings { Converters = new List<Newtonsoft.Json.JsonConverter> { converter } };
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(value, settings);
        json.Should().Be(expectedJson);
        var token = JToken.Parse(json);
        var roundTrip = token.ToObject(value.GetType(), Newtonsoft.Json.JsonSerializer.Create(settings));
        roundTrip.Should().Be(value);
    }

    private static void AssertNullReadAndWrite(Newtonsoft.Json.JsonConverter converter, Type targetType)
    {
        var reader = new JsonTextReader(new StringReader("null"));
        reader.Read();
        converter.ReadJson(reader, targetType, null, Newtonsoft.Json.JsonSerializer.Create()).Should().BeNull();

        var stringWriter = new StringWriter();
        var writer = new JsonTextWriter(stringWriter);
        converter.WriteJson(writer, null, Newtonsoft.Json.JsonSerializer.Create());
        writer.Flush();
        stringWriter.ToString().Should().Be("null");
    }

    private interface ITestContract
    {
        int Value { get; set; }
    }

    private sealed class TestImplementation : ITestContract
    {
        public int Value { get; set; }
    }
}
