using ImageService.Domain.Entities;

namespace ImageService.Domain.Ports;

public interface IImageRepository
{
    Task<Image> DownloadImageAsync(string url);
}
