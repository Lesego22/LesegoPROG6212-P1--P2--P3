using System.IO;

namespace RaceDay.Api.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadAsync(
            Stream content,
            string contentType,
            string folder,
            string extension);

        Task<(byte[] Content, string ContentType)?> DownloadAsync(
            string name);
    }
}