# Capability: Updatable Property Extension

## Purpose
`UpdatablePropertyExtension` provides batch operations on arrays of `UpdatableProperty`:
`GetAll` and `UpdateAll` execute all properties concurrently via `Task.WhenAll`.

## Requirements

### Requirement: GetAll Fetches All Properties Concurrently
`GetAll(UpdatableProperty[])` SHALL call `Get()` on each property in the array
concurrently and await all results via `Task.WhenAll`.

#### Scenario: All properties fetched in parallel
- **WHEN** `GetAll` is called on an array of N properties
- **THEN** all `Get()` calls complete and all cached values are initialized

#### Scenario: Empty array completes immediately
- **WHEN** `GetAll` is called with an empty array
- **THEN** the returned task completes without error

### Requirement: UpdateAll Refreshes All Properties Concurrently
`UpdateAll(UpdatableProperty[])` SHALL call `Update()` on each property concurrently.

#### Scenario: All properties updated in parallel
- **WHEN** `UpdateAll` is called on an array of N properties
- **THEN** all `Update()` calls complete

#### Scenario: Empty array completes immediately
- **WHEN** `UpdateAll` is called with an empty array
- **THEN** the returned task completes without error
