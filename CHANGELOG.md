# Changelog

All notable changes to **Rinzler78.NetExtension** are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).
Versioning follows [SemVer](https://semver.org/).

---

## [Unreleased]

---

## [1.0.1] — 2026-04-08

### Fixed
- **REST URI construction**: `BaseRestApi` used `new Uri(base, segment)` which, per RFC 3986,
  replaced the last path segment instead of appending when the base had no trailing slash.
  Chained hierarchies (e.g. `cosmos/bank/v1beta1/balances`) now preserve all segments.

### Added
- **131 edge-case unit tests** across all 20+ library modules (878 total, up from 737).
  Covers: null/empty inputs, boundary values, overflow, encoding, slash patterns,
  SSRF IP ranges (IPv4-mapped IPv6, CGNAT, link-local), culture-dependent formatting,
  known limitations, and error paths.

---

## [1.0.0] — 2026-04-07

Initial public release.

### Highlights
- **30+ utility modules** — Observable MVVM helpers, JSON converters (Newtonsoft + System.Text.Json),
  SSRF-protected HTTP, network tools, string operations, async patterns, CSV, hash functions,
  distance conversions, process helpers, REST API base classes, and more.
- **`[ObservableProperty]` Roslyn source generator** — zero-boilerplate MVVM properties
  with compile-time `SetProperty` wiring.
- **SSRF protection** — blocks RFC 1918, loopback, link-local, CGNAT, IPv6 ULA,
  IPv4-mapped IPv6 (`::ffff:`), and `0.0.0.0` at both URL validation and DNS resolution time.
- **737 tests** across 3 platforms (Ubuntu, macOS, Windows) with 97%+ line coverage
  and 95%+ branch coverage.
- **Full XML documentation** on all 51 public classes and their public methods.
- **CI/CD pipeline** — lint, build, test, coverage gates, security scan, NuGet pack,
  multi-platform compatibility tests.

### Modules
| Module | Description |
|--------|-------------|
| Array | Byte array helpers (Buffer.BlockCopy, CRC-ready) |
| Collections | Observable batch operations, collection sync |
| Command | ICommand.TryExecute extension |
| Console | Thread-safe colored console output |
| CSV | CSV load/parse via CsvHelper |
| Dates | TimeSlot, date utilities |
| Enums | Parse, Convert with caching (EnumParser, GlobalEnumParser) |
| FootPrint | CRC32, CRC64, SHA512 hash helpers |
| Geo | Distance unit conversions |
| Json | JsonHelper + 12 converters (Newtonsoft + System.Text.Json) |
| Linq | TryAggregate extension |
| Math | BigInteger, Decimal helpers (Pow, Log) |
| Measure | Duration measurement (sync + async) |
| Network | IsPortOpened, Resolve, Ping |
| Objects | CopyTo, UpdatableProperty (async lazy) |
| Observable | ObservableObject, ObservableRangeCollection, [ObservableProperty] generator |
| Process | WaitProcessOutputsAsync (deadlock-free) |
| REST | BaseRestApi, URL path builder |
| Strings | Levenshtein, similarity, case conversion, email validation, HTTP (SSRF-protected) |
| Tasks | TaskHelper.WhenAll, ReusableTask (single-flight async) |
| Types | ActivatorHelper, ConvertHelper, TypeExtensions |
| Uri | ToFullEndpoint |

### Security
- SSRF protection: blocks private IP ranges, loopback, link-local, CGNAT, IPv6 ULA,
  IPv4-mapped IPv6, and wildcard addresses.
- File path validation: extension allowlist (`.json .xml .csv .txt .yaml .yml .toml .ini .conf`).
- DNS rebinding mitigation via `SocketsHttpHandler.ConnectCallback`.
- Pre-commit secret scanning with detect-secrets.

---

[Unreleased]: https://github.com/Rinzler78/NetExtension/compare/v1.0.1...HEAD
[1.0.1]: https://github.com/Rinzler78/NetExtension/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/Rinzler78/NetExtension/releases/tag/v1.0.0
