# Capability: REST API Base

## Purpose
`BaseRestApi` provides a base class for REST API clients with URI construction.
`BaseRestApiExt` provides `CreateUrlPath` to build query strings from a dictionary.

## Requirements

### Requirement: BaseUri Construction With Path
`BaseRestApi(Uri baseUri, string? path)` SHALL combine `baseUri` with `path` using
`new Uri(baseUri, path.TrimStart('/'))`. Leading slashes in `path` SHALL be stripped
so that `/v1/resource` and `v1/resource` produce the same result.

#### Scenario: Path without leading slash
- **WHEN** constructed with `path = "v1/resource"`
- **THEN** `BaseUri` equals `baseUri + "v1/resource"`

#### Scenario: Path with leading slash is normalized
- **WHEN** constructed with `path = "/v1/resource"`
- **THEN** `BaseUri` equals the same URI as `path = "v1/resource"`

#### Scenario: Null path returns base URI unchanged
- **WHEN** constructed with `path = null`
- **THEN** `BaseUri` equals `baseUri` exactly

### Requirement: CreateUrlPath Builds Query String
`CreateUrlPath(baseUrl, args)` SHALL append query parameters from `args`,
URL-encoding both keys and values. Null values SHALL be excluded.
`bool` values SHALL serialize as `"true"` or `"false"`.
`IFormattable` values SHALL use `InvariantCulture`.

#### Scenario: Bool true serializes as "true"
- **WHEN** args contains `["active"] = true`
- **THEN** the query string contains `active=true`

#### Scenario: Null values excluded from query string
- **WHEN** all args values are null
- **THEN** no `?` is appended to the URL

#### Scenario: Special characters in key are URL-encoded
- **WHEN** args contains `["my key"] = 1`
- **THEN** the key is encoded as `my+key` or `my%20key`
