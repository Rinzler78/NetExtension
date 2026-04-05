# Tasks: improve-test-coverage

## Phase 1 — HTTP Test Infrastructure (prerequisite for Phase 2)

### 1.1 WireMock.Net dependency
- [ ] 1.1.1 Add `<PackageReference Include="WireMock.Net" Version="1.*" />` to `Rinzler78.NetExtension.Tests.csproj`
- [ ] 1.1.2 Run `dotnet restore` and verify no version conflicts

### 1.2 WireMockServerFixture
- [ ] 1.2.1 Create `src/Rinzler78.NetExtension.Tests/TestHelpers/WireMockServerFixture.cs`
  — starts `WireMockServer.Start()`, overrides DNS resolver to bypass SSRF validator,
  exposes `BaseUrl`, implements `IDisposable` (stops server, restores DNS resolver)
- [ ] 1.2.2 Create `src/Rinzler78.NetExtension.Tests/Strings/StringHelperHttpTestCollection.cs`
  — `[CollectionDefinition]` with `ICollectionFixture<WireMockServerFixture>` to prevent
  concurrent DNS resolver override conflicts

---

## Phase 2 — StringHelper HTTP method tests (15 tests)

### 2.1 HttpGetStreamAsync
- [ ] 2.1.1 Happy path — WireMock serves `text/plain`, verify stream contains content
- [ ] 2.1.2 SSRF block — private IP URL throws `ArgumentException`
- [ ] 2.1.3 Timeout expired — WireMock delays 500ms, timeout=100ms → `OperationCanceledException`
- [ ] 2.1.4 CancellationToken pre-cancelled → `OperationCanceledException` immediately

### 2.2 HttpGetStringAsync (missing cases)
- [ ] 2.2.1 Happy path — WireMock serves known string, verify returned value
- [ ] 2.2.2 DNS rebinding — hostname mock resolves to 10.0.0.1 → `ArgumentException` at connect
- [ ] 2.2.3 127.x.x.x range — URL with `127.0.0.2` → `ArgumentException`

### 2.3 HttpGetAsync\<T\>
- [ ] 2.3.1 Happy path — WireMock serves JSON, verify deserialized object
- [ ] 2.3.2 Response cannot deserialize to T (returns null) → `InvalidOperationException`

### 2.4 HttpPostString
- [ ] 2.4.1 Happy path — WireMock echoes body, verify response string
- [ ] 2.4.2 SSRF block on POST URL → `ArgumentException`
- [ ] 2.4.3 Null payload (`obj = null`) — POST with empty body, server response returned

### 2.5 HttpPost\<TReq, TImpl, TReturn\>
- [ ] 2.5.1 Happy path — POST + deserialized typed response returned
- [ ] 2.5.2 Response deserializes to null → `InvalidOperationException`

---

## Phase 3 — ObservableObject gaps (8 tests)

- [ ] 3.1 `SetProperty` with `IObservableObject` property:
  — assign depA → attach; assign depB → detach depA + attach depB; assign null → detach depB
  — verify `Dependencies` content at each step
- [ ] 3.2 `SetProperty` with `propertyChanged` callback:
  — callback receives correct `OldValue` and `NewValue`
- [ ] 3.3 `SetProperty` with `propertyChanged` callback when value unchanged:
  — callback NOT invoked when value does not change
- [ ] 3.4 `Dispose()` called twice → no exception, `Dependencies` remains empty
- [ ] 3.5 `Dispose(false)` via exposed protected method → `PropertyChanged` NOT cleared
- [ ] 3.6 `OnDependenciesPropertyChanged` base implementation:
  — use an `ObservableObject` subclass without override; trigger dep property change → no throw
- [ ] 3.7 `AttachDependencies` large collection (11 items) + duplicate → count stays at 11
- [ ] 3.8 `DetachDependencies` large collection (11 items) with duplicates in input → no error

---

## Phase 4 — ObjectExtension dead code + null-callback branches

- [ ] 4.1 Remove redundant `if (targetPropertyInfo.CanWrite)` check in `CopyTo`
  (line 56 of `ObjectExtension.cs`) — already filtered by `.Where(x => x.CanWrite)`
- [ ] 4.2 Add test: `CopyTo` type-mismatch exception **without** callback → no throw
- [ ] 4.3 Add test: `CopyTo` null target object **without** callback → no throw
- [ ] 4.4 Add test: `CopyTo` throwing setter **without** callback → no throw

---

## Phase 5 — NetworkHelper + ReusableTask branch gaps (4 tests)

- [ ] 5.1 `NetworkHelper.IsPortOpened(string, uint)` — `Resolve()` returns null:
  use internal `Resolve` mock pattern or pass hostname that DNS returns empty for → `false`
- [ ] 5.2 `NetworkHelper.IsPortOpened(string, uint)` — `host == null` → returns `false`
- [ ] 5.3 `ReusableTask.Invoke()` — call twice without await → same `Task` returned
  (verify `ReferenceEquals(t1, t2)`)
- [ ] 5.4 `ReusableTask.Dispose()` — called twice → no exception

---

## Phase 6 — JSON Converter invalid input (15 tests)

For each converter: `BigIntegerConverter`, `BigRationalConverter`, `DecimalConverter`,
`LongConverter`, `ULongConverter`:

- [ ] 6.1 `CanConvert(typeof(string))` returns `false` (1 test per converter = 5 tests)
- [ ] 6.2 `ReadJson` with invalid string `"abc"` throws `JsonSerializationException`
  (1 `[Theory]` with MemberData covering all 5 converters)
- [ ] 6.3 `ReadJson` with overflow value throws `JsonSerializationException`
  — `LongConverter`: value `"9999999999999999999999"`
  — `ULongConverter`: value `"18446744073709551616"` (ulong.MaxValue + 1)
  (1 test per overflow-capable converter = 2 tests)

---

## Phase 7 — Remaining gaps (6 tests + 1 new file)

### 7.1 ObservableRangeCollection
- [ ] 7.1.1 `AddRange(empty, Reset)` → no `CollectionChanged` event raised

### 7.2 BaseRestApi
- [ ] 7.2.1 Constructor with path starting with `/` → same URI as path without `/`

### 7.3 BaseRestApiExt
- [ ] 7.3.1 `bool true` in args → query string contains `param=true`
- [ ] 7.3.2 All args values are null → no `?` appended to URL
- [ ] 7.3.3 Key with special character (space) → key is URL-encoded

### 7.4 UpdatablePropertyExtension (new test file)
- [ ] 7.4.1 Create `src/Rinzler78.NetExtension.Tests/Objects/UpdatablePropertyExtensionTests.cs`
- [ ] 7.4.2 `GetAll` with empty array → completes without error
- [ ] 7.4.3 `GetAll` with N properties → all initialized, all values available
- [ ] 7.4.4 `UpdateAll` with empty array → completes without error
- [ ] 7.4.5 `UpdateAll` with N properties → all refreshed

---

## Phase 8 — Validation & Changelog

- [ ] 8.1 Run `dotnet test --filter "Category!=E2E" --collect:"XPlat Code Coverage"` — all pass
- [ ] 8.2 Run coverage report — verify line >= 97%, branch >= 95%
- [ ] 8.3 Update `CHANGELOG.md` `[Unreleased]` section
- [ ] 8.4 Update CI quality gate thresholds in `docs/testing-quality/quality-gates.json`
  from `0.90`/`0.80` to `0.97`/`0.95`
- [ ] 8.5 Mark all tasks `[x]` in this file
