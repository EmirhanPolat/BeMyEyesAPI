namespace BeMyEyesFirst.Services
{
    using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
    using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

    public class DescribeImageService 
    {
        private static string subscriptionKey = Environment.GetEnvironmentVariable("RESOURCE_SUBSCRIPTION_KEY");
        private static string endpoint = Environment.GetEnvironmentVariable("RESOURCE_ENDPOINT");

        private static IComputerVisionClient cvClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials(subscriptionKey))
        {
            Endpoint = endpoint
        };

        public async Task<(int ,string)> AnalyzeImageFromByteCVClient(byte[] byteData)
        {

            // Specify features to be retrieved
            List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
                {
                    VisualFeatureTypes.Description,
                    VisualFeatureTypes.Tags,
                    VisualFeatureTypes.Categories,
                    VisualFeatureTypes.Brands,
                    VisualFeatureTypes.Objects,
                    VisualFeatureTypes.Faces
                };

            var analysis = await cvClient.AnalyzeImageInStreamAsync(new MemoryStream(byteData), features);

            if (analysis == null)
            {
                return (0, "Analysis failed");
            }

            return (1, analysis.Description.Captions.First().Text);
        }
    }
}
