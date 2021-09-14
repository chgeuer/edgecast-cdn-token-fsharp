# edgecast-cdn-token-fsharp

This project demonstrates how to generate access tokens for the "Verizon Digital Media Services / Edgecast Content Delivery Network CDN" in .NET. 

The actual implementation is in .NET in the F# language, so you can easily plug it into your .NET core web app, or call it from F# Interactive. 
It offers a fluent API for both C# and F#, so token creation is self-explanatory.

## F# interactive sample

Just run `dotnet fsi ./edgecast.fsx`

```fsharp
#r "nuget: BouncyCastle.NetCore, 1.8.8"
#load "edgecastlib/EdgecastCrypto.fs"

open System
open EdgecastCrypto

let inspect msg a =
    printfn "%s: %A" msg a
    a

let key = "primary202109099dc4cf480b17a94f5eef938bdb08c18535bcc777cc0420c29133d0134d635aa78a1e28f6b883619ed5f920bd3cd79bfe10c42b5d96b7eeb84571ceee4cb51d89"

createTokenValidFor (TimeSpan.FromDays(365.0))
|> inspect "token"
|> encrypt key
|> inspect "encrypted"
|> decrypt key
|> inspect "decrypted again"
|> ignore
```

This creates a result, similar to this:

```text
token: { ExpirationDate = 9/14/2022 4:50:46 PM
  ClientIPAddress = None
  AllowedCountries = ["DE"]
  DeniedCountries = []
  AllowedReferrers = []
  DeniedReferrers = []
  AllowedProtocol = []
  DeniedProtocol = []
  AllowedUrls = ["/assets"] }

encrypted: "6CM31tJXEmlAeh11HC-23FlVO96xW9udR_gCPcoX5uzQXnpQo2ThqPEUHO1AuAuLiCvJ-dijWiZHYZeRmdSUpku6I7twHn1AY0w4oECe9HQm6Z-WiatHKwU"

decrypted again: { ExpirationDate = 9/14/2022 4:50:46 PM
  ClientIPAddress = None
  AllowedCountries = ["DE"]
  DeniedCountries = []
  AllowedReferrers = []
  DeniedReferrers = []
  AllowedProtocol = []
  DeniedProtocol = []
  AllowedUrls = ["/assets"] }
```

## C# sample

```csharp
using System;
using EdgecastCryptoExtensions;

class Program
{
    static void Main()
    {
        var key = "primary202109099dc4cf480b17a94f5eef938bdb08c18535bcc777cc0420c29133d0134d635aa78a1e28f6b883619ed5f920bd3cd79bfe10c42b5d96b7eeb84571ceee4cb51d89";

        _ = EdgecastCrypto.createTokenValidFor(TimeSpan.FromDays(365.0))
            .AddAllowedCountry("DE")
            .AddAllowedUrl("/assets")
            .Inspect("token")
            .Encrypt(key)
            .Inspect("encrypted")
            .Decrypt(key)
            .Inspect("decrypted");
    }
}

static class Util
{
    internal static T Inspect<T>(this T t, string message)
    {
        Console.Out.WriteLine($"{message}: {t}");
        return t;
    }
}
```

## Links

- [Edgecast Token-Based Authentication Administration Guide](https://docs.edgecast.com/pdfs/VDMS_Token-Based_Authentication_Admin_Guide.pdf)
- [Edgecast Rules Engine v4 User Guide](https://docs.edgecast.com/pdfs/VDMS_Rules_Engine_v4_User_Guide.pdf)
- [Edgecast "HTTP Large" Admin Guide](https://docs.edgecast.com/pdfs/VDMS_HTTP_Large_Admin_Guide.pdf)

