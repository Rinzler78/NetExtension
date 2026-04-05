## ADDED Requirements

### Requirement: CopyTo Handles Exceptions Without Callback
`CopyTo` SHALL silently swallow `ArgumentException`, `TargetException`, and
`TargetInvocationException` when no `onCopyToFailedForProperty` callback is provided.

#### Scenario: Type-mismatch copy without callback does not throw
- **WHEN** `CopyTo` encounters a type mismatch and no callback is supplied
- **THEN** the incompatible property is skipped and no exception propagates

#### Scenario: Throwing setter without callback does not throw
- **WHEN** the target property setter throws and no callback is supplied
- **THEN** the exception is swallowed silently

## MODIFIED Requirements

### Requirement: CopyTo Redundant CanWrite Check Removed
The internal `if (targetPropertyInfo.CanWrite)` check inside `CopyTo` SHALL be
removed. The `targetProperties` list is already filtered by `.Where(x => x.CanWrite)`,
making this check dead code.

#### Scenario: All writable properties are copied
- **WHEN** `CopyTo` is called on objects with matching writable properties
- **THEN** all matching properties are copied without any guard exception
