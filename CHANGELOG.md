# Changelog

All notable changes to **Rinzler78.NetExtension** are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).
Versioning follows [SemVer](https://semver.org/) — `0.MINOR.PATCH` until 1.0.

---

## [Unreleased]

---

## [0.2.0] — 2026-04-05

### Added
- **`ReusableTask` implements `IDisposable`** — disposes the internal `CancellationTokenSource`
  on `Dispose()`; each `Invoke()` cycle now rotates and disposes the old CTS to prevent resource
  leaks. `Invoke()`, `InvokeSync()`, and `Cancel()` throw `ObjectDisposedException` after disposal.
  (**BREAKING** — pre-1.0)
- **`ProcessHelper.WaitProcessOutputsAsync`** — reads stdout and stderr concurrently via
  `Task.WhenAll` before `WaitForExitAsync`, eliminating the OS pipe deadlock on large output.
- **`UpdatableProperty` uses `SemaphoreSlim(1,1)`** — replaces `object _locker` to allow
  `await` inside the critical section; `_initialized` is now read and written atomically,
  preventing concurrent first-access from triggering multiple `InnerUpdate` calls.
- **`ArrayExtension` uses safe `Buffer.BlockCopy`** — replaced `unsafe` byte-copy blocks,
  enabling removal of `AllowUnsafeBlocks` from the project.
- **Quality gate enforcement in CI** — new step in `build-and-test` (Release) parses Cobertura
  XML and fails if `line-rate < 0.90` or `branch-rate < 0.80` per `quality-gates.json`.
- **Test category traits** — `[Trait("Category", "Security")]` added to all Security test
  classes; CI test step now filters `Category!=E2E` to skip network-dependent tests.

### Fixed
- **`ProcessHelper.WaitProcessOutputs` (sync)** — susceptible to OS pipe buffer deadlock when
  the process produced large output on both streams simultaneously. Now delegates to
  `WaitProcessOutputsAsync`. Marked `[Obsolete]`.
- **`ReusableTask.Cancel()` return value** — previously always returned `true`; now returns
  `false` when no task has been started or when already cancelled. (**BREAKING** — pre-1.0)
- **`UpdatableProperty._initialized` race** — was read outside the lock, allowing concurrent
  first-access to call `InnerUpdate` multiple times. Fixed with `SemaphoreSlim`.
- **`ObservableRangeCollection.AddRange` double-enumeration** — the `Add` path now snapshots
  the source into a `List<T>` before calling `AddRangeCore`, preventing silent data loss with
  forward-only `IEnumerable<T>` sources (LINQ queries, `yield return`).
- **`NetworkHelper.IsPortOpened`** — replaced deprecated APM `BeginConnect`/`EndConnect` with
  `ConnectAsync` + `CancellationTokenSource.CancelAfter(1000ms)`. Return type changed from
  `bool` to `Task<bool>` across all overloads. (**BREAKING** — pre-1.0)

### Changed
- **`AllowUnsafeBlocks` removed** — removed from both Debug and Release PropertyGroup.
- **`PackagePath` fixed** — replaced backslash `PackagePath` with empty string for cross-platform NuGet packing.
- **CI .NET version unified** — `lint` and `benchmark` jobs updated from `8.0.x` to `10.0.x`.
- **Pre-commit `dotnet-tools-setup`** — replaced reinstall with `dotnet tool restore`; removed `always_run: true`.
- **`release.yml` sed** — uses `|` delimiter and GNU-compatible `sed -i` with comment.

### Removed
- **`TestResults/` untracked** — `**/TestResults/` added to `.gitignore`; previously committed
  `.trx` and `coverage.cobertura.xml` files removed from version control.

---


## [0.1.0] — 2026-04-03

### Added
- **`[ObservableProperty]` Roslyn source generator** — marks private backing fields in `partial`
  `ObservableObject` subclasses; the generator emits `public` properties with `SetProperty`
  wiring at compile time (zero runtime reflection, zero boilerplate).
- **`TaskHelper.WhenAll<T>`** — generic overload for `IEnumerable<Task<T>>` returning `T[]`.
- **`SECURITY.md`** — security policy, supported versions, responsible disclosure process.
- **`.github/PULL_REQUEST_TEMPLATE.md`** — standardised PR checklist.
- **`.github/ISSUE_TEMPLATE/`** — structured YAML templates for bug reports and feature requests.
- **`ObservableObject`** — `AttachDependenciesCore` / `DetachDependenciesCore` helpers
  eliminate the AB/BA deadlock risk from the previous double-lock pattern.
- **`ensure_hooks()`** in `scripts/lib/common.sh` — auto-installs pre-commit hooks the
  first time any local script runs; worktree-aware and silent if pre-commit is absent.

### Fixed
- **`InterfaceConverter.Write`** — was empty; now serialises via `JsonSerializer.Serialize`.
- **`CollectionConverter.Write`** — result was discarded; now writes to the `Utf8JsonWriter`.
- **`ULongArrayConverter.WriteJson`** — serialised type name instead of data; now emits a
  JSON array of strings via `InvariantCulture`.
- **`NetworkHelper.Ping`** — `Ping` instance was not disposed (socket/handle leak); fixed
  with `using var`.
- **`NetworkHelper.IsPortOpened(string, uint)`** — port 65535 was silently excluded
  (`< ushort.MaxValue` → `<= ushort.MaxValue`).
- **`NetworkHelper.IsPortOpened(IPAddress, int)`** — negative port numbers were cast to
  `uint` silently; now throws `ArgumentOutOfRangeException`.
- **`BaseRestApi`** — `BaseUri` promoted from public field to property; URI construction
  replaced fragile string concatenation with `new Uri(baseUri, path.TrimStart('/'))`.
- **`UpdatableProperty.Get`** — `Property == default` incorrectly treated value-type
  defaults as uninitialised; replaced with a dedicated `_initialized` flag.
- **`StringHelper.GetBytes`** — documented that characters > U+007F are silently replaced
  by `0x3F`; added `Encoding.UTF8` guidance in XML doc.

### Changed
- **API renames** (breaking — pre-1.0, no prior public consumers):
  - `JsonHelper.DeSerialize<T>` → `Deserialize<T>` (all overloads)
  - `JsonHelper.DeSerializeObject` → `DeserializeObject`
  - `JsonHelper.DeSerializeObjectFromFile` → `DeserializeObjectFromFile` (all overloads)
  - `StringHelper.ComputeLevenshteInDistance` → `ComputeLevenshteinDistance` (typo fix)
  - `StringHelper.ToJsonFormatedString` → `ToJsonFormattedString` (typo fix)
- **`ValidateUrl` SSRF hardening** — extends blocking to IPv6 ULA (`fc00::/7`),
  link-local (`fe80::/10`), APIPA `169.254.0.0/16`, wildcard `0.0.0.0`,
  and CGNAT `100.64.0.0/10`.
- **`ValidateFilePath`** — replaced fragile blocklist with an extension allowlist
  (`.json .xml .csv .txt .yaml .yml .toml .ini .conf`).
- **`DecimalHelper.Pow`** — uses exact decimal repeated-squaring for integer exponents;
  falls back to `double`-approximation for fractional exponents (documented in XML).
- **`DecimalHelper.Log`** — explicit `OverflowException` guard for zero/negative values.
- **`ObservablePropertyAttribute`** — uncommented and fully implemented with
  `[AttributeUsage(AttributeTargets.Field)]` and optional explicit property name override.
- **`ReusableTask`** — removed 25-line commented-out dead-code block.
- **CI** — `pre-commit install` (no-op in CI) replaced by `pre-commit run --all-files`.
- **PackageVersion** set to `0.1.0` for the public release.

### Security
- HTTP helpers now block IPv6 ULA, link-local, APIPA, `0.0.0.0`, and CGNAT ranges
  in addition to the existing RFC 1918 / loopback protection.
- File-path helpers switched from blocklist to allowlist for extension validation.

---

[Unreleased]: https://github.com/Rinzler78/NetExtension/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/Rinzler78/NetExtension/releases/tag/v0.1.0
