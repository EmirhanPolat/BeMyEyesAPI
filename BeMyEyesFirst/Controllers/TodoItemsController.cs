namespace TodoApi.Controllers;

using BeMyEyesFirst.Services;
using Microsoft.AspNetCore.Mvc;


[Route("api/[controller]")]
[ApiController]
public class HeyController : ControllerBase
{
    private readonly DescribeImageService describeImageSampleService;

    private readonly TextToSpeechService textToSpeechService;

    public HeyController()
    {
        describeImageSampleService = new DescribeImageService();
        textToSpeechService = new TextToSpeechService();
    }

    // GET: api/ProcessImage
    // <snippet_GetByID>
    [HttpPost("processImage")]
    public async Task<IActionResult> ProcessImage(IFormFile imageFile) 
    {
        if (imageFile == null)
        {
            return BadRequest("Invalid image upload");
        }

        byte[] imageBytes;
        using (var ms = new MemoryStream())
        {
            imageFile.CopyTo(ms);
            imageBytes = ms.ToArray();
        }

        var (status, message) = await describeImageSampleService.AnalyzeImageFromByteCVClient(imageBytes);

        if(status == 0)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }

        return Ok(message);
    }    
}