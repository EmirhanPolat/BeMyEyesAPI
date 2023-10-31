using Azure;
using Azure.AI.Vision.Common;
using Azure.AI.Vision.ImageAnalysis;
using BeMyEyesFirst.Services;
using Humanizer.Bytes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;
using System.Text;
using TodoApi.Models;

namespace TodoApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HeyController : ControllerBase
{
    private readonly TodoContext _context;

    private readonly DescribeImageService describeImageSampleService;

    public HeyController(TodoContext context)
    {
        _context = context;
        describeImageSampleService = new DescribeImageService();
    }

    private async Task<byte[]> FromBase64ToByteArray(Stream stream)
    {
        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
        {
            string requestBody = await reader.ReadToEndAsync();
            // Log or process the request body as needed
            JObject payload = JObject.Parse(requestBody);
            Console.WriteLine("Received Payload:");
            Console.WriteLine(payload["image_data"]);

            return Convert.FromBase64String(payload["image_data"].ToString());
        }
    }

    // GET: api/ProcessImage
    // <snippet_GetByID>
    [HttpPost()]
    public async Task<bool> ProcessImage() 
    {
        byte[] byteData = FromBase64ToByteArray(Request.Body).Result;

        await describeImageSampleService.DescribeImageFromByteAsync(byteData);
        Console.WriteLine("\nPress ENTER to exit.");

        //ComputerVisionClient client = Authenticate(endpoint, subscriptionKey);
        //    var response = client.AnalyzeImageAsync("https://png2jpg.com/images/png2jpg/icon.png");
        //    Console.WriteLine(response.Result.ToJson());

        //    using var imageSource = VisionSource.FromUrl(
        //new Uri("https://learn.microsoft.com/azure/ai-services/computer-vision/media/quickstarts/presentation.png"));

        //    var analysisOptions = new ImageAnalysisOptions()
        //    {
        //        Features =
        //      ImageAnalysisFeature.CropSuggestions
        //    | ImageAnalysisFeature.Caption
        //    | ImageAnalysisFeature.DenseCaptions
        //    | ImageAnalysisFeature.Objects
        //    | ImageAnalysisFeature.People
        //    | ImageAnalysisFeature.Text
        //    | ImageAnalysisFeature.Tags
        //    };
        //    var serviceOptions = new VisionServiceOptions(endpoint, new AzureKeyCredential(subscriptionKey));

        //    using var analyzer = new ImageAnalyzer(serviceOptions, imageSource, analysisOptions);

        //if (response == null)
        //{
        //    return false;
        //}

        return true;
    }

    public static ComputerVisionClient Authenticate(string endpoint, string key)
    {
        ComputerVisionClient client =
          new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
          { Endpoint = endpoint };
        return client;
    }
}