# Test Quality Gates

The repository exposes a repeatable quality workflow for local runs, Docker runs, and CI.

## Local validation

```bash
./scripts/local/quality.sh
```

This command:
- builds the solution unless `--no-build` is passed
- runs `unit`, `integration`, and `e2e` suites independently
- runs the full suite with coverage enabled
- writes file-level coverage outputs under `artifacts/test-results/quality-local/`
- fails if the coverage gate defined in `docs/testing-quality/quality-gates.json` is not met

## Docker validation

```bash
./scripts/docker/quality.sh
```

This command mirrors the local gate while executing the test suites through the Docker test image.

## Coverage artifacts

The quality workflow emits:
- `coverage-summary.json` and `coverage-summary.md` for the current file-level baseline
- `coverage-gate.json` and `coverage-gate.md` for gate status and failures

The checked-in baseline lives in `docs/testing-quality/coverage-baseline.json` and `docs/testing-quality/coverage-baseline.md`.

## Current policy

- Global line coverage must stay at or above 90%.
- Every production source file must stay at or above 90% line coverage unless it is listed as an approved exclusion in `quality-gates.json`.
- `Strings/StringHelper.Http.cs` is an approved exclusion because its HTTP execution paths depend on live network interaction and are intentionally isolated from deterministic local and CI quality runs.
- Any non-excluded file-level regression below the checked-in baseline fails the gate.
