namespace EdgecastCryptoExtensions

open System.Runtime.CompilerServices
open EdgecastCrypto

[<Extension>]
type public EdgecastCryptoExtensionsForCSharp = 
    [<Extension>] 
    static member WithClientIPAddress token x = token |> withClientIPAddress x

    [<Extension>] 
    static member AddAllowedCountry token country = token |> addAllowedCountry country
    [<Extension>] 
    static member AddDeniedCountry token country = token |> addDeniedCountry country
    [<Extension>] 
    static member AddAllowedReferrer token referrer = token |> addAllowedReferrer referrer
    [<Extension>] 
    static member AddDeniedReferrer token referrer = token |> addDeniedReferrer referrer
    [<Extension>] 
    static member AddAllowedUrl token url = token |> addAllowedUrl url

    [<Extension>] 
    static member AllowHttp token = token |> allowHttp
    [<Extension>] 
    static member AllowHttps token = token |> allowHttps
    [<Extension>] 
    static member DenyHttp token = token |> denyHttp
    [<Extension>] 
    static member DenyHttps token = token |> denyHttps
    
    [<Extension>] 
    static member Encrypt token key   = token |> encrypt key    
    [<Extension>] 
    static member Decrypt str key = str |> decrypt key
