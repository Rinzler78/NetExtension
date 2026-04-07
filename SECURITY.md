# Security Policy

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 1.0.0    | ✅ Current release   |

## Reporting a Vulnerability

**Please do not report security vulnerabilities through public GitHub issues.**

If you discover a security vulnerability in this library, please report it
privately to minimise risk before a fix is available.

### How to Report

1. **Open a [GitHub Security Advisory](https://github.com/Rinzler78/NetExtension/security/advisories/new)**
   — this keeps the report private until a fix is published.

2. Alternatively, open a **private** issue if you have collaborator access.

### What to Include

- A clear description of the vulnerability and its potential impact.
- Steps to reproduce or a minimal proof-of-concept.
- The affected version(s) and .NET runtime version.
- Any suggested mitigation or patch (optional but appreciated).

### Response Timeline

| Stage | Target |
|-------|--------|
| Initial acknowledgement | Within **48 hours** |
| Triage and severity assessment | Within **5 business days** |
| Fix or mitigation published | Depends on severity (critical: < 7 days, others: < 30 days) |
| Public disclosure | After fix is released |

## Scope

This library provides utility extensions for .NET applications. Areas of particular
security relevance include:

- **HTTP utilities** (`StringHelper.HttpGetAsync<T>`, `HttpGetStringAsync`, `HttpPostString`) — SSRF protection
  via `ValidateUrl` blocks private ranges, loopback, and link-local addresses.
- **File path utilities** (`JsonHelper.DeserializeObjectFromFile`) — path traversal
  protection via allowlist of safe file extensions.
- **Cryptographic helpers** (`Crc32Helper`, `Crc64Helper`, `Sha512Helper`) — CRC values
  are checksums, not cryptographic hashes; do not use for security-sensitive comparisons.

## Out of Scope

- Vulnerabilities in third-party dependencies (report those to the respective maintainers).
- Issues that require physical access to a machine.
- Social engineering attacks.
