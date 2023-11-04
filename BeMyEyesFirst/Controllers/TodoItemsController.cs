namespace TodoApi.Controllers;

using BeMyEyesFirst.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Text;


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
    public async Task<bool> ProcessImage() 
    {
        // Get the image stream
        byte[] byteData = FromBase64ToByteArray(Request.Body).Result;

        if (byteData == null)
        {
            return false;
        }

        //textToSpeechService.TranslateTextToSpeech();
        
        //await describeImageSampleService.AnalyzeImageFromByteCVClient(byteData);

        //await describeImageSampleService.AnalyzeImageFromByteHttpClient(byteData);

        return true;
    }

    private async Task<byte[]> FromBase64ToByteArray(Stream stream)
    {
        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
        {
            string requestBody = await reader.ReadToEndAsync();
            JObject payload = JObject.Parse(requestBody);
            //Console.WriteLine("Received Payload:");
            //Console.WriteLine(payload["image_data"]);

            return Convert.FromBase64String(payload["image_data"].ToString());
        }
    }
}