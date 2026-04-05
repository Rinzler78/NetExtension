## ADDED Requirements

### Requirement: BaseUri Construction With Path
`BaseRestApi(Uri baseUri, string? path)` SHALL normalize `path` by stripping
a leading slash via `TrimStart('/')`, so `/v1/resource` and `v1/resource`
produce the same `BaseUri`.

#### Scenario: Path with leading slash is normalized
- **WHEN** constructed with `path = "/v1/resource"`
- **THEN** `BaseUri` equals the same URI as constructed with `path = "v1/resource"`

### Requirement: CreateUrlPath Handles All Arg Types Correctly
`CreateUrlPath` SHALL encode `bool true` as `"true"`, omit all-null args (no `?`),
and URL-encode keys containing special characters.

#### Scenario: Bool true serializes as "true"
- **WHEN** args contains `["active"] = true`
- **THEN** the query string contains `active=true`

#### Scenario: All-null args produces no query string
- **WHEN** every value in args is null
- **THEN** the URL is returned unchanged (no `?` appended)

#### Scenario: Special characters in key are URL-encoded
- **WHEN** args contains a key with a space character
- **THEN** the key appears URL-encoded in the query string
