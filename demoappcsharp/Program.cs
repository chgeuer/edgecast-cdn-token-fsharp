using System;
using EdgecastCryptoExtensions;

class Program
{
    static void Main(string[] args)
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