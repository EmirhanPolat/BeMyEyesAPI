using Azure.AI.OpenAI;
using Azure;
using BeMyEyes.Application.Interfaces.AIServices;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;

namespace BeMyEyes.Infrastructure.Services.AIServices
{

    public class ComputerVisionService : IComputerVisionService
    {
        private string subscriptionKey;
        private string azure_endpoint;

        private string chat_key;

        private readonly IConfiguration _configuration;
        private static IComputerVisionClient cvClient;

        public ComputerVisionService(IConfiguration configuration)
        {
            _configuration = configuration;
            GetResourceVariables();

            cvClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials(subscriptionKey))
            {
                Endpoint = azure_endpoint
            };
        }

        public async Task<(int, string)> GetDescriptionsInImage(byte[] byteData)
        {         try
            {
                var analysis = await cvClient.DescribeImageInStreamAsync(new MemoryStream(byteData));
                return (1, analysis.Captions.First().Text);
            }
            catch (Exception ex)
            {
                // Handle other exceptions here
                return (0, $"Error: {ex.Message}");
            }

        }

        public async Task<string> WhatsInTheImage(byte[] byteData)
        {
            var client = new HttpClient();
            var requestUri = "https://api.openai.com/v1/chat/completions"; // The API endpoint

            // Set up the request headers
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {chat_key}"); // Replace with your API key

            // Create the payload
            var payload = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = "Say Test!" },
                            //new
                            //{
                            //    type = "image_url",
                            //    image_url = new { url = "https://upload.wikimedia.org/wikipedia/commons/thumb/d/dd/Gfp-wisconsin-madison-the-nature-boardwalk.jpg/2560px-Gfp-wisconsin-madison-the-nature-boardwalk.jpg" }
                            //}
                        }
                    }
                },
                max_tokens = 2,
                temperature = 0
            };

            // Serialize the payload to JSON
            var jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Make the request
            var response = await client.PostAsync(requestUri, content);
            var responseString = await response.Content.ReadAsStringAsync();

            // Print the response
            return responseString;
        }

        public async Task<IDictionary<string, double>> GetObjectsInImage(byte[] byteData)
        {
            try
            {
                var analysis = await cvClient.DetectObjectsInStreamAsync(new MemoryStream(byteData));
                return DisplayObjects(analysis);

            }
            catch (Exception)
            {
                // Handle other exceptions here
                return new Dictionary<string, double>();
            }
        }

        public async Task<IDictionary<string, double>> GetTagsInImage(byte[] byteData)
        {
            try
            {
                var analysis = await cvClient.TagImageInStreamAsync(new MemoryStream(byteData));

                return DisplayTags(analysis);
            }
            catch (ComputerVisionErrorResponseException)
            {
                return new Dictionary<string, double>();

            }

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

        private void GetResourceVariables()
        {
            try
            {
                subscriptionKey = _configuration["RESOURCE-SUBSCRIPTION-KEY"];
                azure_endpoint = _configuration["RESOURCE-ENDPOINT"];
                chat_key = _configuration["OPENAI-KEY"];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetResourceVariables: {ex.Message}");
            }
        }
    }
}
