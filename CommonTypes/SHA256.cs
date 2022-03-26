using System.Text;

namespace SimpleChatApp.Client;

public static class SHA256
{
    private static readonly System.Security.Cryptography.SHA256 _mySHA256 = System.Security.Cryptography.SHA256.Create();

    private static Stream GenerateStreamFromString(string str) =>
        new MemoryStream(Encoding.Unicode.GetBytes(str));

    public static async Task<string> GetStringHash(string password, CancellationToken cancellationToken = default)
    {
        using var stream = GenerateStreamFromString(password);
        return Encoding.Unicode.GetString(await _mySHA256.ComputeHashAsync(stream, cancellationToken));
    }
}