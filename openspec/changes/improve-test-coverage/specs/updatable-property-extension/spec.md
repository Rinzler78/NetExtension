## ADDED Requirements

### Requirement: GetAll Fetches All Properties Concurrently
`GetAll(UpdatableProperty[])` SHALL call `Get()` on each property concurrently
via `Task.WhenAll` and complete when all are initialized.

#### Scenario: All properties fetched in parallel
- **WHEN** `GetAll` is called on an array of N properties
- **THEN** all properties are initialized and their values are accessible

#### Scenario: Empty array completes immediately
- **WHEN** `GetAll` is called with an empty array
- **THEN** the returned task completes without error

### Requirement: UpdateAll Refreshes All Properties Concurrently
`UpdateAll(UpdatableProperty[])` SHALL call `Update()` on each property concurrently.

#### Scenario: All properties updated in parallel
- **WHEN** `UpdateAll` is called on an array of N properties
- **THEN** all properties are refreshed

#### Scenario: Empty array completes immediately
- **WHEN** `UpdateAll` is called with an empty array
- **THEN** the returned task completes without error
