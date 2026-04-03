# Changelog

All notable changes to **Rinzler78.NetExtension** are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).
Versioning follows [SemVer](https://semver.org/) — `0.MINOR.PATCH` until 1.0.

---

## [Unreleased]

---

## [0.2.0] — 2026-04-03

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
- **`scripts/local/setup.sh`** — one-shot developer environment initialisation
  (submodules, pre-commit hooks, NuGet restore).
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
- **PackageVersion** bumped to `0.2.0`.

### Security
- HTTP helpers now block IPv6 ULA, link-local, APIPA, `0.0.0.0`, and CGNAT ranges
  in addition to the existing RFC 1918 / loopback protection.
- File-path helpers switched from blocklist to allowlist for extension validation.

---

## [0.1.2] — 2025-07-15

### Changed
- Smart commit-based CI execution to avoid redundant runs.
- CI quality gates: enforce ≥ 90% coverage (`quality-gates.json`, `coverage-baseline.json`).
- Refactored developer workflow scripts (`scripts/local/`, `scripts/docker/`).
- Added explicit integration and e2e test suites; aligned Docker and local test suites.

### Added
- `docs/testing-quality/` — quality gate documentation and baselines.
- Bash/Perl wrappers for coverage reporting, comment language checks, changelog extraction.
- Docker support (`docker/netextension/Dockerfile`, `scripts/docker/`).

---

## [0.1.1] — 2025-07-10

### Changed
- CI validation test reset (clean start).

---

## [0.1.0] — 2024-02-01

### Added
- Initial release.
- Core utility extensions: collections, JSON, strings, observables, process, REST,
  tasks, dates, geo, math, enums, network, types, footprint.
- xUnit test project with coverage via Coverlet.
- GitHub Actions CI pipeline.

---

[Unreleased]: https://github.com/Rinzler78/NetExtension/compare/v0.2.0...HEAD
[0.2.0]: https://github.com/Rinzler78/NetExtension/compare/v0.1.2...v0.2.0
[0.1.2]: https://github.com/Rinzler78/NetExtension/compare/v0.1.1...v0.1.2
[0.1.1]: https://github.com/Rinzler78/NetExtension/compare/v0.1.0...v0.1.1
[0.1.0]: https://github.com/Rinzler78/NetExtension/releases/tag/v0.1.0
