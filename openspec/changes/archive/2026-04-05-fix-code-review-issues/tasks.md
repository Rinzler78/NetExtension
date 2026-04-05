# Tasks: fix-code-review-issues

## 1. Critical Bug Fixes

### 1.1 ProcessHelper — Fix pipe deadlock
- [x] 1.1.1 Convert `WaitProcessOutputsAsync` to read stdout and stderr concurrently with `Task.WhenAll`
- [x] 1.1.2 Mark synchronous `WaitProcessOutputs` with `[Obsolete("Prefer WaitProcessOutputsAsync to avoid pipe deadlock")]`
- [x] 1.1.3 Implement sync overload using `.GetAwaiter().GetResult()` on the async version
- [x] 1.1.4 Add unit tests verifying that both streams are read without deadlock (use process producing large output on both streams)
- [x] 1.1.5 Add regression test: forward both streams >64KB → must not block

### 1.2 ReusableTask — Fix CancellationTokenSource leak and IDisposable
- [x] 1.2.1 Add `IDisposable` to `ReusableTask` declaration
- [x] 1.2.2 In `Invoke()`: capture old CTS, create new CTS, then dispose old CTS before returning
- [x] 1.2.3 In `Dispose()`: call `Cancel()` then `_cancellationTokenSource.Dispose()`; set disposed flag
- [x] 1.2.4 Fix `Cancel()` to return `false` if already cancelled or no task was ever started
- [x] 1.2.5 Add `ObjectDisposedException` guard in `Invoke()`, `InvokeSync()`, and `Cancel()` after disposal
- [x] 1.2.6 Add tests: Dispose disposes CTS; Invoke after Dispose throws; Cancel returns correct bool

### 1.3 UpdatableProperty — Fix _initialized race condition
- [x] 1.3.1 Replace `object _locker` + `lock` with `SemaphoreSlim(1, 1)` for async-safe mutual exclusion
- [x] 1.3.2 Rewrite `GetCore`: acquire semaphore, double-check `_initialized`, release semaphore
- [x] 1.3.3 Rewrite `UpdateCore`: acquire semaphore, run `InnerUpdate`, set `_initialized`, release semaphore
- [x] 1.3.4 Ensure `Get()` task-deduplication still works after semaphore introduction
- [x] 1.3.5 Add concurrent test: 10 threads call `Get()` simultaneously → `InnerUpdate` called exactly once

## 2. High Priority Bug Fixes

### 2.1 ObservableRangeCollection — Fix double-enumeration in AddRange
- [x] 2.1.1 In `AddRange(Add mode)`: enumerate `collection` into `List<T> snapshot` before calling `AddRangeCore(snapshot)`
- [x] 2.1.2 Use `snapshot` as the notification payload (remove the second enumeration on line 60)
- [x] 2.1.3 Add test: call `AddRange` with a `yield return` LINQ sequence → notification contains all items

### 2.2 NetworkHelper — Replace deprecated APM with ConnectAsync
- [x] 2.2.1 Change `IsPortOpened(this IPAddress, uint portNumber)` return type to `Task<bool>`
- [x] 2.2.2 Implement with `await socket.ConnectAsync(endpoint, cts.Token)` and `CancellationTokenSource.CancelAfter`
- [x] 2.2.3 Update all `IsPortOpened` overloads (string+uint, IPAddress+int) to return `Task<bool>`
- [x] 2.2.4 Update `IsPortOpened(string host, uint portNumber)` to `await` the core overload
- [x] 2.2.5 Update all call sites in tests to use `await`
- [x] 2.2.6 Add test: verify timeout behavior (non-routable address → returns false within 1.1s)

## 3. Build / Project Configuration

### 3.1 Remove AllowUnsafeBlocks
- [x] 3.1.1 Remove both `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>` entries from `Rinzler78.NetExtension.Standard.csproj`
- [x] 3.1.2 Run `dotnet build --configuration Release` to confirm build succeeds

### 3.2 TestResults in .gitignore
- [x] 3.2.1 Add `**/TestResults/` to `.gitignore`
- [x] 3.2.2 Run `git rm -r --cached src/Rinzler78.NetExtension.Tests/TestResults/` to untrack committed results
- [x] 3.2.3 Confirm `git status` shows TestResults as untracked/ignored

### 3.3 Fix PackagePath backslash
- [x] 3.3.1 Replace `PackagePath="\"` with `PackagePath=""` in `Rinzler78.NetExtension.Standard.csproj`
- [x] 3.3.2 Run `dotnet pack --configuration Release` and verify README.md is at package root

## 4. CI/CD Fixes

### 4.1 Consistent .NET version in lint and benchmark jobs
- [x] 4.1.1 Update `lint` job `dotnet-version` from `8.0.x` to `10.0.x` in `ci.yml`
- [x] 4.1.2 Update `benchmark` job `dotnet-version` from `8.0.x` to `10.0.x` in `ci.yml`

### 4.2 Pre-commit tool restore instead of reinstall
- [x] 4.2.1 Replace the `dotnet-tools-setup` hook body with: `dotnet tool restore --tool-manifest .tools/.config/dotnet-tools.json`
- [x] 4.2.2 Remove `always_run: true` (the hook runs only when `.cs` files change)
- [x] 4.2.3 Test: run `pre-commit run dotnet-format` twice → second run completes faster (no reinstall)

### 4.3 Quality gate enforcement in CI
- [x] 4.3.1 Add a shell step in `build-and-test` (Release config) that reads `docs/testing-quality/quality-gates.json`
- [x] 4.3.2 Parse Cobertura XML (`coverage/**/coverage.cobertura.xml`) for line-rate and branch-rate
- [x] 4.3.3 Fail the step (`exit 1`) if either rate is below the gate thresholds
- [x] 4.3.4 Document the gate format in `docs/testing-quality/README.md`

### 4.4 Test Trait annotations
- [x] 4.4.1 Add `[Trait("Category", "Integration")]` to all test classes under `Integration/`
- [x] 4.4.2 Add `[Trait("Category", "E2E")]` to all test classes under `E2E/`
- [x] 4.4.3 Add `[Trait("Category", "Security")]` to all test classes under `Security/`
- [x] 4.4.4 Update CI `Test` step (Debug) to run `--filter "Category!=E2E"` (E2E requires network)
- [x] 4.4.5 Verify `dotnet test --filter "Category=Integration"` runs only integration tests

### 4.5 Release sed portability
- [x] 4.5.1 Replace `sed -i '...'` in `release.yml` with GNU-compatible: `sed -i "s/..." file`
  (already compatible on ubuntu-latest; add explicit test comment)
- [x] 4.5.2 Alternative: use `xmlstarlet ed -u "//PackageVersion" -v "$VERSION"` if xmlstarlet is available

## 5. Documentation & Changelog

- [x] 5.1 Update `CHANGELOG.md` `[Unreleased]` section with all breaking changes and fixes
- [x] 5.2 Version bump to `0.2.0` in `CHANGELOG.md` (new section header `## [0.2.0]`)
- [x] 5.3 Verify all modified public APIs have complete XML `<summary>` and `<exception>` tags
