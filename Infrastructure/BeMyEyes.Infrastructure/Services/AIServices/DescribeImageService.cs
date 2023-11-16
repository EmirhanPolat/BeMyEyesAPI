using BeMyEyes.Application.Interfaces.AIServices;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace BeMyEyes.Infrastructure.Services.AIServices
{

    public class DescribeImageService : IDescribeImageService
    {
        private static string subscriptionKey = Environment.GetEnvironmentVariable("RESOURCE_SUBSCRIPTION_KEY");
        private static string endpoint = Environment.GetEnvironmentVariable("RESOURCE_ENDPOINT");

        private static IComputerVisionClient cvClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials(subscriptionKey))
        {
            Endpoint = endpoint
        };

        public async Task<(int, string)> GetDescriptionsInImage(byte[] byteData)
        {
            var analysis = await cvClient.DescribeImageInStreamAsync(new MemoryStream(byteData));

            return (1, analysis.Captions.First().Text);
        }

        public async Task<IDictionary<string, double>> GetObjectsInImage(byte[] byteData)
        {
            var analysis = await cvClient.DetectObjectsInStreamAsync(new MemoryStream(byteData));

            return DisplayObjects(analysis);
        }

        public async Task<IDictionary<string, double>> GetTagsInImage(byte[] byteData)
        {
            var analysis = await cvClient.TagImageInStreamAsync(new MemoryStream(byteData));

            return DisplayTags(analysis);
        }

        private static IDictionary<string, double> DisplayObjects(DetectResult analysis)
        {
            var objectsConfidence = new Dictionary<string, double>();

            foreach (var obj in analysis.Objects)
            {
                if (objectsConfidence.ContainsKey(obj.ObjectProperty))
                {
                    objectsConfidence[obj.ObjectProperty] = obj.Confidence;
                }
                else
                {
                    objectsConfidence.Add(obj.ObjectProperty, obj.Confidence);
                }
            }

            return objectsConfidence;
        }

        private static IDictionary<string, double> DisplayTags(TagResult analysis)
        {
            var objectsConfidence = new Dictionary<string, double>();

            foreach (var obj in analysis.Tags)
            {
                if (objectsConfidence.ContainsKey(obj.Name))
                {
                    objectsConfidence[obj.Name] = obj.Confidence;
                }
                else
                {
                    objectsConfidence.Add(obj.Name, obj.Confidence);
                }
            }

            return objectsConfidence;
        }
    }
}
