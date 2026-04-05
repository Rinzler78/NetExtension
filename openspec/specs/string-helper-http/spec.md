# Capability: String Helper HTTP

## Purpose
Extension methods on `string` for making HTTP requests: GET (stream, string, typed),
POST (raw string, typed), with built-in SSRF protection via URL and DNS validation.
All methods use a shared static `HttpClient` with a custom `ConnectCallback` that
validates resolved IP addresses before connecting.

## Requirements

### Requirement: HTTP GET Stream
`HttpGetStreamAsync(url, timeout?, ct)` SHALL perform a GET request and return
the response body as a `Stream`. It SHALL call `ValidateUrl` before sending.

#### Scenario: Successful GET returns stream
- **WHEN** called with a valid public URL serving a known response
- **THEN** the returned stream contains the response body

#### Scenario: SSRF-blocked URL throws ArgumentException
- **WHEN** called with a localhost or private-range URL
- **THEN** `ArgumentException` is thrown before any network call

#### Scenario: Timeout parameter triggers cancellation
- **WHEN** called with a timeout shorter than the server response time
- **THEN** an `OperationCanceledException` or `TaskCanceledException` is thrown

#### Scenario: CancellationToken already cancelled throws
- **WHEN** called with an already-cancelled `CancellationToken`
- **THEN** `OperationCanceledException` is thrown immediately

### Requirement: HTTP GET String
`HttpGetStringAsync(url, timeout?, ct)` SHALL perform a GET request and return the
response body as a `string`. Shares all SSRF validation from `HttpGetStreamAsync`.

#### Scenario: Successful GET returns body string
- **WHEN** called with a valid URL serving plain text
- **THEN** the returned string matches the served content

#### Scenario: DNS rebinding blocked at ConnectAsync
- **WHEN** a hostname resolves to a private IP address at connect time
  (after passing URL validation)
- **THEN** `ArgumentException` is thrown by the `ConnectCallback`

### Requirement: HTTP GET Typed Deserialization
`HttpGetAsync<T>(url, settings?, timeout?, ct)` SHALL deserialize the JSON response
body into `T` using `JsonSerializer`. SHALL throw `InvalidOperationException` when
the deserialized result is null.

#### Scenario: Successful typed GET returns deserialized object
- **WHEN** called with a URL serving valid JSON for type `T`
- **THEN** a non-null `T` instance with correct values is returned

#### Scenario: Non-deserializable response throws InvalidOperationException
- **WHEN** the response body cannot be deserialized to `T` (returns null from deserializer)
- **THEN** `InvalidOperationException` is thrown

### Requirement: HTTP POST String
`HttpPostString<TReq>(url, obj?, timeout?, ct)` SHALL serialize `obj` to JSON,
POST it, and return the response body as a `string`.
SHALL call `ValidateUrl` before sending.

#### Scenario: Successful POST returns response body
- **WHEN** called with a valid URL and a serializable object
- **THEN** the response body string is returned

#### Scenario: SSRF-blocked POST URL throws ArgumentException
- **WHEN** called with a localhost or private URL
- **THEN** `ArgumentException` is thrown before any network call

#### Scenario: Null payload sends empty body
- **WHEN** called with `obj = null`
- **THEN** the POST body is null/empty and the server response is returned normally

### Requirement: HTTP POST Typed Response
`HttpPost<TReq, TImpl, TReturn>(url, obj, timeout?, ct)` SHALL POST the request
and deserialize the response body into `TImpl`, returning it as `TReturn`.
SHALL throw `InvalidOperationException` when the cast fails.

#### Scenario: Successful POST returns typed response
- **WHEN** the server returns JSON deserializable as `TImpl`
- **THEN** the result cast as `TReturn` is returned

#### Scenario: Non-deserializable response throws InvalidOperationException
- **WHEN** the response cannot be deserialized as `TImpl`
- **THEN** `InvalidOperationException` is thrown
