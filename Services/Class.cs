using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

public class AzureBlobService
{
    private readonly string _connectionString;
    private readonly string _containerName;
    private readonly BlobContainerClient _blobContainerClient;

    public AzureBlobService(IConfiguration configuration)
    {
        try
        {
            Console.WriteLine("🌐 Entering AzureBlobService constructor...");

            _connectionString = configuration["AzureStorage:ConnectionString"];
            _containerName = configuration["AzureStorage:ContainerName"];

            Console.WriteLine("🔍 Blob Config:");
            Console.WriteLine("Connection String = " + _connectionString);
            Console.WriteLine("Container Name = " + _containerName);

            if (string.IsNullOrWhiteSpace(_connectionString) || string.IsNullOrWhiteSpace(_containerName))
            {
                throw new Exception("Missing AzureStorage config in appsettings.json");
            }

            _blobContainerClient = new BlobContainerClient(_connectionString, _containerName);
            _blobContainerClient.CreateIfNotExists();

            Console.WriteLine("✅ AzureBlobService setup complete");
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ AzureBlobService ERROR: " + ex.Message);
            throw;
        }
    }


    public async Task<string> UploadAsync(IFormFile file)
    {
        try
        {
            var blob = _blobContainerClient.GetBlobClient(Guid.NewGuid() + Path.GetExtension(file.FileName));
            using (var stream = file.OpenReadStream())
            {
                await blob.UploadAsync(stream, overwrite: true);
            }

            return blob.Uri.ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine("⚠️ Error uploading to Azure Blob: " + ex.Message);
            throw;
        }
    }
}
