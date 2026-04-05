# Project Context

## Purpose
Rinzler78.NetExtension is a .NET utility library distributed as a NuGet package.
It provides reusable extension methods and helpers for common .NET development tasks:
Observable/MVVM patterns, HTTP (with SSRF protection), JSON, strings, collections,
math, geo, footprint (CRC/SHA), tasks, REST, process, and network utilities.
Primary users are .NET application developers who want to reduce boilerplate code.

## Tech Stack
- **Language:** C# 13 (LangVersion latest), .NET 10.0 (`net10.0`)
- **Build:** MSBuild / `dotnet build`, solution file `src/Rinzler78.NetExtension.sln`
- **Test:** xUnit 2.9, FluentAssertions, Moq, coverlet (XPlat Code Coverage)
- **JSON:** Newtonsoft.Json 13.0.4 (primary) + System.Text.Json (STJ converters)
- **Other deps:** CsvHelper 33.1.0, System.IO.Hashing 10.0.5, bigrational 1.0.0.7
- **Source generator:** Roslyn `IIncrementalGenerator` (`Rinzler78.NetExtension.SourceGenerators`)
- **Quality:** pre-commit (dotnet-format, gitleaks/detect-secrets, build, test, security scan)
- **CI:** GitHub Actions — `ci.yml` (build/test/lint/compat), `release.yml` (tag → NuGet)
- **Package:** NuGet `Rinzler78.NetExtension`, MIT license

## Project Conventions

### Code Style
- All code, comments, and documentation in **English** (enforced by pre-commit hook)
- `dotnet-format` applied on every commit (pre-commit hook)
- `TreatWarningsAsErrors=true` in Release configuration
- Conventional Commits: `feat|fix|docs|test|refactor|chore|perf|ci|build|revert`
- Namespace pattern: `Rinzler78.NetExtension.<Domain>` (e.g. `Rinzler78.NetExtension.Tasks`)
- One public type per file; file name matches type name

### Architecture Patterns
- **Extension methods** as the primary API surface (static classes with `this` parameters)
- **Helpers** for non-extension utility logic (static classes without `this` on primary param)
- **Observable pattern:** `ObservableObject` base class → `SetProperty<T>` → `INotifyPropertyChanged`
- **Source generator:** `[ObservableProperty]` on private fields → compile-time property generation
- No DI container in the library itself; consumers wire dependencies externally

### Testing Strategy
- Test project: `src/Rinzler78.NetExtension.Tests/`
- Test types: unit (default), integration (`Integration/`), E2E (`E2E/`), security (`Security/`)
- Run locally: `scripts/local/test.sh` or `dotnet test src/Rinzler78.NetExtension.Tests/...`
- Coverage collected with `--collect:"XPlat Code Coverage"` → Cobertura XML
- Quality gates defined in `docs/testing-quality/quality-gates.json`
- Global thresholds: line >= 90%, branch >= 80%

### Test Infrastructure
- Test HTTP server: `WireMock.Net` (embedded, no real network) for HTTP method unit tests
- Test categories enforced via `[Trait("Category", "<type>")]`:
  - Unit (default, no trait needed)
  - Integration (real I/O, `[Trait("Category","Integration")]`)
  - E2E (external network, `[Trait("Category","E2E")]`, filtered out in CI)
  - Security (SSRF/injection tests, `[Trait("Category","Security")]`)
- Coverage targets: line >= 97%, branch >= 95% (up from 94.2%/90.9%)
- Coverage measured per capability class, enforced by CI quality gate

### Git Workflow
- Git Flow: `master` (stable) / `develop` (integration) / `feature/<name>` branches
- No direct push to `master` or `develop`
- Worktrees in `.worktrees/` for isolated feature development
- Releases triggered by annotated tags `v<MAJOR>.<MINOR>.<PATCH>`
- CHANGELOG.md updated before every release

## Domain Context
- **Observable:** MVVM-compatible property change notifications; `ObservableObject` manages
  dependencies between observable objects and propagates `PropertyChanged` events automatically.
- **SSRF:** HTTP helpers (`StringHelper.Http.cs`) validate URLs and resolved IP addresses to
  block access to localhost, RFC 1918 private ranges, APIPA, CGNAT, and IPv6 ULA/link-local.
- **FootPrint:** CRC32/CRC64 (via `System.IO.Hashing`) and SHA-512 for data integrity checks.
- **UpdatableProperty:** Lazy async property pattern - fetches and caches a value on first access,
  supports forced refresh; built on `ObservableObject` for change notifications.
- **Source generator:** `[ObservableProperty]` attribute + `ObservablePropertyGenerator`
  (incremental Roslyn generator) replaces manual property boilerplate at compile time.

## Important Constraints
- **Public API stability:** `EnablePackageValidation=true` — breaking changes must be intentional
  and documented in CHANGELOG under `### Changed` or `### Removed`.
- **No unsafe code:** `AllowUnsafeBlocks=true` is set but no unsafe code exists; this flag
  must be removed to prevent future accidental unsafe usage.
- **No direct HttpClient injection:** The static `HttpClient` in `StringHelper.Http.cs` uses a
  custom `ConnectCallback` for SSRF protection; consumers cannot replace the handler.
- **Thread safety:** `ObservableObject.SetProperty` and `ObservableRangeCollection` operations
  hold internal locks; callers must not hold external locks that could cause AB/BA deadlocks.
- **Single target framework:** Currently `net10.0` only — multi-targeting is a future decision.

## External Dependencies
- **NuGet.org / GitHub Packages:** Release workflow publishes to both; `NUGET_API_KEY` secret required for NuGet.org.
- **GitHub Actions:** CI uses `ubuntu-latest`, `windows-latest`, `macos-latest` for compat tests.
- **detect-secrets:** Secret scanning via pre-commit; baseline in `.secrets.baseline`.
