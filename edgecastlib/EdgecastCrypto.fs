module EdgecastCrypto

open System
open System.IO 
open Org.BouncyCastle.Crypto
open Org.BouncyCastle.Crypto.Engines
open Org.BouncyCastle.Crypto.Modes
open Org.BouncyCastle.Security
open Org.BouncyCastle.Crypto.Parameters
    
type Token =
    { ExpirationDate: DateTime
      ClientIPAddress: string option
      AllowedCountries: string list 
      DeniedCountries: string list
      AllowedReferrers: string list
      DeniedReferrers: string list
      AllowedProtocol: string list
      DeniedProtocol: string list
      AllowedUrls: string list }

module internal Token =
    let private ec_expire = "ec_expire"
    let private ec_clientip = "ec_clientip"
    let private ec_country_allow = "ec_country_allow"
    let private ec_country_deny = "ec_country_deny"
    let private ec_ref_allow = "ec_ref_allow"
    let private ec_ref_deny = "ec_ref_deny"
    let private ec_proto_allow = "ec_proto_allow"
    let private ec_proto_deny = "ec_proto_deny"
    let private ec_url_allow = "ec_url_allow"

    let internal tokenToString (t: Token) =
        let getEpoch (t: DateTime) = 
            let e = (new DateTime(1970, 1, 1)).ToUniversalTime()

            (t.ToUniversalTime().Subtract(e).TotalSeconds)
            |> int
            |> string

        let someStr (parameterName : string) (l : string option) (str : string) =
            match l with
            | None -> str
            | Some(v) -> $"{str}&{parameterName}={v}"

        let listStr (parameterName : string) (l : string list) (str : string) =
            match l with
            | []  -> str
            | _ -> l
                |> List.rev
                |> List.distinct
                |> String.concat ","
                |> (fun v -> $"{str}&{parameterName}={v}")

        $"{ec_expire}={t.ExpirationDate |> getEpoch}"
        |>  someStr ec_clientip t.ClientIPAddress
        |>  listStr ec_country_allow t.AllowedCountries
        |>  listStr ec_country_deny t.DeniedCountries
        |>  listStr ec_ref_allow t.AllowedReferrers
        |>  listStr ec_ref_deny t.DeniedReferrers
        |>  listStr ec_proto_allow t.AllowedProtocol
        |>  listStr ec_proto_deny t.DeniedProtocol
        |>  listStr ec_url_allow t.AllowedUrls

    let internal tokenFromStr (str : string) =
        let utcDateTime name x =            
            x
            |> Seq.find(fun (k, v) -> k = name) 
            |> snd
            |> Int32.Parse
            |> float
            |> (new DateTime(1970, 1, 1)).ToUniversalTime().AddSeconds

        let singleString name x =
            x
            |> Seq.tryFind(fun (k, _) -> k = name )
            |> function
                | Some(_, ip) -> Some(ip)
                | _ -> None

        let stringList name (x : seq<string*string>) =
            x
            |> Seq.tryFind(fun (k, _) -> k = name)
            |> function
                | Some(_, v) -> v.Split(',') |> Array.toList
                | _ -> list.Empty

        let parseElement (str : string) =
            str
            |> (fun s -> s.Split([|'='|], count = 2))
            |> Array.toList
            |> function
                | [k; v] -> Some(k, v)
                | _ -> None

        let parts = 
            str.Split([|'&'|])
            |> Seq.map parseElement
            |> Seq.choose id

        { ExpirationDate = parts |> utcDateTime ec_expire
          ClientIPAddress = parts |> singleString ec_clientip
          AllowedCountries = parts |> stringList ec_country_allow
          DeniedCountries = parts |> stringList ec_country_deny
          AllowedReferrers = parts |> stringList ec_ref_allow
          DeniedReferrers = parts |> stringList ec_ref_deny
          AllowedProtocol = parts |> stringList ec_proto_allow
          DeniedProtocol = parts |> stringList ec_proto_deny
          AllowedUrls = parts |> stringList ec_url_allow }

open Token

let createTokenValidUntil expirationDate =
    { ExpirationDate = expirationDate
      ClientIPAddress = None
      AllowedCountries = list.Empty
      DeniedCountries = list.Empty
      AllowedReferrers = list.Empty
      DeniedReferrers = list.Empty
      AllowedProtocol = list.Empty
      DeniedProtocol = list.Empty
      AllowedUrls = list.Empty }

let createTokenValidFor timeSpan =
    DateTime.UtcNow.Add(timeSpan)
    |> createTokenValidUntil

