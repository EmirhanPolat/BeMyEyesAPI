using BeMyEyes.Application.Interfaces.AIServices;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Configuration;

namespace BeMyEyes.Infrastructure.Services.AIServices
{

    public class ComputerVisionService : IComputerVisionService
    {
        private string subscriptionKey;
        private string endpoint;

        private readonly IConfiguration _configuration;
        private static IComputerVisionClient cvClient;

        public ComputerVisionService(IConfiguration configuration)
        {
            _configuration = configuration;
            GetResourceVariables();

            cvClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials(subscriptionKey))
            {
                Endpoint = endpoint
            };
        }

        public async Task<IDictionary<string, double>> GetWordsInImage(byte[] byteData)
        {
            var url = await cvClient.ReadInStreamAsync(new MemoryStream(byteData));

            Thread.Sleep(2000);


            var operationLocation = url.OperationLocation;
            var sizeID = 36;

            var ID = operationLocation.Substring(operationLocation.Length - sizeID);


            ReadOperationResult analysis;
            do
                {
                analysis = await cvClient.GetReadResultAsync(new Guid(ID));
            }
            while (analysis.Status == OperationStatusCodes.Running ||
                        analysis.Status == OperationStatusCodes.NotStarted);


            return DisplayLines(analysis);
        }


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

        private static IDictionary<string, double> DisplayLines(ReadOperationResult analysis)
        {
            var linesConfidence = new Dictionary<string, double>();

            foreach (var line in analysis.AnalyzeResult.ReadResults[0].Lines)
            {
                double number_of_words = line.Words.Count;
                double total_confidence = 0.0;
                double weighted_confidence;
                double threshold = 0.85;

                foreach(var words in line.Words)
                {
                    total_confidence += words.Confidence;
                }

                weighted_confidence = total_confidence / number_of_words;

                if (weighted_confidence >=  threshold)
                {
                    linesConfidence.Add(line.Text, weighted_confidence);
                }
                else
                {
                    continue;
                }
            }

            return linesConfidence;
        }

        private void GetResourceVariables()
        {
            try
            {
                subscriptionKey = _configuration.GetSection("CognitiveServices")["RESOURCE_SUBSCRIPTION_KEY"];
                endpoint = _configuration.GetSection("CognitiveServices")["RESOURCE_ENDPOINT"];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetResourceVariables: {ex.Message}");
            }
        }

    }
}
