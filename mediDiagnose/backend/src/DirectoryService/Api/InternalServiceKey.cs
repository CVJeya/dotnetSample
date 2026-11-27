using System.Security.Cryptography;
using System.Text;

namespace DirectoryService.Api;

public sealed class InternalServiceKey
{
    private const string HeaderName = "X-Internal-Service-Key";
    private readonly byte[] _key;

    public InternalServiceKey(string key)
    {
        _key = Encoding.UTF8.GetBytes(key);
    }

    public bool IsValid(HttpRequest request)
    {
        if (!request.Headers.TryGetValue(HeaderName, out var values) || values.Count != 1)
        {
            return false;
        }

        var suppliedKey = Encoding.UTF8.GetBytes(values[0]!);
        return suppliedKey.Length == _key.Length
            && CryptographicOperations.FixedTimeEquals(suppliedKey, _key);
    }
}
