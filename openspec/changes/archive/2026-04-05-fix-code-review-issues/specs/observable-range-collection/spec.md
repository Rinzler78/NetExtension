## MODIFIED Requirements

### Requirement: AddRange Does Not Double-Enumerate
`AddRange` SHALL NOT enumerate the source `IEnumerable<T>` more than once.
The items SHALL be snapshotted into a `List<T>` at the start of `AddRange` (before
calling `AddRangeCore`). The same snapshot SHALL be passed to `AddRangeCore` and
used as the notification payload.

#### Scenario: Forward-only enumerable — all items added
- **WHEN** `AddRange` is called with a forward-only `IEnumerable<T>` (e.g., LINQ query)
- **THEN** all items are added to the collection and the `Add` notification contains
  all added items (the notification payload is not empty)

#### Scenario: Add mode with List source — no copy overhead
- **WHEN** `AddRange` is called with a source that is already a `List<T>`
- **THEN** no extra copy is made; the existing list is used directly as the snapshot
