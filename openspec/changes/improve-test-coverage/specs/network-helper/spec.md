## ADDED Requirements

### Requirement: IsPortOpened Returns False When Host Resolves to No Address
`IsPortOpened(string, uint)` SHALL return `false` when `Resolve()` returns `null`.

#### Scenario: Unresolvable host returns false without throwing
- **WHEN** `IsPortOpened` is called with a hostname whose `Resolve()` returns null
- **THEN** `false` is returned

#### Scenario: Null host returns false without throwing
- **WHEN** `IsPortOpened` is called with `host = null`
- **THEN** `false` is returned
