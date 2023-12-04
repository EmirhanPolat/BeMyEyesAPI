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
            if (imageFile == null || imageFile.Length/1024 > 4096)
            {
                return BadRequest("Invalid image upload or image size too big");
            }

            Console.WriteLine(imageFile.Length/1024);
            byte[] imageBytes;
            using (var ms = new MemoryStream())
            {
                imageFile.CopyTo(ms);
                imageBytes = ms.ToArray();
            }

            var messagggee = await _computerVisionService.WhatsInTheImage(imageBytes);

            var (status, message) = await _computerVisionService.GetDescriptionsInImage(imageBytes);

            if (status == 0)
            {
                return BadRequest(message);
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

        [HttpPost("wordsImage")]
        public async Task<IActionResult> GetWordsInImage(IFormFile imageFile)
        {
            if (imageFile == null)
            {
                return BadRequest("Invalid image upload"); ;
            }

            byte[] imageBytes;
            using (var ms = new MemoryStream())
            {
                imageFile.CopyTo(ms);
                imageBytes = ms.ToArray();
            }

            var result = await _computerVisionService.GetWordsInImage(imageBytes);

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
