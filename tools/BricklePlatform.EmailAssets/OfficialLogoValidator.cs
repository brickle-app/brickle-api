using System.Buffers.Binary;
using System.Security.Cryptography;

namespace BricklePlatform.EmailAssets;

public static class OfficialLogoValidator
{
    private static readonly byte[] PngSignature = [137, 80, 78, 71, 13, 10, 26, 10];
    private const string ApprovedSha256 = "8cbdba94035d8cdab4a945742e46d4c83eb8368d9ea12e0e0120fce0e3f589e7";

    public static void Validate(string path)
    {
        var bytes = File.ReadAllBytes(path);
        if (bytes.Length < 33 || !bytes.AsSpan(0, 8).SequenceEqual(PngSignature))
            throw new InvalidDataException("Logo must have the exact PNG signature.");

        var width = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(16, 4));
        var height = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(20, 4));
        if (width != 518 || height != 164)
            throw new InvalidDataException("Logo must be exactly 518 x 164 pixels.");

        if (bytes[25] != 6)
            throw new InvalidDataException("Logo must use PNG RGBA color type 6.");

        var actualSha256 = Convert.ToHexString(SHA256.HashData(bytes));
        if (!string.Equals(actualSha256, ApprovedSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Logo does not match the approved SHA-256.");
    }
}
