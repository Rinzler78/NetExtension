## Context
Critical bugs found in code review require targeted fixes before v0.2.0. All changes
are in existing public or internal types; no new external dependencies are introduced.
The library targets .NET 10 only, so all modern APIs (ConnectAsync, Task.WhenAll, etc.)
are available.

## Goals / Non-Goals
- Goals:
  - Eliminate all confirmed bugs (deadlock, resource leak, race condition)
  - Fix double-enumeration in ObservableRangeCollection
  - Replace deprecated APM socket API
  - Harden CI/CD (version consistency, quality gates, test categorization)
  - Clean up project build configuration
- Non-Goals:
  - Adding new library capabilities or extension methods
  - Multi-targeting (netstandard2.x, net8.0, net9.0) — tracked separately
  - JSON library unification (Newtonsoft vs STJ) — tracked separately
  - Performance optimizations (reflection caching, stackalloc) — tracked separately
  - XML documentation completeness — tracked separately

## Decisions

### ProcessHelper: Task.WhenAll for stream reading
**Decision:** Use `Task.WhenAll(ReadToEndAsync(stdout), ReadToEndAsync(stderr))` before
`WaitForExitAsync()` in both the sync (converted to async internally) and async overloads.

**Rationale:** The OS pipe buffer is typically 64 KB. A process writing more than that to
one stream while the other is not being read causes deadlock. Parallel reads prevent this.

**Sync overload:** The existing sync signature `WaitProcessOutputs` is kept for API compat
but internally uses `.GetAwaiter().GetResult()` on the async version. This avoids
introducing an `async` keyword on a `public` method that was previously synchronous.

**Alternative considered:** `Task.Run` for each stream — rejected as it uses more thread pool
resources than necessary.

### ReusableTask: IDisposable with CTS rotation
**Decision:** On each `Invoke()`, before creating the new `CancellationTokenSource`, dispose
the previous one. On `Dispose()`, cancel and dispose the current one.

**Rationale:** `CancellationTokenSource` registers callbacks and allocates OS timer handles
when `CancelAfter` is used. Not disposing causes resource leaks especially in high-frequency
scenarios where `Invoke()` is called many times.

**Cancel() return value:** Changed from always `true` to returning whether cancellation was
newly requested (i.e., was not already requested). This is a pre-1.0 breaking change but
makes the return value semantically correct.

### UpdatableProperty: Lock-protected _initialized
**Decision:** Move the `_initialized` check inside `GetCore` to be protected by the
`_locker` lock using a double-checked locking pattern that awaits inside the lock scope
only after acquiring it.

**Note on async inside lock:** C# does not allow `await` inside a `lock` block. The solution
is to use a `SemaphoreSlim(1,1)` in place of `lock (_locker)` for the async path, replacing
the object lock. The task-deduplication pattern (checking `_getTask.IsCompleted`) remains
but `_initialized` is now checked and set atomically.

**Alternative:** `Lazy<Task<T>>` with `LazyThreadSafetyMode.ExecutionAndPublication` —
rejected because it doesn't support forced refresh.

### ObservableRangeCollection: Snapshot before AddRangeCore
**Decision:** In `AddRange`, enumerate `collection` into a `List<T>` snapshot immediately
(before calling `AddRangeCore`), then pass the snapshot to both `AddRangeCore` and the
notification builder.

**Rationale:** `AddRangeCore` enumerates the collection internally. If the caller then tries
to enumerate again for the notification payload, a forward-only source (LINQ, `yield return`)
is already exhausted. Snapshotting once is the correct fix.

### NetworkHelper: ConnectAsync with CancellationToken
**Decision:** Replace `BeginConnect`/`EndConnect` with `await socket.ConnectAsync(endpoint, cts.Token)`
inside an `async` method. The public API `IsPortOpened` becomes `async Task<bool>`.

**Breaking change:** `IsPortOpened` signature changes from `bool` to `Task<bool>`. This is a
pre-1.0 breaking change. All overloads (`string host`, `int portNumber`, `uint portNumber`)
are updated consistently.

**Timeout:** `CancellationTokenSource.CancelAfter(PortCheckTimeoutMs)` replaces
`AsyncWaitHandle.WaitOne(PortCheckTimeoutMs)`.

## Risks / Trade-offs
- **ProcessHelper sync API change:** Wrapping async in `.GetAwaiter().GetResult()` can cause
  deadlock on ASP.NET synchronization contexts. Mitigation: document the recommendation to use
  the async overload; the sync overload is marked `[Obsolete("Prefer WaitProcessOutputsAsync")]`.
- **IsPortOpened becomes async:** Callers must update to `await`. This is a breaking change on
  a pre-1.0 library with no declared public consumers outside tests.
- **SemaphoreSlim in UpdatableProperty:** Introduces async overhead. Acceptable because
  `Get()` is already async by design.

## Migration Plan
1. Branch off `develop` with `feature/fix-code-review-issues`
2. Apply fixes in order: ProcessHelper → ReusableTask → UpdatableProperty → ObservableRangeCollection → NetworkHelper → build/CI
3. For each fix: run `dotnet test` to confirm no regression before moving to next
4. Update CHANGELOG `[Unreleased]` section with all breaking changes
5. PR to `develop`, CI must be green
6. No rollback needed — all changes are self-contained per file

## Open Questions
- Should `IsPortOpened(string host, uint portNumber)` also become async? Yes — all overloads
  updated for consistency.
- Should `WaitProcessOutputs` (sync) be kept or removed? Kept with `[Obsolete]` for backward
  compat, to be removed at v1.0.
