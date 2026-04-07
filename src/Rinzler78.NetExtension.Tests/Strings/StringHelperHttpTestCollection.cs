using Rinzler78.NetExtension.Tests.TestHelpers;
using Xunit;

namespace Rinzler78.NetExtension.Tests.Strings;

[CollectionDefinition(nameof(StringHelperHttpTestCollection))]
public sealed class StringHelperHttpTestCollection
    : ICollectionFixture<WireMockServerFixture>
{
}
