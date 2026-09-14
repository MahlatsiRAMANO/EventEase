
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace EventEase.Services
{
    public class AzureBlobService
    {
        private readonly BlobContainerClient _containerClient;

        public AzureBlobService(IConfiguration configuration)
        {
            var connectionString =
                configuration.GetConnectionString("AzureStorage");

            _containerClient = new BlobContainerClient(
                connectionString,
                "eventease-images"
            );
        }

        // Upload an image to Azure Blob Storage
        public async Task<string> UploadImageAsync(
            IFormFile file,
            string fileName)
        {
            await _containerClient.CreateIfNotExistsAsync(
                PublicAccessType.None
            );

            var blobClient = _containerClient.GetBlobClient(fileName);

            using var stream = file.OpenReadStream();

            await blobClient.UploadAsync(
                stream,
                new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                }
            );

            // Save the normal blob URL in the database
            return blobClient.Uri.ToString();
        }

        // Create a temporary secure URL for displaying a private image
        public string GetImageUrl(string blobUrl)
        {
            if (string.IsNullOrEmpty(blobUrl))
            {
                return "";
            }

            var blobUri = new Uri(blobUrl);

            var blobName = Uri.UnescapeDataString(
                blobUri.AbsolutePath.Substring(
                    blobUri.AbsolutePath.IndexOf(
                        "/eventease-images/",
                        StringComparison.OrdinalIgnoreCase
                    ) + "/eventease-images/".Length
                )
            );

            var blobClient = _containerClient.GetBlobClient(blobName);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerClient.Name,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
            };

            sasBuilder.SetPermissions(
                BlobSasPermissions.Read
            );

            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }
    }
}
