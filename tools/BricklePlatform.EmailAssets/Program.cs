using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using BricklePlatform.EmailAssets;

const string BlobPath = "branding/email/brickle-logo-2026-07.png";

if (args.Length != 1)
    throw new ArgumentException("Usage: BricklePlatform.EmailAssets <local-png-path>");

var localPath = Path.GetFullPath(args[0]);
if (!File.Exists(localPath))
    throw new FileNotFoundException("Logo PNG was not found.", localPath);

OfficialLogoValidator.Validate(localPath);

var connectionString = Environment.GetEnvironmentVariable(
    "InfrastructureSettings__AzureSettings__ConnectionString");
var containerName = Environment.GetEnvironmentVariable(
    "InfrastructureSettings__AzureSettings__BlobName");

if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(containerName))
    throw new InvalidOperationException("Azure connection string and blob container must be supplied through environment variables.");

var container = new BlobContainerClient(connectionString, containerName);
var blob = container.GetBlobClient(BlobPath);

if (await blob.ExistsAsync())
    throw new InvalidOperationException($"Refusing to overwrite existing blob: {BlobPath}");

await using var stream = File.OpenRead(localPath);
try
{
    await blob.UploadAsync(stream, new BlobUploadOptions
    {
        HttpHeaders = new BlobHttpHeaders
        {
            ContentType = "image/png",
            CacheControl = "public, max-age=31536000, immutable"
        },
        Conditions = new BlobRequestConditions
        {
            IfNoneMatch = ETag.All
        }
    });
}
catch (RequestFailedException exception) when (exception.Status == 412)
{
    throw new InvalidOperationException($"Refusing to overwrite existing blob: {BlobPath}", exception);
}

var sas = new BlobSasBuilder
{
    BlobContainerName = container.Name,
    BlobName = BlobPath,
    Resource = "b",
    ExpiresOn = DateTimeOffset.Parse("2038-01-01T00:00:00Z")
};
sas.SetPermissions(BlobSasPermissions.Read);

Console.WriteLine(blob.GenerateSasUri(sas));
