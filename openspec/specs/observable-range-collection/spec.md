# Capability: Observable Range Collection

## Purpose
`ObservableRangeCollection<T>` extends `ObservableCollection<T>` with bulk operations
(AddRange, RemoveRange, ReplaceRange) that raise a single change notification instead of
one per item, with thread-safe internal list access.

## Requirements

### Requirement: AddRange With Single Notification
`AddRange(IEnumerable<T>, NotifyCollectionChangedAction)` SHALL add all items from
the collection to the internal list and raise exactly one `CollectionChanged` event.
`notificationMode` MUST be `Add` (default) or `Reset`; other values SHALL throw `ArgumentException`.

#### Scenario: Add mode notification
- **WHEN** `AddRange` is called with mode `Add` and a non-empty collection
- **THEN** a single `CollectionChanged` event with action `Add` is raised, containing all added items

#### Scenario: Reset mode notification
- **WHEN** `AddRange` is called with mode `Reset`
- **THEN** a single `CollectionChanged` event with action `Reset` is raised

#### Scenario: Null collection throws
- **WHEN** `AddRange` is called with `null`
- **THEN** `ArgumentNullException` is thrown

#### Scenario: Invalid mode throws
- **WHEN** `AddRange` is called with a mode other than `Add` or `Reset`
- **THEN** `ArgumentException` is thrown

### Requirement: AddRange Does Not Double-Enumerate
`AddRange` SHALL NOT enumerate the source `IEnumerable<T>` more than once.
The list of items added and the notification payload SHALL be constructed from
a single pass over the source collection.

#### Scenario: Forward-only enumerable
- **WHEN** `AddRange` is called with a forward-only `IEnumerable<T>` (e.g., LINQ query)
- **THEN** all items are added to the collection and the `Add` notification contains
  all added items (no empty or partial notification)

### Requirement: RemoveRange With Single Notification
`RemoveRange(IEnumerable<T>, NotifyCollectionChangedAction)` SHALL remove matching items
and raise exactly one `CollectionChanged` event. Accepts `Remove` (default) or `Reset`.

#### Scenario: Remove mode
- **WHEN** `RemoveRange` is called with mode `Remove` and items that exist in the collection
- **THEN** those items are removed and a single `Remove` event is raised

### Requirement: ReplaceRange Atomic Replacement
`ReplaceRange(IEnumerable<T>)` SHALL clear the collection and add the new items atomically,
raising a single `Reset` notification. If both the old and new states are empty, no
notification SHALL be raised.

#### Scenario: Replace with new items
- **WHEN** `ReplaceRange` is called with a non-empty collection
- **THEN** existing items are cleared, new items are added, and a single `Reset` is raised

#### Scenario: Both empty — no event
- **WHEN** `ReplaceRange` is called on an empty collection with an empty collection
- **THEN** no `CollectionChanged` event is raised
