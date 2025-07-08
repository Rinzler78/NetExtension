# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Rinzler78.NetExtension is a .NET 8.0 utility library containing common extension methods and helper classes for various .NET operations. This is a supporting library that appears to be part of the larger OsmoBot project ecosystem.

## Common Development Commands

### Building and Testing
```bash
# Build the solution
dotnet build src/Rinzler78.NetExtension.sln

# Build in Release mode
dotnet build src/Rinzler78.NetExtension.sln --configuration Release

# Build in Debug mode (default)
dotnet build src/Rinzler78.NetExtension.sln --configuration Debug

# Clean build artifacts
dotnet clean src/Rinzler78.NetExtension.sln

# Restore NuGet packages
dotnet restore src/Rinzler78.NetExtension.sln
```

### NuGet Package Management
```bash
# Build NuGet package
dotnet pack src/Rinzler78.NetExtension.Standard/Rinzler78.NetExtension.Standard.csproj
```

## Architecture Overview

### Core Components

**ObservableObject System** (`Observable/`)
- `ObservableObject`: Base class implementing INotifyPropertyChanged with dependency tracking
- `IObservableObject`: Interface for observable pattern implementation
- `ObservableRangeCollection`: Enhanced observable collection with range operations
- Event-driven architecture with automatic property change notifications
- Thread-safe property setting with dependency management

**JSON Serialization** (`Json/`)
- `JsonHelper`: Extension methods for JSON serialization/deserialization using Newtonsoft.Json
- Custom converters for various data types (BigInteger, BigRational, DateTime, etc.)
- File-based JSON operations with error handling
- Interface and collection converter factories

**Utility Extensions** - Various helper classes organized by category:
- `Array/ArrayExtension`: Array manipulation utilities
- `Linq/EnumerableExtension`: LINQ extension methods
- `Math/`: Mathematical operations (BigInteger, Decimal helpers)
- `Strings/StringHelper`: String manipulation utilities
- `Types/`: Type system utilities (Activator, Convert, Type extensions)
- `Tasks/`: Task and async operation helpers
- `Process/ProcessHelper`: Process execution utilities
- `Network/NetworkHelper`: Network-related utilities
- `Csv/CsvExtension`: CSV file handling
- `Enums/`: Enum parsing and manipulation utilities

### Key Dependencies

**External Packages:**
- `Newtonsoft.Json` (13.0.3) - JSON serialization
- `CsvHelper` (30.0.1) - CSV file processing
- `System.Data.HashFunction.CRC` (2.0.0) - CRC hashing
- `bigrational` (1.0.0.7) - BigRational number support
- `MeziAntou.Analyzer` (Debug only) - Code analysis

### Project Structure

```
src/
├── Rinzler78.NetExtension.sln          # Main solution file
├── Rinzler78.NetExtension.Standard/    # Main library project
│   ├── Array/                          # Array extensions
│   ├── Command/                        # Command utilities
│   ├── Csv/                           # CSV handling
│   ├── Dates/                         # Date/time utilities
│   ├── Enums/                         # Enum utilities
│   ├── FootPrint/                     # Hashing utilities (CRC, SHA)
│   ├── Geo/                           # Geographic utilities
│   ├── Json/                          # JSON serialization
│   ├── Linq/                          # LINQ extensions
│   ├── Math/                          # Mathematical utilities
│   ├── Measure/                       # Performance measurement
│   ├── Network/                       # Network utilities
│   ├── Objects/                       # Object manipulation
│   ├── Observable/                    # Observable pattern implementation
│   ├── Process/                       # Process execution
│   ├── Rest/                          # REST API utilities
│   ├── Strings/                       # String utilities
│   ├── Tasks/                         # Task utilities
│   └── Types/                         # Type system utilities
└── Rinzler78.NetExtension.NuGet/      # NuGet packaging project
```

## Development Notes

### Configuration Features
- Targets .NET 8.0 with latest C# language features
- Unsafe code blocks enabled for performance-critical operations
- Nullable reference types enabled
- Debug-only analyzer integration with MeziAntou.Analyzer

### Observable Pattern Implementation
- Thread-safe property change notifications
- Automatic dependency tracking between observable objects
- Conditional compilation for debugging (TRACE_PROPERTY_CHANGED, etc.)
- Proper disposal pattern with dependency cleanup

### Performance Considerations
- Unsafe code blocks allowed for performance-critical sections
- Efficient collection operations with range support
- Reusable task implementations
- Performance measurement utilities included

### Code Patterns
- Extensive use of extension methods for fluent API design
- Generic constraints for type safety
- Proper async/await patterns throughout
- Immutable data structures where appropriate
- Comprehensive error handling with null checks