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

        private string chat_endpoint;
        private string chat_key;
        private string chat_deploymentID;

        private readonly IConfiguration _configuration;
        private static IComputerVisionClient cvClient;
        private static OpenAIClient openAI_client;

        public ComputerVisionService(IConfiguration configuration)
        {
            _configuration = configuration;
            GetResourceVariables();

            cvClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials(subscriptionKey))
            {
                Endpoint = azure_endpoint
            };

            openAI_client = new OpenAIClient(new Uri(chat_endpoint), new AzureKeyCredential(chat_key));
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
            //        var systemPrompt = "You are a virtual agent that helps users write creative advertisements for products.";

            //        var userPrompt =
            //            """
            //Write a creative ad for the following product to run on Facebook aimed at parents:

            //Product: Learning Room is a virtual environment to help students from kindergarten to high school excel in school.
            //""";

            //        var completionOptions = new ChatCompletionsOptions
            //        {
            //            MaxTokens = 100,
            //            Temperature = 0.5f,
            //            FrequencyPenalty = 0.0f,
            //            PresencePenalty = 0.0f,
            //            NucleusSamplingFactor = 1 // Top P
            //        };
            //        completionOptions.Messages.Add(new ChatMessage(ChatRole.System, systemPrompt));

            //        completionOptions.Messages.Add(new ChatMessage(ChatRole.User, userPrompt));


            //        ChatCompletions response = await openAI_client.GetChatCompletionsAsync(completionOptions);

            //        return response.Choices.First().Message.Content;

            var client = new HttpClient();
            var requestUri = "https://api.openai.com/v1/chat/completions"; // The API endpoint

            // Set up the request headers
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {chat_key}"); // Replace with your API key

            // Create the payload
            var payload = new
            {
                model = "gpt-4-vision-preview",
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = "What's in this image" },
                            new
                            {
                                type = "image_url",
                                image_url = new { url = "https://upload.wikimedia.org/wikipedia/commons/thumb/d/dd/Gfp-wisconsin-madison-the-nature-boardwalk.jpg/2560px-Gfp-wisconsin-madison-the-nature-boardwalk.jpg" }
                            }
                        }
                    }
                },
                max_tokens = 10
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
                subscriptionKey = _configuration.GetSection("CognitiveServices")["RESOURCE_SUBSCRIPTION_KEY"];
                azure_endpoint = _configuration.GetSection("CognitiveServices")["RESOURCE_ENDPOINT"];

                chat_key = _configuration.GetSection("OPEN_AI")["AOAI_KEY"];
                chat_endpoint = "https://api.openai.com/v1/chat/completions";
                //chat_deploymentID = _configuration.GetSection("OPEN_AI")["AOAI_DEPLOYMENTID"];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetResourceVariables: {ex.Message}");
            }
        }
    }
}
