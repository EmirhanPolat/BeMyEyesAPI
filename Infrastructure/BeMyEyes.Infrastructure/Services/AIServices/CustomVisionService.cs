using BeMyEyes.Application.Interfaces.AIServices;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;

namespace BeMyEyes.Infrastructure.Services.AIServices
{
    public class CustomVisionService : ICustomVisionService
    {
        private static string predictionEndpoint = Environment.GetEnvironmentVariable("RESOURCE_ENDPOINT");
        private static string predictionKey = Environment.GetEnvironmentVariable("PREDICTION_KEY");
        private static Guid projectId = new Guid("fcef7194-78b7-4fde-a411-b09281dc8c92");

        private static ICustomVisionPredictionClient predictionClient = new CustomVisionPredictionClient(new ApiKeyServiceClientCredentials(predictionKey))
        {
            Endpoint = predictionEndpoint
        };

        public async Task<(double, string)> PredictImageTags(byte[] byteData)
        {
            try
            {
                var prediction = await predictionClient.ClassifyImageAsync(projectId, "MoneyClassification", new MemoryStream(byteData));

                return (prediction.Predictions.First().Probability, prediction.Predictions.First().TagName);
            }
            catch (CustomVisionErrorException ex)
            {
                Console.WriteLine($"Custom Vision Error: {ex.Response.StatusCode} - {ex.Response.ReasonPhrase}");
                Console.WriteLine($"Error Content: {ex.Response.Content}");
               
                return (0, "Predd failed");
            }
        }
    }
}
