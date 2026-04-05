# Capability: CI/CD Pipeline

## Purpose
GitHub Actions workflows for continuous integration (`ci.yml`) and release (`release.yml`).
Enforces build, test, coverage, lint, compatibility, and security checks on every push.

## Requirements

### Requirement: Consistent .NET Version Across Jobs
All CI jobs (`build-and-test`, `lint`, `benchmark`, `compatibility-test`) SHALL use the
same major .NET version (currently `10.0.x`).

#### Scenario: Lint uses same .NET as build
- **WHEN** the CI `lint` job runs
- **THEN** it uses `dotnet-version: '10.0.x'`

### Requirement: Pre-commit Tool Restore Not Reinstall
The `dotnet-tools-setup` pre-commit hook SHALL use `dotnet tool restore` to reuse
already-installed tools and SHALL NOT have `always_run: true`.

#### Scenario: Hook is idempotent
- **WHEN** the pre-commit hook runs with tools already installed
- **THEN** no re-installation occurs

### Requirement: Quality Gates Enforced in CI
The CI pipeline SHALL fail if coverage falls below the thresholds defined in
`docs/testing-quality/quality-gates.json` (line >= 90%, branch >= 80%).

#### Scenario: Coverage below threshold fails CI
- **WHEN** the coverage report shows line coverage below 90%
- **THEN** the CI pipeline step exits with a non-zero code

### Requirement: Test Trait Filtering
Tests in `Integration/`, `E2E/`, and `Security/` directories SHALL be decorated with
`[Trait("Category", "<type>")]` where `<type>` matches the directory name.

#### Scenario: Unit-only filter works
- **WHEN** tests are run with `--filter "Category!=E2E"`
- **THEN** no E2E tests execute

### Requirement: Release Versioning Is Linux-Compatible
The `release.yml` version-substitution step SHALL use GNU-sed-compatible syntax.

#### Scenario: Version updated on ubuntu-latest
- **WHEN** a `v*` tag is pushed
- **THEN** `<PackageVersion>` in the csproj is correctly updated
