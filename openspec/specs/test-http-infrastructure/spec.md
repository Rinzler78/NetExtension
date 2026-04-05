# Capability: Test HTTP Infrastructure

## Purpose
Provides an embedded HTTP test server (WireMock.Net) used by unit and integration tests
to exercise HTTP methods without making real network calls. Enables deterministic,
isolated testing of `StringHelper.Http.cs` methods.

## Requirements

### Requirement: Embedded HTTP Server
The test project SHALL include `WireMock.Net` as a test dependency.
Tests using HTTP methods SHALL use `WireMockServer.Start()` to bind to a random
available port on localhost, and stop the server in `Dispose()`.

#### Scenario: Server starts and serves configured responses
- **WHEN** `WireMockServer.Start()` is called and a stub is configured
- **THEN** HTTP requests to the server's URL return the configured response

#### Scenario: Server stops cleanly
- **WHEN** the test class is disposed
- **THEN** `WireMockServer.Stop()` is called and the port is released

### Requirement: SSRF Validator Override for Local Testing
Tests that need to call `http://localhost:PORT` (WireMock) SHALL use
`StringHelper.OverrideHostAddressResolverForTesting` to make the local
address appear as a public IP, bypassing the SSRF validator for the
duration of the test.

#### Scenario: Override routes localhost through validation
- **WHEN** `OverrideHostAddressResolverForTesting` maps `127.0.0.1` to a public IP
- **THEN** `ValidateUrl` does not throw for `http://localhost:PORT`
- **AND** the override is restored when the returned `IDisposable` is disposed
