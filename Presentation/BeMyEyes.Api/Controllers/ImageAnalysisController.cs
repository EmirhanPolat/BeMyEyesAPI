using BeMyEyes.Application.Interfaces.AIServices;
using Microsoft.AspNetCore.Mvc;

namespace BeMyEyes.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageAnalysisController : Controller
    {
        private readonly IComputerVisionService _computerVisionService;
        private readonly ICustomVisionService _customVisionService;

        public ImageAnalysisController(IComputerVisionService computerVisionService, ICustomVisionService customVisionService)
        {
            _computerVisionService = computerVisionService;
            _customVisionService = customVisionService;
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

            var (status, message) = await _computerVisionService.GetDescriptionsInImage(imageBytes);

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

            var result = await _computerVisionService.GetObjectsInImage(imageBytes);

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

            var result = await _computerVisionService.GetTagsInImage(imageBytes);

            if (result == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok(result);
        }

        [HttpPost("moneyPredict")]
        public async Task<IActionResult> GetPredictionForMoney(IFormFile imageFile)
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

            var (probability, tag) = await _customVisionService.PredictImageTags(imageBytes);

            return Ok(tag);
        }
    }
}
