using ImageService.Application.DTOs;
using ImageService.Domain.Ports;

namespace ImageService.Application.UseCases;

public interface IGetImageToBase64UseCase
{
    public Task<ImageDto> ExecuteAsync(string url);
}

public class GetImageToBase64UseCase : IGetImageToBase64UseCase
{
    private readonly IImageRepository _imageRepository;

    public GetImageToBase64UseCase(IImageRepository imageRepository)
    {
        _imageRepository = imageRepository;
    }

    public async Task<ImageDto> ExecuteAsync(string url)
    {
        var response = await _imageRepository.DownloadImageAsync(url);

        return new ImageDto()
        {
            Base64 = response.Base64 ?? "",
            ContentType = response.ContentType ?? ""
        };
    }
}
