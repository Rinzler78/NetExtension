# 🚀 Rinzler78.NetExtension

[![CI](https://github.com/Rinzler78/NetExtension/actions/workflows/ci.yml/badge.svg?branch=master)](https://github.com/Rinzler78/NetExtension/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Rinzler78.NetExtension?logo=nuget&label=NuGet)](https://www.nuget.org/packages/Rinzler78.NetExtension)
[![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue?logo=dotnet&style=flat-square)](https://dotnet.microsoft.com/)

A comprehensive collection of .NET 8 extensions and utilities: observable MVVM helpers (with source-generated properties), JSON utilities, SSRF-protected HTTP, network tools, string operations, async patterns, CSV, hash functions, and more.

## 📦 Installation

```bash
dotnet add package Rinzler78.NetExtension
```

```xml
<PackageReference Include="Rinzler78.NetExtension" Version="0.0.0.2" />
```

## 🚀 Quick Start

```csharp
using Rinzler78.NetExtension.Json;
using Rinzler78.NetExtension.Observable;
using Rinzler78.NetExtension.Strings;

// ── Observable properties — source-generated, zero boilerplate ────────────
public partial class PersonViewModel : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private int    _age;
    // Generates:  public string Name { get => _name; set => SetProperty(ref _name, value); }
    //             public int    Age  { get => _age;  set => SetProperty(ref _age,  value);  }
}

// ── JSON ──────────────────────────────────────────────────────────────────
string json     = myObject.SerializeObject(Formatting.Indented);
MyType restored = json.DeSerialize<MyType>()!;
MyConfig cfg    = "/app/config.json".DeSerializeObjectFromFile<MyConfig>();

// ── String utilities ──────────────────────────────────────────────────────
bool   ok      = "user@example.com".IsValidEmail();        // true
double sim     = "kitten".CalculateSimilarity("sitting");  // 0.57
string slug    = "hello_world".ToPascalCase();             // "HelloWorld"
string size    = 1_073_741_824L.GetBytesReadable();        // "1 GB"

// ── Collections ───────────────────────────────────────────────────────────
var list = new List<int> { 1, 2, 3 };
list.Set(new[] { 2, 3, 4 });   // removes 1, adds 4 → [2, 3, 4]

// ── HTTP (SSRF-protected) ─────────────────────────────────────────────────
string body = await "https://api.example.com/data".HttpGetStringAsync();
MyDto  dto  = await "https://api.example.com/dto".HttpGetAsync<MyDto>();

// ── Tasks ─────────────────────────────────────────────────────────────────
var tasks = new List<Task> { Task.Delay(100), Task.Delay(200) };
await tasks.WhenAll();
```

## 📚 Modules

### 👁️ Observable Objects

MVVM-ready base class with source-generated properties, dependency tracking, and batch collection operations.

```csharp
// Source-generated properties — no manual SetProperty boilerplate
public partial class OrderViewModel : ObservableObject
{
    [ObservableProperty] private string  _title       = string.Empty;
    [ObservableProperty] private decimal _totalPrice;
    [ObservableProperty] private bool    _isConfirmed;
}

// Batch collection — single CollectionChanged notification per operation
var items = new ObservableRangeCollection<string>();
items.AddRange(new[] { "alpha", "beta", "gamma" });  // one event
items.RemoveRange(new[] { "beta" });
items.ReplaceRange(new[] { "delta" });

// Manual SetProperty with change callback
public partial class UserViewModel : ObservableObject
{
    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value,
            changed => Console.WriteLine($"{changed.OldValue} → {changed.NewValue}"));
    }
}
```

### 📝 JSON

Serialization helpers and custom converters for Newtonsoft.Json.

```csharp
// Serialize / deserialize
string  json = myObject.SerializeObject(Formatting.Indented);
MyType  obj  = json.DeSerialize<MyType>()!;

// Custom converters for numeric and date types
var settings = new JsonSerializerSettings();
settings.Converters.Add(BigIntegerConverter.Singleton);
settings.Converters.Add(DecimalConverter.Singleton);
settings.Converters.Add(DateTimeOffsetConverter.Singleton);

MyDto result = json.DeSerialize<MyDto>(settings)!;

// File-based JSON (path-validated against traversal attacks)
MyConfig cfg = "/app/config.json".DeSerializeObjectFromFile<MyConfig>();
```

### 🔤 Strings & HTTP

String utilities and built-in SSRF-protected HTTP helpers.

```csharp
// Validation & transformation
bool   valid = "user@example.com".IsValidEmail();
string slug  = "My Article".ToPascalCase();          // "MyArticle"
string cap   = "xml parser".BeginByUpperCase();      // "Xml parser"
string size  = 2_147_483_648L.GetBytesReadable();    // "2 GB"
double dist  = "kitten".CalculateSimilarity("sitting"); // 0.57

// HTTP — blocks localhost, RFC 1918, APIPA, IPv6 ULA/link-local (SSRF protection)
string   body   = await "https://api.example.com/items".HttpGetStringAsync();
ItemDto  dto    = await "https://api.example.com/item/1".HttpGetAsync<ItemDto>();
string   result = await "https://api.example.com/items".HttpPostString(newItem);
```

### ⚡ Tasks & Async

```csharp
// WhenAll as fluent extension
await urls.Select(url => FetchAsync(url)).ToList().WhenAll();

// Null-safe helpers
bool running   = myTask.IsRunning();
bool cancelled = myCts.IsCancellationRequested();

// Single-instance re-runnable task (second call returns in-flight task)
var worker = new ReusableTask(() => DoWork());
await worker.Invoke();
worker.Cancel();
```

### 🌐 Network

```csharp
bool     open     = IPAddress.Loopback.IsPortOpened(8080u);
bool     open2    = "192.168.1.1".IsPortOpened(80u);
IPAddress? addr   = "api.example.com".Resolve();
PingReply  reply  = "8.8.8.8".Ping();
string     ep     = new Uri("https://api.example.com/v1/").ToFullEnpoint();
// → "https://api.example.com/v1"
```

### 🔢 Math & BigInteger

```csharp
// Safe conversions with overflow protection
ulong   u = (new BigInteger(ulong.MaxValue) + 1).ToULong();    // → ulong.MaxValue
decimal d = (new BigInteger(decimal.MaxValue) + 1).ToDecimal(); // → decimal.MaxValue

// Decimal math (exact for integer exponents)
decimal result = principal * DecimalHelper.Pow(1 + rate, years);
decimal ln     = value.Log();
```

### 🗂️ Collections

```csharp
// Synchronise: adds missing items, removes stale ones
var current = new ObservableCollection<string> { "a", "b", "c" };
current.Set(new[] { "b", "c", "d" });  // → ["b", "c", "d"]
```

### 📄 CSV

```csharp
IEnumerable<Product>? rows  = "products.csv".LoadCsv<Product>(separator: ',', hasHeaderRecord: true);
IEnumerable<Product>? async = await "products.csv".LoadCsvAsync<Product>();
```

### 🌍 REST

```csharp
public class ProductApi : BaseRestApi
{
    public ProductApi() : base("https://api.example.com", "v1/products") { }

    public Task<Product?> GetAsync(int id)
    {
        var url = BaseUri.AbsoluteUri.CreateUrlPath(
            new Dictionary<string, object?> { ["id"] = id });
        return url.HttpGetAsync<Product>();
    }
}
```

### 🔏 Hash & Footprint

```csharp
uint   crc32  = "hello".GenerateCrc32();
ulong  crc64  = "hello".GenerateCrc64();
byte[] sha512 = "hello".GenerateSha512();
```

### 📅 Dates & Time

```csharp
// Split a date range into daily slots
TimeSlot[] slots = DatesHelper.ToTimeSlots(
    DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, daysSlotDuration: 1);

TimeSpan elapsed = myDate.ElapsedUtc();
```

### 🗃️ Enums

```csharp
MyEnum? val      = "pending_review".Parse<MyEnum>();
Target  converted = sourceValue.Convert<SourceEnum, TargetEnum>();
```

## 🏗️ Architecture

```
Rinzler78.NetExtension/
├── Array/          # Array → byte[] helpers (unsafe, CRC-ready)
├── Command/        # ICommand.TryExecute extension
├── ConsoleExt.cs   # Thread-safe coloured console output
├── Csv/            # CSV load/parse via CsvHelper
├── Dates/          # TimeSlot, ToTimeSlots, ElapsedUtc
├── Enums/          # Parse, Convert, GlobalEnumParser (with cache)
├── FootPrint/      # CRC32, CRC64, SHA512
├── Geo/            # Distance conversions (miles ↔ km)
├── Json/           # JsonHelper + custom Newtonsoft converters
├── Linq/           # TryAggregate
├── Math/           # BigIntegerExt, DecimalHelper
├── Measure/        # MeasureDuration, MeasureDurationAsync
├── Network/        # IsPortOpened, Resolve, Ping
├── Objects/        # CopyTo, IsEqualTo, UpdatableProperty
├── Observable/     # ObservableObject, ObservableRangeCollection, [ObservableProperty]
├── Process/        # WaitProcessOutputs(Async)
├── Rest/           # BaseRestApi, CreateUrlPath
├── Strings/        # StringHelper (string utils + SSRF-protected HTTP)
├── Tasks/          # TaskHelper, ReusableTask
├── Types/          # ActivatorHelper, ConvertHelper, TypeExtensions
└── UriHelper.cs    # URI normalisation
```

## 🛠️ Development

### Setup

```bash
git clone https://github.com/Rinzler78/NetExtension.git
cd NetExtension
dotnet restore src/Rinzler78.NetExtension.sln
pip install pre-commit && pre-commit install
```

### Scripts

```bash
./scripts/local/build.sh                   # build (Release)
./scripts/local/test.sh --suite unit       # unit tests only
./scripts/local/test.sh --suite all        # full test suite
./scripts/local/quality.sh                 # build + all tests + coverage gate
```

### CI/CD

| Job | Trigger | Purpose |
|-----|---------|---------|
| `build-and-test` | push / PR | Debug + Release build, coverage gate, NuGet pack |
| `compatibility-test` | push / PR | Ubuntu · Windows · macOS |
| `lint` | push / PR | `dotnet format` + `TreatWarningsAsErrors` |
| `release` | tag `v*` | Build, pack, GitHub Release, publish NuGet |

## 🤝 Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for setup instructions, code style, and PR process.

## 📄 License

MIT — see [LICENSE](LICENSE).

---

**Made with ❤️ by [Rinzler78](https://github.com/Rinzler78)**
