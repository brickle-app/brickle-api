using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;
using BricklePlatform.EmailAssets;
using Xunit;

namespace BricklePlatform.Test.Services;

public class EmailLogoAssetTests
{
    private static readonly byte[] PngSignature = [137, 80, 78, 71, 13, 10, 26, 10];

    [Fact]
    public void OfficialEmailLogoHasExpectedSvgSourceAndPngOutput()
    {
        var root = FindRepositoryRoot();
        var svgPath = Path.Combine(root, "assets", "branding", "email", "brickle-email-logo.svg");
        var pngPath = Path.Combine(root, "assets", "branding", "email", "brickle-email-logo.png");

        Assert.True(File.Exists(svgPath), $"Missing SVG source: {svgPath}");
        var svg = File.ReadAllText(svgPath);
        Assert.Contains("viewBox=\"0 0 259 82\"", svg);
        Assert.Contains("#1C3647", svg);
        Assert.Contains("#EB7F58", svg);

        Assert.True(File.Exists(pngPath), $"Missing PNG output: {pngPath}");
        var png = File.ReadAllBytes(pngPath);
        Assert.True(png.Length > 33, "PNG is too short to contain IHDR data.");
        Assert.Equal(PngSignature, png[..8]);
        Assert.Equal("IHDR", System.Text.Encoding.ASCII.GetString(png, 12, 4));
        Assert.Equal(518, BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16, 4)));
        Assert.Equal(164, BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20, 4)));
        Assert.Equal(6, png[25]); // RGBA.

        var pixels = DecodeRgbaPixels(png, 518, 164);
        Assert.Contains(Enumerable.Range(0, 518 * 164), index => pixels[index * 4 + 3] < byte.MaxValue);
        Assert.Contains(Enumerable.Range(0, 518 * 164), index => pixels[index * 4 + 3] > 0);
        AssertEachBorderContainsTransparency(pixels, 518, 164);
    }

    [Fact]
    public void UploaderAcceptsOnlyTheApprovedLogoBytes()
    {
        var pngPath = Path.Combine(FindRepositoryRoot(), "assets", "branding", "email", "brickle-email-logo.png");
        OfficialLogoValidator.Validate(pngPath);

        var mutations = new (Action<byte[]> Mutate, string ExpectedMessage)[]
        {
            (bytes => bytes[0] = 0, "PNG signature"),
            (bytes => BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(16, 4), 517), "518 x 164"),
            (bytes => BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(20, 4), 163), "518 x 164"),
            (bytes => bytes[25] = 2, "RGBA color type 6"),
            (bytes => bytes[^1] ^= 1, "approved SHA-256")
        };

        foreach (var (mutate, expectedMessage) in mutations)
        {
            var bytes = File.ReadAllBytes(pngPath);
            mutate(bytes);
            var temporaryPath = Path.Combine(Path.GetTempPath(), $"brickle-logo-{Guid.NewGuid():N}.png");
            try
            {
                File.WriteAllBytes(temporaryPath, bytes);
                var exception = Assert.Throws<InvalidDataException>(() => OfficialLogoValidator.Validate(temporaryPath));
                Assert.Contains(expectedMessage, exception.Message);
            }
            finally
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private static byte[] DecodeRgbaPixels(byte[] png, int width, int height)
    {
        using var compressed = new MemoryStream();
        var offset = 8;
        while (offset < png.Length)
        {
            var length = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(offset, 4));
            var chunkType = Encoding.ASCII.GetString(png, offset + 4, 4);
            if (chunkType == "IDAT")
                compressed.Write(png, offset + 8, length);
            offset += length + 12;
        }

        compressed.Position = 0;
        using var decompressed = new MemoryStream();
        using (var inflater = new ZLibStream(compressed, CompressionMode.Decompress, leaveOpen: true))
            inflater.CopyTo(decompressed);

        var scanlines = decompressed.ToArray();
        var stride = width * 4;
        Assert.Equal((stride + 1) * height, scanlines.Length);
        var pixels = new byte[stride * height];
        for (var y = 0; y < height; y++)
        {
            var filter = scanlines[y * (stride + 1)];
            for (var x = 0; x < stride; x++)
            {
                var raw = scanlines[y * (stride + 1) + x + 1];
                var left = x >= 4 ? pixels[y * stride + x - 4] : (byte)0;
                var up = y > 0 ? pixels[(y - 1) * stride + x] : (byte)0;
                var upperLeft = y > 0 && x >= 4 ? pixels[(y - 1) * stride + x - 4] : (byte)0;
                pixels[y * stride + x] = filter switch
                {
                    0 => raw,
                    1 => unchecked((byte)(raw + left)),
                    2 => unchecked((byte)(raw + up)),
                    3 => unchecked((byte)(raw + ((left + up) / 2))),
                    4 => unchecked((byte)(raw + Paeth(left, up, upperLeft))),
                    _ => throw new InvalidDataException($"Unsupported PNG filter: {filter}.")
                };
            }
        }

        return pixels;
    }

    private static byte Paeth(byte left, byte up, byte upperLeft)
    {
        var prediction = left + up - upperLeft;
        var leftDistance = Math.Abs(prediction - left);
        var upDistance = Math.Abs(prediction - up);
        var upperLeftDistance = Math.Abs(prediction - upperLeft);
        return leftDistance <= upDistance && leftDistance <= upperLeftDistance
            ? left
            : upDistance <= upperLeftDistance ? up : upperLeft;
    }

    private static void AssertEachBorderContainsTransparency(byte[] pixels, int width, int height)
    {
        Assert.Contains(Enumerable.Range(0, width), x => pixels[x * 4 + 3] == 0);
        Assert.Contains(Enumerable.Range(0, width), x => pixels[((height - 1) * width + x) * 4 + 3] == 0);
        Assert.Contains(Enumerable.Range(0, height), y => pixels[(y * width) * 4 + 3] == 0);
        Assert.Contains(Enumerable.Range(0, height), y => pixels[(y * width + width - 1) * 4 + 3] == 0);
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "BricklePlatform.Api.sln")))
                return directory.FullName;
            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate BricklePlatform.Api.sln.");
    }
}
