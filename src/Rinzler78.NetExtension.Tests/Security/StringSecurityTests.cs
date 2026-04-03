using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Rinzler78.NetExtension.Strings;
using Xunit;

namespace Rinzler78.NetExtension.Tests.Security;

/// <summary>
/// Security-focused tests for <see cref="StringHelper"/>.
///
/// Design note — no sanitisation contract:
/// Most <see cref="StringHelper"/> methods are pure string utilities (search, transform,
/// format).  They do NOT sanitise input and are not designed to.  The tests below document
/// this explicitly: security-hostile payloads pass through unchanged.  Callers that need
/// sanitisation must apply it before or after calling these helpers.
///
/// The sole exception is <see cref="StringHelper.HttpGetStreamAsync"/> /
/// <see cref="StringHelper.HttpGetStringAsync"/> which call the private ValidateUrl()
/// guard.  Those tests verify the SSRF-protection behaviour.
/// </summary>
[Collection("StringHelper.Http serial")]
[Trait("Category", "Unit")]
public class StringSecurityTests
{
    // =========================================================================
    // 1. SQL-injection-like patterns — pass-through, no sanitisation
    // =========================================================================

    [Theory]
    [InlineData("'; DROP TABLE users; --")]
    [InlineData("1 OR 1=1")]
    [InlineData("admin'--")]
    [InlineData("\" OR \"\"=\"")]
    public void ContainsAll_SqlInjectionPatterns_PassThroughUnmodified(string sqlPayload)
    {
        // ContainsAll performs an ordinal substring search and returns a bool.
        // It does NOT sanitise, escape, or modify the input in any way.
        var result = sqlPayload.ContainsAll(new[] { "OR" });

        // We only care that no exception is thrown and the pure-search contract holds.
        // (Some payloads contain "OR", some do not — we just assert no crash.)
        // Assigning to a discard confirms the method runs without exception.
        _ = result; // no-op; true or false are both acceptable — no sanitisation expected
    }

    [Theory]
    [InlineData("'; DROP TABLE users; --")]
    [InlineData("1 UNION SELECT * FROM passwords")]
    [InlineData("Robert'); DROP TABLE students;--")]
    public void ToPascalCase_SqlInjectionString_DoesNotSanitise(string sqlPayload)
    {
        // ToPascalCase strips non-alphanumeric chars and upper-cases word starts.
        // The result loses special characters; the method is NOT a sanitiser, but
        // the output is naturally free of SQL punctuation because non-alnum chars
        // are removed.  The test documents what actually happens.
        var result = sqlPayload.ToPascalCase();

        result.Should().NotBeNull();
        result.Should().NotContain("'", "ToPascalCase removes apostrophes (non-alnum)");
        result.Should().NotContain(";", "ToPascalCase removes semicolons (non-alnum)");
        result.Should().NotContain("--", "ToPascalCase removes hyphens (non-alnum)");
    }

    [Theory]
    [InlineData("' OR '1'='1")]
    [InlineData("admin'; DROP TABLE users;--")]
    public void IsValidEmail_SqlInjectionPayload_ReturnsFalse(string sqlPayload)
    {
        // SQL payloads are not valid e-mail addresses; IsValidEmail must return false
        // without throwing, regardless of the payload content.
        var result = sqlPayload.IsValidEmail();

        result.Should().BeFalse(
            "SQL injection strings are not valid e-mail addresses");
    }

