namespace BeMyEyesFirst.Services
{
    using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
    using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
    using Newtonsoft.Json.Linq;
    using System.Net.Http.Headers;


    public class DescribeImageService
    {
        public static string subscriptionKey = Environment.GetEnvironmentVariable("COMPUTER_VISION_SUBSCRIPTION_KEY");
        public static string endpoint = Environment.GetEnvironmentVariable("COMPUTER_VISION_ENDPOINT");

        private static ComputerVisionClient cvClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials(subscriptionKey))
        {
            Endpoint = endpoint
        };

        public async Task DescribeImageFromUrl(string url)
        {
            // Specify features to be retrieved
            List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
                {
                    VisualFeatureTypes.Description,
                    VisualFeatureTypes.Tags,
                    VisualFeatureTypes.Categories,
                    VisualFeatureTypes.Brands,
                    VisualFeatureTypes.Objects,
                    VisualFeatureTypes.Adult
                };

            using (var httpClient = new HttpClient())
            {
                // Download the image from the URL
                byte[] imageData = await httpClient.GetByteArrayAsync(url);

                var analysis = await cvClient.AnalyzeImageInStreamAsync(new MemoryStream(imageData), features);

                foreach (var caption in analysis.Description.Captions)
                {
                    Console.WriteLine($"Description: {caption.Text} (confidence: {caption.Confidence.ToString("P")})");
                }
            }
            // get image captions

        }

        public async Task AnalyzeImageFromUrl(string url)
        {
            Console.WriteLine("Describe an image:");

            string remoteImageUrl = url;

            await AnalyzeImageFromUrlAsync(remoteImageUrl);
        }

        /// <summary>
        /// Describes the image
        /// </summary>
        /// <param name="imageUrl"></param>
        /// <returns></returns>
        private static async Task DescribeImageFromUrlAsync(string imageUrl)
        {
            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                Console.WriteLine("\nInvalid remote image url:\n{0} \n", imageUrl);
                return;
            }
            try
            {
                HttpClient client = new HttpClient();
                // Request headers
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                string uri = $"{endpoint}/vision/v3.1/describe";

                string requestBody = " {\"url\":\"" + imageUrl + "\"}";
                var content = new StringContent(requestBody);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                // Post the request and display the result
                HttpResponseMessage response = await client.PostAsync(uri, content);
                string contentString = await response.Content.ReadAsStringAsync();
                Console.WriteLine("\nResponse:\n\n{0}\n", JToken.Parse(contentString).ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
            }
        }

        /// <summary>
        /// Analyzes the image
        /// </summary>
        /// <param name="imageURL"></param>
        /// <returns></returns>
        private static async Task AnalyzeImageFromUrlAsync(string imageURL)
        {
            try
            {
                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", subscriptionKey);

                // Request parameters. A third optional parameter is "details".
                // The Analyze Image method returns information about the following
                // visual features:
                // Categories:  categorizes image content according to a
                //              taxonomy defined in documentation.
                // Description: describes the image content with a complete
                //              sentence in supported languages.
                // Color:       determines the accent color, dominant color, 
                //              and whether an image is black & white.
                string requestParameters =
                    "visualFeatures=" + VisualFeatureTypes.Description;

                // Assemble the URI for the REST API method.
                string uri = $"{endpoint}/vision/v3.1/analyze?{requestParameters}";


                // Read the contents of the specified local image
                // into a byte array.
                //byte[] byteData = GetImageAsByteArray(imageFilePath);

                string requestBody = " {\"url\":\"" + imageURL + "\"}";
                var content = new StringContent(requestBody);

                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response; response = await client.PostAsync(uri, content);

                // Asynchronously get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync();

                var jsonized = JObject.Parse(contentString);

                var desc = jsonized.SelectToken("description.tags");

                // Display the JSON response.
                Console.WriteLine("\nResponse:\n\n{0}\n",
                    JToken.Parse(contentString).ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
            }
        }

        /// <summary>
        /// Given byteData describe the image using Azure. 
        /// </summary>
        /// <param name="byteData"></param>
        /// <returns></returns>
        public async Task DescribeImageFromByteAsync(byte[] byteData)
        {
            try
            {
                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                string uri = $"{endpoint}/vision/v2.0/describe";
                // Read the contents of the specified local image into a byte array

                // Add the byte array as an octet stream to the request body.
                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    // This example uses the "application/octet-stream" content type.
                    // The other content types you can use are "application/json" and "multipart/form-data".
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                    // Asynchronously call the REST API method.
                    HttpResponseMessage response = await client.PostAsync(uri, content);
                    // Asynchronously get the JSON response.
                    string contentString = await response.Content.ReadAsStringAsync();
                    // Display the JSON response.
                    Console.WriteLine("\nResponse:\n\n{0}\n", JToken.Parse(contentString).ToString());
                    

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
            }
        }

        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            // Open a read-only file stream for the specified file.
            using (FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                // Read the file's contents into a byte array.
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }
    }
}
