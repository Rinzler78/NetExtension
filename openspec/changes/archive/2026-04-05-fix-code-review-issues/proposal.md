# Change: Fix Code Review Issues

## Why
A full code review of the Rinzler78.NetExtension library identified 23 issues spanning
correctness bugs (deadlocks, resource leaks, race conditions), design problems, CI/CD gaps,
and missing documentation. Three of these issues are critical (potential deadlock in
ProcessHelper, CancellationTokenSource leak in ReusableTask, and thread-safety race in
UpdatableProperty) and must be fixed before any new features are added.

## What Changes

### Critical Bug Fixes
- **ProcessHelper** — parallel stdout/stderr reading to fix OS pipe deadlock (**BREAKING**: sync overload now reads streams concurrently; behavior change)
- **ReusableTask** — add `IDisposable` implementation; dispose `CancellationTokenSource` on each `Invoke()` and on `Dispose()`; fix `Cancel()` return value (returns `false` when no task was started)
- **UpdatableProperty** — move `_initialized` read/write inside `_locker` to eliminate concurrent first-access race condition

### High Priority Bug Fixes
- **ObservableRangeCollection** — fix double-enumeration in `AddRange`: capture items in a `List<T>` before calling `AddRangeCore` to support forward-only enumerables
- **NetworkHelper** — replace deprecated APM `BeginConnect`/`EndConnect` with `ConnectAsync` + `CancellationToken` for port checking

### Build / Project Configuration
- **AllowUnsafeBlocks** — remove `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>` from both Debug and Release configurations (no unsafe code exists)
- **TestResults in .gitignore** — add `**/TestResults/` to `.gitignore`; existing committed TestResults files removed from tracking
- **PackagePath backslash** — replace `PackagePath="\"` with `PackagePath=""` in the README None item

### CI/CD Fixes
- **Consistent .NET version** — update `lint` and `benchmark` jobs from `8.0.x` to `10.0.x`
- **Pre-commit tool restore** — replace `dotnet new tool-manifest --force && dotnet tool install` with `dotnet tool restore` in the `dotnet-tools-setup` hook
- **Quality gate enforcement** — add a CI step that reads `docs/testing-quality/quality-gates.json` and fails if Cobertura coverage is below the defined thresholds
- **Test Traits** — add `[Trait("Category", "Integration")]`, `[Trait("Category", "E2E")]`, and `[Trait("Category", "Security")]` to all tests in the respective directories
- **Release sed portability** — replace `sed -i` with `sed -i ''` guarded by OS detection, or use `xmlstarlet`

## Impact
- Affected specs: process-helper, reusable-task, updatable-property, observable-range-collection, network-helper, project-build, ci-cd
- Affected code:
  - `src/Rinzler78.NetExtension.Standard/Process/ProcessHelper.cs`
  - `src/Rinzler78.NetExtension.Standard/Tasks/ReusableTask.cs`
  - `src/Rinzler78.NetExtension.Standard/Objects/UpdatableProperty.cs`
  - `src/Rinzler78.NetExtension.Standard/Observable/ObservableRangeCollection.cs`
  - `src/Rinzler78.NetExtension.Standard/Network/NetworkHelper.cs`
  - `src/Rinzler78.NetExtension.Standard/Rinzler78.NetExtension.Standard.csproj`
  - `src/Rinzler78.NetExtension.Tests/` (trait annotations across Integration/, E2E/, Security/)
  - `.gitignore`
  - `.pre-commit-config.yaml`
  - `.github/workflows/ci.yml`
  - `.github/workflows/release.yml`
- Breaking changes:
  - `ReusableTask.Cancel()` now returns `false` instead of `true` when no task is running (**BREAKING** — pre-1.0, no prior public consumers)
  - `ReusableTask` now implements `IDisposable` — consumers must call `Dispose()` (**BREAKING** — pre-1.0)
