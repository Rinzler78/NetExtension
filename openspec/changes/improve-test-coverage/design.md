## Context
The test suite covers 94.2% of lines but has a significant structural gap: the entire
`StringHelper.Http.cs` surface (5 methods, ~90 lines) has 0% coverage. The root cause
is that all existing HTTP tests only exercise SSRF rejection paths — they never test
successful HTTP calls because there is no embedded test server. All other gaps are
smaller branch-coverage issues on existing well-structured tests.

## Goals / Non-Goals
- Goals:
  - Reach line >= 97%, branch >= 95%
  - Test every public method at least once in a meaningful (not just "does not throw") way
  - Make HTTP tests fully hermetic (no real network, no httpbin.org dependency)
  - Remove dead code in ObjectExtension
- Non-Goals:
  - Testing private/internal implementation details directly
  - Performance/mutation testing
  - Adding new library features
  - Replacing existing E2E tests that use real networks (kept as-is, just filtered in CI)

## Decisions

### HTTP test infrastructure: WireMock.Net
**Decision:** Use `WireMock.Net` as the embedded HTTP server for HTTP method tests.

**Rationale:**
- Zero real network dependency — server binds to localhost:0 (OS-assigned port)
- Rich stub API: fixed responses, regex URL matching, delayed responses for timeout testing
- Well-maintained, .NET 10 compatible, actively used in production test suites
- Alternative `TestServer` (ASP.NET Core) is heavier and requires a full pipeline setup

**SSRF bypass strategy:**
The `HttpClient` uses a custom `ConnectCallback` that blocks 127.x.x.x. Tests calling
`http://localhost:PORT` would be rejected. We reuse the existing internal
`OverrideHostAddressResolverForTesting(resolver)` method to make WireMock's localhost
port appear as a public IP during the test. The DNS resolver override is scoped via
`IDisposable` and restored after each test.

**WireMock fixture pattern:**
```csharp
public sealed class WireMockServerFixture : IDisposable
{
    public WireMockServer Server { get; } = WireMockServer.Start();
    public string BaseUrl => Server.Urls[0];

    // Override DNS so SSRF validator lets localhost through
    private readonly IDisposable _resolverOverride;

    public WireMockServerFixture()
    {
        _resolverOverride = StringHelper.OverrideHostAddressResolverForTesting(
            _ => new[] { IPAddress.Parse("93.184.216.34") }); // spoof as example.com
    }

    public void Dispose()
    {
        Server.Stop();
        _resolverOverride.Dispose();
    }
}
```

### Phase ordering rationale
Phase 1 (infrastructure) is a hard prerequisite for Phase 2 (HTTP tests).
Phases 3–7 are independent and can be implemented in any order or in parallel.
Phase 4 (ObjectExtension dead code removal) is a source change, not just a test change
— it should be done first within its phase to avoid misleading coverage metrics.

### JSON converter test pattern
Rather than duplicating test infrastructure per converter, we add a single
parameterized `[Theory]` per scenario type using `MemberData` that lists all 5
converters. This keeps the test count low (3 theories × 5 converters instead of 15
independent test methods) while covering all branches.

### Dead code removal: ObjectExtension.CopyTo
The `if (targetPropertyInfo.CanWrite)` check on line 56 is preceded by a
`.Where(x => x.CanWrite)` filter on line 46. The `false` branch is structurally
unreachable. Removing it eliminates 1 dead branch from coverage metrics and
makes the intent clearer. No behavioral change.

## Risks / Trade-offs
- **WireMock.Net test dependency**: adds ~2 MB to the test binary. Acceptable for a
  test-only dependency.
- **DNS resolver override is global static state**: tests using the override MUST be
  in a `[Collection]` that prevents concurrent execution if multiple fixtures are active.
  Existing `ConsoleTestCollection` and `StringHttpTestCollection` set the pattern.
- **Timeout tests are time-sensitive**: tests verifying `timeout` parameter use
  WireMock's response delay feature. A delay of 500ms with timeout of 100ms provides
  enough margin without slowing the suite significantly.

## Migration Plan
1. Phase 1 first (unblocks Phase 2)
2. Phases 2–7 can be implemented by separate workers in parallel (no shared files)
3. After each phase: `dotnet test --filter "Category!=E2E"` must pass
4. Final coverage check: run with `--collect:"XPlat Code Coverage"` and verify thresholds

## Open Questions
- None — all architectural decisions resolved above.
