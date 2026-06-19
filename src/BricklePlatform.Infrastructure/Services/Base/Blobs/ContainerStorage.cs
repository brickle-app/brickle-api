using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace BricklePlatform.Infrastructure.Services.Base.Blobs
{
    public class ContainerStorage
    {
        private readonly BlobContainerClient _containerClient;

        public ContainerStorage(string storageConnectionString, string containerName)
        {
            _containerClient = new BlobContainerClient(storageConnectionString, containerName);
            _containerClient.CreateIfNotExists();
        }

        public async Task<string> GetBlob(string fileName)
        {
            try
            {
                BlobClient blobClient = _containerClient.GetBlobClient(fileName);
                BlobSasBuilder sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = _containerClient.Name,
                    BlobName = fileName,
                    Resource = "b",
                    ExpiresOn = DateTimeOffset.Parse("2038-01-01T00:00:00Z"),
                };
                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                Uri uri = blobClient.GenerateSasUri(sasBuilder);
                return uri.ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> DeleteBlob(string fileName)
        {
            try
            {
                BlobClient blobClient = _containerClient.GetBlobClient(fileName);
                return await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected BlobContainerClient GetContainerClient()
        {
            return _containerClient;
        }
    }
}