let withClientIPAddress x t = { t with ClientIPAddress = Some(x) }
let addAllowedCountry x t = { t with AllowedCountries = x :: t.AllowedCountries }
let addDeniedCountry x t = { t with DeniedCountries = x :: t.DeniedCountries }
let addAllowedReferrer x t = { t with AllowedReferrers = x :: t.AllowedReferrers }
let addDeniedReferrer x t = { t with DeniedReferrers = x :: t.DeniedReferrers }
let private addAllowedProtocol x t = { t with AllowedProtocol = x :: t.AllowedProtocol }
let private addDeniedProtocol x t = { t with DeniedProtocol = x :: t.DeniedProtocol }
let allowHttp t = t |> addAllowedProtocol "http"
let allowHttps t = t |> addAllowedProtocol "https"
let denyHttp t = t |> addDeniedProtocol "http"
let denyHttps t = t |> addDeniedProtocol "https"
let addAllowedUrl x t = { t with AllowedUrls = x :: t.AllowedUrls }

module internal Helpers =
    open System.Text
    open System.Security.Cryptography
    
    let sha256(x : byte[]) = x |> (SHA256.Create()).ComputeHash
    let trimEnd (a: char) (str: string) = str.TrimEnd(a)
    let replaceStr (a: string) (b: string) (str: string) = str.Replace(a, b)
    let replaceChar (a: char) (b: char) (str: string) = str.Replace(a, b)
    let toUTF8 (s : string) = s |> Encoding.UTF8.GetBytes
    let fromUTF8 (x:  byte[]) = x |> Encoding.UTF8.GetString

    let private removeBase64Padding = trimEnd '='
    let private restoreBase64Padding (s : string) =
        match (s.Length % 4) with
        | 0 -> s
        | 3 -> s + "="
        | 2 -> s + "=="
        | _ -> failwith "Illegal base64url string"

    let private properBase64toSafeUrl s = 
        s
        |> replaceChar '+' '-'
        |> replaceChar '/' '_'
    let private safeUrlToProperBase64 s = 
        s
        |> replaceChar '_' '/'
        |> replaceChar '-' '+'

    let toSafeBase64 (s: byte[]) = 
        s
        |> Convert.ToBase64String
        |> removeBase64Padding
        |> properBase64toSafeUrl
    let fromSafeBase64 (s: string) =
        s
        |> safeUrlToProperBase64
        |> restoreBase64Padding
        |> Convert.FromBase64String

open Helpers

let private NonceByteSize = 12

let private secureRandom = new SecureRandom()

let private createIV =
    let iv = Array.zeroCreate<byte> NonceByteSize
    secureRandom.NextBytes(iv)
    iv
    
let private createKey value =
    value
    |> toUTF8
    |> sha256
    |> (fun d -> new KeyParameter(d))

let private createCipher key iv forEncryption =
    let cipher = new GcmBlockCipher(new AesEngine())
    let parameters = new ParametersWithIV (key, iv)
    cipher.Init(forEncryption = forEncryption, parameters = parameters)
    cipher

let private enforceMaxLength length (s : string) =
    if s <> null && s.Length <= length
    then s
    else raise (ArgumentOutOfRangeException(paramName = nameof(s), message = (sprintf "String must be less than %d" length)))

let private encryptString key plainText =
    let encrypt_impl (key : KeyParameter) (plaintext: byte[]) =
        let iv = createIV
        let cipher = createCipher key iv true
        let cipherText = Array.zeroCreate<byte>(cipher.GetOutputSize(plaintext.Length))
        let len = cipher.ProcessBytes(input = plaintext, inOff = 0, len = plaintext.Length, output = cipherText, outOff = 0)
        cipher.DoFinal(cipherText, len) |> ignore
        use memoryStream = new MemoryStream()
        using (new BinaryWriter(memoryStream)) (fun binaryWriter -> 
            binaryWriter.Write(iv)
            binaryWriter.Write(cipherText)
        )
        memoryStream.ToArray()
        
    plainText
    |> enforceMaxLength 512
    |> replaceStr "ec_secure=1" ""
    |> replaceStr "&&" "&"
    |> toUTF8
    |> encrypt_impl (createKey key)
    |> toSafeBase64

let private decryptString key cipherText =
    let decrypt_impl (key : KeyParameter) (ciphertext: byte[]) =
        try
            use cipherStream = new MemoryStream (ciphertext)
            use cipherReader = new BinaryReader (cipherStream)
            let iv = cipherReader.ReadBytes(NonceByteSize)
            let cipher = createCipher key iv false
            let cipherText = cipherReader.ReadBytes(ciphertext.Length - NonceByteSize)
            let plainText = Array.zeroCreate<byte>(cipher.GetOutputSize(cipherText.Length))
            let len = cipher.ProcessBytes(input = cipherText, inOff = 0, len = cipherText.Length, output = plainText, outOff = 0)
            cipher.DoFinal(plainText, len) |> ignore
            plainText
        with 
        | :? InvalidCipherTextException -> Array.empty

    cipherText
    |> enforceMaxLength 512
    |> fromSafeBase64
    |> decrypt_impl (createKey key)
    |> fromUTF8

let encrypt key token =
    token
    |> tokenToString
    |> encryptString key

let decrypt key tokenStr =
    tokenStr 
    |> decryptString key
    |> tokenFromStr

