# Capability: JSON Helper

## Purpose
Provides extension methods for JSON serialization/deserialization using Newtonsoft.Json,
and System.Text.Json converters for interface types and special numeric types.
Includes secure file-based deserialization with path validation.

## Requirements

### Requirement: Object Serialization
`SerializeObject(this object)` and its overloads SHALL serialize any object to a JSON
string using Newtonsoft.Json, respecting provided `Formatting` and `JsonSerializerSettings`.

#### Scenario: Basic serialization
- **WHEN** an object is serialized
- **THEN** a valid JSON string representation is returned

### Requirement: Typed Deserialization
`Deserialize<T>(this string?)` SHALL return `default(T)` for null or empty input,
and the deserialized object for valid JSON.

#### Scenario: Null input
- **WHEN** `Deserialize<T>` is called with null or empty string
- **THEN** `default(T)` is returned without throwing

#### Scenario: Valid JSON
- **WHEN** `Deserialize<T>` is called with valid JSON
- **THEN** the deserialized object of type `T` is returned

### Requirement: Secure File Deserialization
`DeserializeObjectFromFile<T>(this string filePath)` SHALL validate the file path before
reading, using a path allowlist restricted to data file extensions.
Path traversal sequences (`..`) and home directory expansion (`~`) SHALL be rejected.
Only the following extensions are permitted: `.json .xml .csv .txt .yaml .yml .toml .ini .conf`.

#### Scenario: Valid path and extension
- **WHEN** called with a path to an existing `.json` file
- **THEN** the file is read and deserialized

#### Scenario: Path traversal rejected
- **WHEN** called with a path containing `..`
- **THEN** `ArgumentException` is thrown

#### Scenario: Disallowed extension
- **WHEN** called with a path ending in `.exe` or `.dll`
- **THEN** `ArgumentException` is thrown

### Requirement: STJ Interface Converter
`InterfaceConverter<TImpl, TInterface>` SHALL serialize using the concrete
`TImplementation` type and deserialize by constructing a `TImplementation` instance.

#### Scenario: Serialize through interface
- **WHEN** an `TInterface` value is serialized
- **THEN** the JSON output reflects the concrete `TImplementation` properties

#### Scenario: Deserialize to implementation
- **WHEN** valid JSON is deserialized as `TInterface`
- **THEN** a `TImplementation` instance is returned
