using BeMyEyes.Application.Interfaces.AIServices;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using Microsoft.Extensions.Configuration;

namespace BeMyEyes.Infrastructure.Services.AIServices
{
    public class CustomVisionService : ICustomVisionService
    {
        private readonly IConfiguration _configuration;
        private string predictionEndpoint;
        private string predictionKey;
        private static Guid projectId;
        private static ICustomVisionPredictionClient predictionClient;

        public CustomVisionService(IConfiguration configuration)
        {
            _configuration = configuration;
            GetResourceVariables();
            predictionClient = new CustomVisionPredictionClient(new ApiKeyServiceClientCredentials(predictionKey))
            {
                Endpoint = predictionEndpoint
            };
        }

        public async Task<(double, string)> PredictImageTags(byte[] byteData)
        {
            try
            {
                var prediction = await predictionClient.ClassifyImageAsync(projectId, "MoneyClassification", new MemoryStream(byteData));

                if (prediction.Predictions.First().Probability >= 0.8)
                {
                    return (prediction.Predictions.First().Probability, prediction.Predictions.First().TagName);
                }
                else
                {
                    return (0, "Could not identify the value of the money");
                }
            }
            catch (CustomVisionErrorException ex)
            {
                Console.WriteLine($"Custom Vision Error: {ex.Response.StatusCode} - {ex.Response.ReasonPhrase}");
                Console.WriteLine($"Error Content: {ex.Response.Content}");

                return (0, "");
            }
        }

        private void GetResourceVariables()
        {
            try
            {
                predictionKey = _configuration["PREDICTION-KEY"];
                predictionEndpoint = _configuration["RESOURCE-ENDPOINT"];
                projectId = new Guid("fcef7194-78b7-4fde-a411-b09281dc8c92");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetResourceVariables: {ex.Message}");
            }
        }
    }
}
