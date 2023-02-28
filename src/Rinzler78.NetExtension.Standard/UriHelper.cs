using System;
using System.Runtime.CompilerServices;

namespace Rinzler78.NetExtension;

public static class UriHelper
{
    public static string ToFullEnpoint(this Uri uri)
        => $"{uri.Scheme}://{uri.Host}:{uri.Port}/";
}

