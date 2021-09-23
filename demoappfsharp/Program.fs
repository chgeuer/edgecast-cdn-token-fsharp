open System
open CommandLine
open EdgecastCrypto

// --direction Encrypt --key yyy --ipaddress 128.0.0.1 --urls http://fee http://fee2
type MyCommandLineOptions = {
    [<Option(HelpText = "The key.", Required = true)>] key : string;
    [<Option(HelpText = "'Encrypt' or 'Decrypt'.", Required = true)>] direction : Direction;
    [<Option(HelpText = "Client IP address.")>] ipaddress : string option;
    [<Option(HelpText = "Token to decrypt.")>] token : string;
    [<Option("urls", HelpText = "Allowed URLs.")>] allowedUrls : seq<string>;
}
and Direction =
    | Encrypt = 1
    | Decrypt = 2

[<EntryPoint>]
let main argv =
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

    let args = match Parser.Default.ParseArguments<MyCommandLineOptions>(argv) with
               | :? Parsed<MyCommandLineOptions> as parsed -> parsed.Value
               | :? NotParsed<MyCommandLineOptions> as notParsed -> failwith "Could not parse"
               |  _ -> failwith "Could not parse"

    match args.direction with
    | Direction.Encrypt-> 
        createTokenValidFor (TimeSpan.FromDays(365.0))
        |> maybe withClientIPAddress args.ipaddress
        |> forall addAllowedUrl args.allowedUrls 
        |> encrypt args.key
        |> printfn "%s"
        |> ignore
    | Direction.Decrypt ->
        args.token
        |> decrypt args.key
        |> printfn "%A"
    | _ -> failwith "Unknown operation"
        
    0