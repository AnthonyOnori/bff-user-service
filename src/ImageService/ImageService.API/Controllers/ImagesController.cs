using ImageService.Application.DTOs;
using ImageService.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ImageService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("images")]
[Authorize]
public class ImagesController : ControllerBase
{
    private readonly IGetImageToBase64UseCase _getImageToBase64UseCase;

    public ImagesController(IGetImageToBase64UseCase getImageToBase64UseCase)
    {
        _getImageToBase64UseCase = getImageToBase64UseCase;
    }

    /// <summary>
    /// Endpoint para procesar imagen: descargar desde URL y convertir a base64
    /// </summary>
    [HttpGet("process")]
    public async Task<ActionResult<ImageDto>> ProcessImageAsBase64([FromQuery] string url)
    {
        if (string.IsNullOrEmpty(url))
            return BadRequest(new { message = "url de imagen es requerida." });

        var imageBase64 = await _getImageToBase64UseCase.ExecuteAsync(url);
        return Ok(imageBase64);
    }
}
