using BeMyEyes.Application.Interfaces.AIServices;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Linq;

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


        private async Task<string> GetDescriptionsInImage(byte[] byteData)
        {         
            try
            {
                var analysis = await cvClient.DescribeImageInStreamAsync(new MemoryStream(byteData));
                return (analysis.Captions.First().Text);
            }
            catch (Exception ex)
            {
                // Handle other exceptions here
                return ($"Error: {ex.Message}");
            }

        }

        public async Task<string> DescribeImage(byte[] byteData)
        {
            var briefDescription = await GetDescriptionsInImage(byteData);
            var objectsInImage = await GetObjectsInImage(byteData);
            var tagsInImage = await GetTagsInImage(byteData);

            var objects = JsonConvert.SerializeObject(objectsInImage);
            var tags = JsonConvert.SerializeObject(tagsInImage);

            
            var client = new HttpClient();
            var requestUri = "https://api.openai.com/v1/chat/completions"; // The API endpoint

            // Set up the request headers
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {chat_key}"); // Replace with your API key

            var helperPrompt = "Hi, I will provide some results to you. The description gives you the main idea, " +
                "the objects gives you the detected objects, and the tags contain some useful information (not all of the tags are necessary). " +
                "Given these results, I want you to provide a summary response which insights these results (max of 2-3 sentence, be specific and don't waste your tokens) " +
                "Do not say things like 'in this image' or 'in the analysis result', just directly say what's in the image. Keep in mind that your response will be converted to speech for visually impaired people to understand their surroundings\r\n\r\n" +
                $"Here are the results:\r\n Description: {briefDescription}\n Objects: {objects}\n Tags: {tags}";
            
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
                            new { type = "text", text = helperPrompt }
                        }
                    }
                },
                max_tokens = 40,
                temperature = 0
            };

            // Serialize the payload to JSON
            var jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Make the request
            var response = await client.PostAsync(requestUri, content);
            var responseString = await response.Content.ReadAsStringAsync();

            // Assuming responseString contains your JSON response
            var jsonResponse = JObject.Parse(responseString);

            // Extracting the 'message' part
            var message = jsonResponse["choices"][0]["message"]["content"].ToString();

            // Print the response
            return message;
        }

        private async Task<IDictionary<string, double>> GetObjectsInImage(byte[] byteData)
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

        private async Task<IDictionary<string, double>> GetTagsInImage(byte[] byteData)
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
