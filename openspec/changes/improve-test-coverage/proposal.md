# Change: Improve Test Coverage to 97%+ Line / 95%+ Branch

## Why
The current test suite reaches 94.2% line coverage and 90.9% branch coverage.
A full code review identified 52 missing tests across 9 capabilities.
The most critical gap is `StringHelper.Http.cs`: 5 public HTTP methods (HttpGetStreamAsync,
HttpGetAsync, HttpPostString, HttpPost, and the typed variants) have 0% coverage despite
containing non-trivial deserialization and SSRF-protection logic.
Secondary gaps in ObservableObject, JSON converters, ObjectExtension, and infrastructure
classes reduce confidence in core library behavior.

## What Changes

### Phase 1 — HTTP Test Infrastructure (prerequisite)
- Add `WireMock.Net` test dependency to `Rinzler78.NetExtension.Tests.csproj`
- Create `TestHelpers/WireMockServerFixture.cs` — shared `IDisposable` fixture that starts
  an embedded HTTP server on a random port and overrides the DNS resolver for SSRF bypass
- Create `Strings/StringHelperHttpTests.cs` — new test file (replaces absent coverage)

### Phase 2 — StringHelper HTTP methods (~15 tests)
All 5 uncovered HTTP methods + missing cases on `HttpGetStringAsync`:
- `HttpGetStreamAsync` — happy path, SSRF block, timeout, cancellation
- `HttpGetStringAsync` — happy path, DNS rebinding via mock, 127.x.x.x range
- `HttpGetAsync<T>` — deserialization OK, `InvalidOperationException` on null
- `HttpPostString` — happy path, SSRF block, null payload
- `HttpPost<TReq,TImpl,TReturn>` — round-trip, `InvalidOperationException`

### Phase 3 — ObservableObject gaps (~8 tests)
- `SetProperty` with `IObservableObject` property (auto attach/detach)
- `SetProperty` with `propertyChanged` callback
- `Dispose()` called twice (idempotent)
- `Dispose(false)` — finalizer path
- `OnDependenciesPropertyChanged` base implementation (no override)
- `AttachDependencies` large collection (>= 10) with duplicate

### Phase 4 — ObjectExtension dead code + null-callback branches (~4 changes)
- Remove redundant `if (targetPropertyInfo.CanWrite)` (dead code)
- Add 3 tests for `CopyTo` exception paths without callback (null delegate branch)

### Phase 5 — Network + ReusableTask branches (~4 tests)
- `NetworkHelper.IsPortOpened(string, uint)` — `ip is null` path (mock DNS resolver)
- `NetworkHelper.IsPortOpened(string, uint)` — `host == null` → silent false
- `ReusableTask.Invoke()` — second call while running returns same `Task`
- `ReusableTask.Dispose()` — called twice, idempotent

### Phase 6 — JSON Converters invalid input (~15 tests)
For each of the 5 numeric converters (BigInteger, BigRational, Decimal, Long, ULong):
- `CanConvert(typeof(string))` → false
- `ReadJson` with invalid string → `JsonSerializationException`
- `ReadJson` with overflow value (Long, ULong only) → `JsonSerializationException`

### Phase 7 — Remaining gaps (~6 tests + 1 new file)
- `ObservableRangeCollection.AddRange(empty, Reset)` — no event raised
- `BaseRestApi` — path with leading slash normalized by `TrimStart('/')`
- `BaseRestApiExt` — `bool true` → `"true"`, all-null args → no `?`, special char key
- New file `UpdatablePropertyExtensionTests.cs` — `GetAll`/`UpdateAll` empty + N elements

## Impact
- Affected specs: test-http-infrastructure (new), string-helper-http (new),
  observable-object, observable-range-collection, json-helper, network-helper,
  reusable-task, rest-api (new), updatable-property-extension (new), object-extension (new)
- Affected code (source changes):
  - `src/Rinzler78.NetExtension.Standard/Objects/ObjectExtension.cs`
    — remove redundant `if (CanWrite)` check
  - `src/Rinzler78.NetExtension.Tests/Rinzler78.NetExtension.Tests.csproj`
    — add `WireMock.Net` package reference
- New test files (8):
  - `TestHelpers/WireMockServerFixture.cs`
  - `Strings/StringHelperHttpTests.cs`
  - `Objects/UpdatablePropertyExtensionTests.cs`
  - (additions to existing test files for all other phases)
- Coverage target: line >= 97%, branch >= 95%
- No breaking changes to public API