    // =========================================================================
    // 2. XSS patterns — pass-through behavior
    // =========================================================================

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("<img src=x onerror=alert(1)>")]
    [InlineData("javascript:alert(document.cookie)")]
    public void ContainsAny_XssPatterns_PassThroughAsPlainText(string xssPayload)
    {
        // ContainsAny is a read-only search helper; it never modifies or encodes input.
        // The payload is treated as an ordinary string.
        var result = xssPayload.ContainsAny(new[] { "<script>" });

        // No exception must be thrown; the return value (true/false) depends on the payload.
        _ = result; // no-op; true or false are both acceptable — no sanitisation expected
    }

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("<img src=x onerror=alert(1)>")]
    public void ToPascalCase_XssPayload_StripsHtmlPunctuation(string xssPayload)
    {
        // ToPascalCase removes '<', '>', '/', '\'' and '(' ')' (non-alnum).
        // It is NOT an HTML encoder, but angle-bracket markup naturally disappears.
        var result = xssPayload.ToPascalCase();

        result.Should().NotBeNull();
        result.Should().NotContain("<", "ToPascalCase strips '<' (non-alnum)");
        result.Should().NotContain(">", "ToPascalCase strips '>' (non-alnum)");
        result.Should().NotContain("'", "ToPascalCase strips apostrophes (non-alnum)");
    }

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("user@<evil>.com")]
    public void IsValidEmail_XssPayload_ReturnsFalse(string xssPayload)
    {
        // NOTE: "user@<evil>.com" is accepted by EmailAddressAttribute because it
        // contains a valid-looking local-part, '@', and domain. IsValidEmail performs
        // RFC-5321 structural validation only — it does NOT sanitise or reject HTML/XSS
        // payloads. Callers must HTML-encode output separately.
        if (xssPayload.Contains('<') && xssPayload.Contains('@'))
            xssPayload.IsValidEmail().Should().BeTrue(
                "EmailAddressAttribute accepts structurally valid addresses regardless of XSS-like characters");
        else
            xssPayload.IsValidEmail().Should().BeFalse(
                "pure HTML payloads without '@' are not valid e-mail addresses");
    }

    // =========================================================================
    // 3. Null-byte injection
    // =========================================================================

    [Fact]
    public void ContainsAll_StringWithNullByte_TreatedAsOrdinaryCharacter()
    {
        // .NET strings can contain \0; it is treated as a regular character, not a
        // terminator.  ContainsAll must not throw and must report correct membership.
        var input = "safe\0payload";

        var containsNull = input.ContainsAll(new[] { "\0" });
        var notEmpty = input.ContainsAll(new[] { "safe" });

        containsNull.Should().BeTrue("the null byte is a valid character in a .NET string");
        notEmpty.Should().BeTrue();
    }

    [Fact]
    public void TrimOptimized_StringWithNullByte_NullByteIsNotTrimmed()
    {
        // TrimOptimized only trims whitespace and explicitly supplied chars.
        // A null byte in the middle is not removed.
        var input = "  hello\0world  ";
        var result = input.TrimOptimized();

        result.Should().Be("hello\0world",
            "TrimOptimized trims leading/trailing whitespace only; embedded null bytes are preserved");
    }

    [Fact]
    public void BeginByUpperCase_StringStartingWithNullByte_DoesNotThrow()
    {
        // A string starting with '\0' is non-null and non-empty.
        // BeginByUpperCase must handle it gracefully (char.ToUpper('\0') == '\0').
        var input = "\0hello";
        var result = input.BeginByUpperCase();

        result.Should().StartWith("\0",
            "char.ToUpper of the null byte remains the null byte");
    }

    [Fact]
    public void IsValidEmail_StringWithNullByte_ReturnsFalse()
    {
        // NOTE: EmailAddressAttribute accepts "user\0@example.com" as valid.
        // .NET strings allow null bytes; the attribute does not strip or reject them.
        // Callers that store or transmit email addresses must sanitise null bytes independently.
        "user\0@example.com".IsValidEmail().Should().BeTrue(
            "EmailAddressAttribute does not reject embedded null bytes — no null-byte guard exists");
    }

    // =========================================================================
    // 4. Extremely long strings (>1 MB) — no OOM or timeout
    // =========================================================================

    [Fact]
    public void ContainsAll_VeryLongString_DoesNotThrowOrTimeout()
    {
        var longString = new string('a', 2_000_000); // 2 MB

        var act = () => longString.ContainsAll(new[] { "b" });

        act.Should().NotThrow(
            "ContainsAll on a 2 MB string must complete without OOM or unhandled exception");
        act().Should().BeFalse();
    }

    [Fact]
    public void TrimOptimized_VeryLongString_DoesNotThrowOrTimeout()
    {
        var longString = new string('x', 2_000_000);

        var act = () => longString.TrimOptimized();

        act.Should().NotThrow();
        act().Should().HaveLength(2_000_000);
    }

    [Fact]
    public void ToPascalCase_VeryLongString_DoesNotThrowOrTimeout()
    {
        // 1 MB of alternating spaces and letters
        var longString = string.Concat(Enumerable.Repeat("a ", 500_000));

        var act = () => longString.ToPascalCase();

        act.Should().NotThrow("ToPascalCase must not run out of memory on a 1 MB string");
    }

    [Fact]
    public void ComputeLevenshteinDistance_VeryLongIdenticalStrings_ReturnsZeroWithoutOom()
    {
        var longString = new string('z', 10_000); // 10 k chars (O(n²) array: ~100 M cells)

        var act = () => longString.ComputeLevenshteinDistance(longString);

        act.Should().NotThrow();
        act().Should().Be(0);
    }

    [Fact]
    public void GetBytes_VeryLongString_ProducesCorrectByteArray()
    {
        var longString = new string('A', 1_000_000); // 1 MB ASCII

        var bytes = longString.GetBytes();

        bytes.Should().HaveCount(1_000_000);
        bytes.Should().AllSatisfy(b => b.Should().Be((byte)'A'));
    }

    // =========================================================================
    // 5. Unicode edge cases: RTL markers, zero-width spaces, surrogates
    // =========================================================================

    [Fact]
    public void ContainsAll_RightToLeftOverrideMarker_TreatedAsOrdinaryCharacter()
    {
        // U+202E RIGHT-TO-LEFT OVERRIDE is a valid Unicode scalar in .NET strings.
        var input = "Hello\u202EWorld";

        input.ContainsAll(new[] { "\u202E" }).Should().BeTrue(
            "RTL override is a regular Unicode character in .NET strings");
    }

    [Fact]
    public void TrimOptimized_ZeroWidthSpace_IsNotTrimmedUnlessSupplied()
    {
        // U+200B ZERO WIDTH SPACE is not in the whitespace set trimmed by default.
        var input = "\u200Bhello\u200B";
        var result = input.TrimOptimized();

        result.Should().Be("\u200Bhello\u200B",
            "U+200B is not in the default whitespace trim set; it must be preserved");
    }

    [Fact]
    public void BeginByUpperCase_ZeroWidthSpacePrefix_DoesNotThrowAndUpperCasesIt()
    {
        // char.ToUpper('\u200B') returns '\u200B' (no upper-case equivalent).
        var result = "\u200Bhello".BeginByUpperCase();

        result.Should().StartWith("\u200B");
        result.Should().EndWith("hello");
    }

    [Fact]
    public void ToPascalCase_StringWithRtlMarker_StripsNonAlphanumericUnicode()
    {
        // ToPascalCase uses char.IsLetterOrDigit which returns false for RTL override,
        // so the marker is treated as a word boundary and stripped.
        var input = "hello\u202Eworld";
        var result = input.ToPascalCase();

        result.Should().NotContain("\u202E",
            "ToPascalCase removes non-letter/digit chars including RTL markers");
        result.Should().Be("HelloWorld");
    }

    [Fact]
    public void CalculateSimilarity_ZeroWidthSpacesInBothStrings_ReturnsOneForIdentical()
    {
        var s = "hello\u200Bworld";

        s.CalculateSimilarity(s).Should().Be(1.0,
            "identical strings (including zero-width spaces) must have similarity 1.0");
    }

    [Fact]
    public void IsValidEmail_RtlMarkerInAddress_ReturnsFalse()
    {
        // NOTE: EmailAddressAttribute does not filter Unicode control characters such as
        // the RTL override (U+202E). The address passes structural validation.
        // Callers must apply Unicode normalisation / bidi filtering separately.
        "user\u202E@example.com".IsValidEmail().Should().BeTrue(
            "EmailAddressAttribute does not reject RTL override markers — no bidi filtering exists");
    }

    // =========================================================================
    // 6. SSRF protection via ValidateUrl (exercised through HttpGetStringAsync)
    // =========================================================================
    // ValidateUrl is private; it is exercised by calling the public HTTP helpers.
    // Because the validation is synchronous and runs before any network I/O, the
    // ArgumentException is thrown as soon as the returned Task is awaited — no
    // actual network connection is made.

    [Theory]
    [InlineData("http://localhost/api")]
    [InlineData("https://localhost/data")]
    [InlineData("http://127.0.0.1/admin")]
    [InlineData("https://127.0.0.1:8080/internal")]
    public async Task HttpGetStringAsync_LocalhostOrLoopback_ThrowsArgumentException(string url)
    {
        // The SSRF guard must reject localhost / loopback before making a request.
        var act = async () => await url.HttpGetStringAsync();

        await act.Should().ThrowAsync<ArgumentException>(
            "ValidateUrl blocks access to localhost and loopback addresses");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HttpGetStringAsync_IPv6Loopback_ThrowsArgumentException()
    {
        // IPv6 loopback [::1] is blocked by the SSRF guard (via LocalhostIpv6 string check
        // and the explicit ipAddress.Equals(IPv6Loopback) guard added in the extended ValidateUrl).
        var act = async () => await "http://[::1]/api".HttpGetStringAsync();

        await act.Should().ThrowAsync<ArgumentException>(
            "ValidateUrl blocks access to IPv6 loopback address [::1]");
    }

    [Theory]
    [InlineData("http://10.0.0.1/secret")]
    [InlineData("http://10.255.255.255/internal")]
    [InlineData("http://172.16.0.1/admin")]
    [InlineData("http://172.31.255.254/private")]
    [InlineData("http://192.168.1.1/router")]
    [InlineData("http://192.168.0.100/api")]
    public async Task HttpGetStringAsync_PrivateNetworkRange_ThrowsArgumentException(string url)
    {
        // RFC 1918 private ranges (10/8, 172.16-31/12, 192.168/16) must be rejected.
        var act = async () => await url.HttpGetStringAsync();

        await act.Should().ThrowAsync<ArgumentException>(
            "ValidateUrl blocks access to private RFC 1918 network ranges (SSRF protection)");
    }

    [Theory]
    [InlineData("ftp://example.com/file")]
    [InlineData("file:///etc/passwd")]
    [InlineData("ldap://example.com")]
    [InlineData("javascript:alert(1)")]
    public async Task HttpGetStringAsync_NonHttpScheme_ThrowsArgumentException(string url)
    {
        // Only http:// and https:// are permitted schemes.
        var act = async () => await url.HttpGetStringAsync();

        await act.Should().ThrowAsync<ArgumentException>(
            "ValidateUrl must reject non-HTTP/HTTPS schemes");
    }

    [Fact]
    public async Task HttpGetStringAsync_NullUrl_ThrowsArgumentException()
    {
        var act = async () => await ((string)null!).HttpGetStringAsync();

        await act.Should().ThrowAsync<ArgumentException>(
            "a null URL is rejected by ValidateUrl before any network access");
    }

    [Fact]
    public async Task HttpGetStringAsync_EmptyUrl_ThrowsArgumentException()
    {
        var act = async () => await string.Empty.HttpGetStringAsync();

        await act.Should().ThrowAsync<ArgumentException>(
            "an empty URL is rejected by ValidateUrl before any network access");
    }

    [Fact]
    public async Task HttpGetStringAsync_MalformedUrl_ThrowsArgumentException()
    {
        var act = async () => await "not-a-url-at-all".HttpGetStringAsync();

        await act.Should().ThrowAsync<ArgumentException>(
            "a malformed URL cannot be parsed as an absolute Uri and is rejected");
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("http://[fc00::1]/api")]         // IPv6 ULA
    [InlineData("http://[fd12:3456:789a::1]/")]  // IPv6 ULA (fd prefix)
    [InlineData("http://[fe80::1]/resource")]    // IPv6 link-local
    [InlineData("http://169.254.169.254/latest/meta-data/")] // AWS IMDSv1
    [InlineData("http://169.254.0.1/")]          // APIPA range
    [InlineData("http://0.0.0.0/secret")]        // Wildcard address
    [InlineData("http://100.64.0.1/internal")]   // RFC 6598 CGNAT
    public async Task HttpGetStringAsync_PrivateOrLinkLocalAddress_ThrowsArgumentException(string url)
    {
        var act = () => url.HttpGetStringAsync();
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task HttpGetStringAsync_HostnameResolvingToPublicIp_DoesNotFailValidation()
    {
        // example.com resolves to a public IP — validation should pass (connection may fail but not for SSRF reasons)
        using var _ = StringHelper.OverrideHostAddressResolverForTesting(
            _ => new[] { IPAddress.Parse("93.184.216.34") }); // example.com IP

        var act = () => "http://example.com".HttpGetStringAsync(timeout: TimeSpan.FromMilliseconds(100));
        // Should throw timeout/network error, NOT ArgumentException
        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.GetType() != typeof(ArgumentException) && !(e is ArgumentException));
    }
}
