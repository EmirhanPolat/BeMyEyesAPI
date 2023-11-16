using BeMyEyes.Application.Interfaces.AIServices;
using Microsoft.AspNetCore.Mvc;

namespace BeMyEyes.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageAnalysisController : Controller
    {
        private readonly IDescribeImageService _describeImageService;

        public ImageAnalysisController(IDescribeImageService describeImageService)
        {
            _describeImageService = describeImageService;    
        }

        [HttpPost("describeImage")]
        public async Task<IActionResult> DescribeImage(IFormFile imageFile)
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

            var (status, message) = await _describeImageService.GetDescriptionsInImage(imageBytes);

            if (status == 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }

            return Ok(message);
        }

        [HttpPost("objectsImage")]
        public async Task<IActionResult> GetObjectsInImage(IFormFile imageFile)
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

            var result = await _describeImageService.GetObjectsInImage(imageBytes);

            if (result == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok(result);
        }

        [HttpPost("tagsImage")]
        public async Task<IActionResult> GetTagsInImage(IFormFile imageFile)
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

            var result = await _describeImageService.GetTagsInImage(imageBytes);

            if (result == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok(result);
        }
    }
}
