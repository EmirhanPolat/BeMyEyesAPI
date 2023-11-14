namespace BeMyEyesFirst.Services
{
    using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
    using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
    using Microsoft.Rest;
    using NuGet.Protocol;

    public class MoneyPredictionService
    {
        //private static string trainingEndpoint = Environment.GetEnvironmentVariable("VISION_TRAINING_ENDPOINT");
        //private static string trainingKey = Environment.GetEnvironmentVariable("VISION_TRAINING_KEY");
        private static string predictionEndpoint = Environment.GetEnvironmentVariable("RESOURCE_ENDPOINT");
        private static string predictionKey = Environment.GetEnvironmentVariable("PREDICTION_KEY");
        private static string predictionResourceId = Environment.GetEnvironmentVariable("PREDICTION_RESOURCE_ID");
        private static Guid projectId = new Guid("fcef7194-78b7-4fde-a411-b09281dc8c92");

        private static ICustomVisionPredictionClient predictionClient = new CustomVisionPredictionClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.ApiKeyServiceClientCredentials(predictionKey))
        {
            Endpoint = predictionEndpoint
        };

        public async Task<(int, string)> PredictImageTags(byte[] byteData)
        {
            var prediction = await predictionClient.ClassifyImageAsync(projectId, "MoneyClassification", new MemoryStream(byteData));

            if (prediction == null)
            {
                return (0, "Prediction failed");
            }

            Console.WriteLine(prediction.ToJson());

            return (1, prediction.Predictions.First().TagName);
        }
    }
}
