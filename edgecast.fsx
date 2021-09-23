#r "nuget: BouncyCastle.NetCore, 1.8.8"
#r "nuget: CommandLineParser.FSharp, 2.8.0"
#load "edgecastlib/EdgecastCrypto.fs"

open System
open EdgecastCrypto
open CommandLine

// --direction Encrypt --key yyy --ipaddress 128.0.0.1 --urls http://fee http://fee2
type MyCommandLineOptions = {
    [<Option(HelpText = "The key.", Required = true)>] Key : string;
    [<Option(HelpText = "'Encrypt' or 'Decrypt'.", Required = true)>] Direction : Direction;
    [<Option(HelpText = "Client IP address.")>] IPAddress : string option;
    [<Option(HelpText = "Token to decrypt.")>] Token : string;
    [<Option("urls", HelpText = "Allowed URLs.")>] AllowedUrls : seq<string>;
}
and Direction =
    | Encrypt = 1
    | Decrypt = 2

let inspect msg a =
    printfn "%s: %A" msg a
    a

let maybe func optionalArg =
    match optionalArg with
    | Some(arg) -> func arg
    | None -> id

let forall (func : 'a -> 't -> 't) (l : 'a seq) (t: 't) : 't =
    match (Seq.isEmpty l) with
    | true -> t
    | false -> Seq.fold (fun t a -> func a t) t l

let argv = fsi.CommandLineArgs |> Array.tail
let args = match Parser.Default.ParseArguments<MyCommandLineOptions>( argv ) with
            | :? Parsed<MyCommandLineOptions> as parsed -> parsed.Value
            | :? NotParsed<MyCommandLineOptions> as notParsed -> failwith "Could not parse"
            |  _ -> failwith "Could not parse"

match args.Direction with
| Direction.Encrypt -> 
    createTokenValidFor (TimeSpan.FromDays(365.0))
    |> maybe withClientIPAddress args.IPAddress
    |> forall addAllowedUrl args.AllowedUrls
    |> encrypt args.Key
    |> printfn "%s"
    |> ignore
| Direction.Decrypt ->
    args.Token
    |> inspect "the token"
    |> decrypt args.Key
    |> printfn "%A"
| _ -> failwith "Unknown operation"

// createTokenValidFor (TimeSpan.FromDays(365.0))
// |> withClientIPAddress address
// |> inspect "token"
// |> encrypt key
// |> inspect "encrypted"
// |> decrypt key
// |> inspect "decrypted again"
// |> ignore
