using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api.Services;

public interface IBlobStorageService
{
    
    Task<string> UploadAsync(Stream content, string contentType, string folder, string fileExtension);

    
    Task<(Stream Content, string ContentType)?> DownloadAsync(string blobName);
}


public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _container;

    public BlobStorageService(IConfiguration config)
    {
        var connectionString = config["AzureBlob:ConnectionString"] ?? "UseDevelopmentStorage=true";
        var containerName = config["AzureBlob:ContainerName"] ?? "raceday-images";
        _container = new BlobContainerClient(connectionString, containerName);
    }

    public async Task<string> UploadAsync(Stream content, string contentType, string folder, string fileExtension)
    {
        await _container.CreateIfNotExistsAsync();

        var blobName = $"{folder}/{Guid.NewGuid():N}{fileExtension}";
        var blob = _container.GetBlobClient(blobName);
        await blob.UploadAsync(content, httpHeaders: new BlobHttpHeaders { ContentType = contentType });
        return blobName;
    }

    public async Task<(Stream Content, string ContentType)?> DownloadAsync(string blobName)
    {
        var blob = _container.GetBlobClient(blobName);
        if (!await blob.ExistsAsync()) return null;

        var download = await blob.DownloadStreamingAsync();
        return (download.Value.Content, download.Value.Details.ContentType);
    }
}
