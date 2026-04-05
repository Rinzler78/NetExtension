## ADDED Requirements

### Requirement: Consistent .NET Version Across Jobs
All CI jobs (`build-and-test`, `lint`, `benchmark`, `compatibility-test`) SHALL use
`dotnet-version: '10.0.x'` to ensure analyzer and format behavior is identical
across all pipeline stages.

#### Scenario: Lint uses 10.0.x
- **WHEN** the CI `lint` job runs
- **THEN** the step `Setup .NET` uses `dotnet-version: '10.0.x'`

#### Scenario: Benchmark uses 10.0.x
- **WHEN** the CI `benchmark` job runs
- **THEN** the step `Setup .NET` uses `dotnet-version: '10.0.x'`

## ADDED Requirements

### Requirement: Pre-commit Tool Restore Not Reinstall
The `dotnet-tools-setup` pre-commit hook SHALL use `dotnet tool restore` to reuse
already-installed tools and SHALL NOT force-recreate the tool manifest on every run.
The hook SHALL NOT have `always_run: true`.

#### Scenario: Hook is idempotent
- **WHEN** the `dotnet-tools-setup` hook runs with tools already installed
- **THEN** the command exits 0 without reinstalling any tool

## ADDED Requirements

### Requirement: Quality Gates Enforced in CI
The CI pipeline SHALL fail if coverage falls below the thresholds in
`docs/testing-quality/quality-gates.json` (line-rate >= 0.90, branch-rate >= 0.80).
A dedicated step SHALL parse the Cobertura XML produced by `dotnet test` and compare
the aggregate rates against the gate file.

#### Scenario: Coverage below line threshold
- **WHEN** aggregate line-rate in Cobertura XML is below 0.90
- **THEN** the quality-gate step exits with code 1 and the CI job fails

#### Scenario: Coverage meets threshold
- **WHEN** aggregate line-rate >= 0.90 and branch-rate >= 0.80
- **THEN** the quality-gate step exits with code 0

## ADDED Requirements

### Requirement: Test Trait Categorization
Test classes in `Integration/`, `E2E/`, and `Security/` directories SHALL declare
a `[Trait("Category", "<type>")]` attribute where `<type>` matches the directory name
(`Integration`, `E2E`, `Security`).
The CI test step SHALL use `--filter "Category!=E2E"` to exclude E2E tests that
require external network access.

#### Scenario: Filter excludes E2E
- **WHEN** tests are run with `--filter "Category!=E2E"`
- **THEN** no tests from `E2E/` execute

#### Scenario: Filter isolates Integration
- **WHEN** tests are run with `--filter "Category=Integration"`
- **THEN** only tests decorated with `[Trait("Category", "Integration")]` execute

## ADDED Requirements

### Requirement: Release Versioning Is Linux-Compatible
The `release.yml` version-substitution step SHALL use a `sed` command compatible with
GNU sed (ubuntu-latest), avoiding macOS-specific `sed -i ''` syntax.

#### Scenario: Version updated on ubuntu-latest
- **WHEN** a `v*` tag is pushed and the release job runs on ubuntu-latest
- **THEN** `<PackageVersion>` in the csproj is correctly replaced with the tag version
  and the subsequent `dotnet build` succeeds
