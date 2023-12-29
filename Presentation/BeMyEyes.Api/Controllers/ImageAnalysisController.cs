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
        private readonly IVideoIntelligenceService _videoIntelligenceService;


        public ImageAnalysisController(IComputerVisionService computerVisionService, ICustomVisionService customVisionService, IVideoIntelligenceService videoIntelligenceService)
        {
            _computerVisionService = computerVisionService;
            _customVisionService = customVisionService;
            _videoIntelligenceService = videoIntelligenceService;
        }

        [HttpPost("describeImage")]
        public async Task<IActionResult> DescribeImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length/1024 > 4096)
            {
                return BadRequest("Invalid image upload or image size too big");
            }

            byte[] imageBytes;
            using (var ms = new MemoryStream())
            {
                imageFile.CopyTo(ms);
                imageBytes = ms.ToArray();
            }

            var message = await _computerVisionService.DescribeImage(imageBytes);

            return Ok(message);
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

        [HttpPost("summarizeVideo")]
        public async Task<IActionResult> GetVideoSummarization(IFormFile videoFile)
        {
            if (videoFile == null)
            {
                return BadRequest("Invalid image upload"); ;
            }

            byte[] videoBytes;
            using (var ms = new MemoryStream())
            {
                videoFile.CopyTo(ms);
                videoBytes = ms.ToArray();
            }

            var result = await _videoIntelligenceService.GetVideoSummarization(videoBytes);
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
