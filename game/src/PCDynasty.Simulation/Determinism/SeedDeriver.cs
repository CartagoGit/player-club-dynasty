using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace PCDynasty.Simulation.Determinism;

public static class SeedDeriver
{
    private const string Domain = "pcdynasty-seed-v1\n";

    public static ulong Derive(ulong rootSeed, params string[] scopes)
    {
        ArgumentNullException.ThrowIfNull(scopes);

        using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Append(hash, Domain);
        Append(hash, rootSeed.ToString(CultureInfo.InvariantCulture));
        Append(hash, "\n");
        foreach (string scope in scopes)
        {
            ArgumentNullException.ThrowIfNull(scope);
            int byteCount = Encoding.UTF8.GetByteCount(scope);
            Append(hash, byteCount.ToString(CultureInfo.InvariantCulture));
            Append(hash, ":");
            Append(hash, scope);
            Append(hash, "\n");
        }

        byte[] digest = hash.GetHashAndReset();
        return BinaryPrimitives.ReadUInt64BigEndian(digest);
    }

    private static void Append(IncrementalHash hash, string value) =>
        hash.AppendData(Encoding.UTF8.GetBytes(value));
}
