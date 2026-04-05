## ADDED Requirements

### Requirement: Numeric Converters Reject Invalid Input
Numeric JSON converters SHALL throw `JsonSerializationException` when given unparseable or overflowing input.
`BigIntegerConverter`, `BigRationalConverter`, `DecimalConverter`, `LongConverter`,
and `ULongConverter` SHALL throw `JsonSerializationException` when `ReadJson`
encounters a string that cannot be parsed into the target numeric type.
`LongConverter` and `ULongConverter` SHALL also throw on overflow.

#### Scenario: Invalid string throws JsonSerializationException
- **WHEN** a JSON value of `"abc"` is deserialized via any numeric converter
- **THEN** `JsonSerializationException` is thrown

#### Scenario: Overflow throws for LongConverter
- **WHEN** a JSON value exceeding `long.MaxValue` is deserialized via `LongConverter`
- **THEN** `JsonSerializationException` is thrown

#### Scenario: Overflow throws for ULongConverter
- **WHEN** a JSON value exceeding `ulong.MaxValue` is deserialized via `ULongConverter`
- **THEN** `JsonSerializationException` is thrown

### Requirement: Numeric Converters CanConvert Returns False for Non-Matching Types
All numeric `JsonConverter` implementations SHALL return `false` from `CanConvert` for non-matching types.
All numeric `JsonConverter` implementations SHALL return `false` from `CanConvert`
when called with a type that is neither the target type nor its nullable variant.

#### Scenario: CanConvert returns false for string type
- **WHEN** `CanConvert(typeof(string))` is called on any numeric converter
- **THEN** `false` is returned
