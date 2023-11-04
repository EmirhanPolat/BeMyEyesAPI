namespace BeMyEyesFirst.Services
{
    using BeMyEyesFirst.Models;
    using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
    using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
    using Newtonsoft.Json.Linq;
    using System.Net.Http.Headers;
    using TodoApi.Models;

    public class DescribeImageService
    {
        public static string subscriptionKey = Environment.GetEnvironmentVariable("RESOURCE_SUBSCRIPTION_KEY");
        public static string endpoint = Environment.GetEnvironmentVariable("RESOURCE_ENDPOINT");

        private static IComputerVisionClient cvClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials(subscriptionKey))
        {
            Endpoint = endpoint
        };

        public async Task AnalyzeImageFromByteCVClient(byte[] byteData)
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

            ImageCaption caption = analysis.Description.Captions.First();

            ImageMetadata metadata = analysis.Metadata;

            MyImageDescription myImageDescription = new MyImageDescription
            {
                Text = caption.Text,
                Confidence = caption.Confidence,
            };

            MyImageMetadata myImageMetadata = new MyImageMetadata
            {
                Format = metadata.Format,
                Width = metadata.Width,
                Height = metadata.Height,
            };


            Console.ReadLine();
        }

        /// <summary>
        /// Given byteData describe the image using HTTP Client. 
        /// </summary>
        /// <param name="byteData"></param>
        /// <returns></returns>
        public async Task AnalyzeImageFromByteHttpClient(byte[] byteData)
        {
            try
            {
                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", subscriptionKey);

                string requestParameters =
                "visualFeatures=" + 
                        VisualFeatureTypes.Description + "," +
                        VisualFeatureTypes.Brands + ',' +
                        VisualFeatureTypes.Tags + "," +
                        VisualFeatureTypes.Faces;

                // Assemble the URI for the REST API method.
                string uri = $"{endpoint}/vision/v3.1/analyze?{requestParameters}";

                // Add the byte array as an octet stream to the request body.
                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    // This example uses the "application/octet-stream" content type.
                    // The other content types you can use are "application/json" and "multipart/form-data".
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                    HttpResponseMessage response = await client.PostAsync(uri, content);

                    string contentString = await response.Content.ReadAsStringAsync();

                    var jsonized = JObject.Parse(contentString);

                    var desc = jsonized.SelectToken("description.tags");

                    Console.WriteLine("\nResponse:\n\n{0}\n", JToken.Parse(contentString).ToString());        

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
            }
        }

        public static ComputerVisionClient Authenticate(string endpoint, string key)
        {
            ComputerVisionClient client =
              new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
              { Endpoint = endpoint };
            return client;
        }
    }
}
