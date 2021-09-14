open System
open EdgecastCrypto

[<EntryPoint>]
let main argv =
    let inspect msg a =
        printfn "%s: %A" msg a
        a

    let key = "primary202109099dc4cf480b17a94f5eef938bdb08c18535bcc777cc0420c29133d0134d635aa78a1e28f6b883619ed5f920bd3cd79bfe10c42b5d96b7eeb84571ceee4cb51d89"

    createTokenValidFor (TimeSpan.FromDays(365.0))
    |> addAllowedCountry "DE"
    |> addAllowedUrl "/assets"
    |> inspect "token"
    |> encrypt key
    |> inspect "encrypted"
    |> decrypt key
    |> inspect "decrypted again"
    |> ignore

    0