## ADDED Requirements

### Requirement: AddRange Reset Mode With Empty Collection Raises No Event
`AddRange` with `Reset` mode and an empty collection SHALL raise no `CollectionChanged` event.
When `AddRange` is called with `NotifyCollectionChangedAction.Reset` and an empty
collection, no `CollectionChanged` event SHALL be raised.

#### Scenario: Empty collection in Reset mode produces no notification
- **WHEN** `AddRange` is called with an empty enumerable and `Reset` mode
- **THEN** no `CollectionChanged` event is raised and the collection remains empty
