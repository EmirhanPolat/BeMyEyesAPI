﻿namespace BeMyEyesFirst.Services
{
    using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
    using Microsoft.EntityFrameworkCore.Metadata.Internal;
    using Newtonsoft.Json.Linq;
    using System.Net.Http.Headers;
    using System.Runtime.Intrinsics.Arm;


    public class DescribeImageService
    {
        public static string subscriptionKey = Environment.GetEnvironmentVariable("COMPUTER_VISION_SUBSCRIPTION_KEY");
        public static string endpoint = Environment.GetEnvironmentVariable("COMPUTER_VISION_ENDPOINT");

        public async Task DescribeImageFromUrl(string url)
        {
            Console.WriteLine("Describe an image:");

            string remoteImageUrl = url; 

            await DescribeImageFromUrlAsync(remoteImageUrl);
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
                    "visualFeatures=" + VisualFeatureTypes.Description + "," + VisualFeatureTypes.Color + "," + VisualFeatureTypes.Objects;

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

                // Display the JSON response.
                Console.WriteLine("\nResponse:\n\n{0}\n",
                    JToken.Parse(contentString).ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
            }
        }

        // See this repo's readme.md for info on how to get these images. Or, set the path to any appropriate image on your machine.
        //string imageFilePath = @"Images\objects.jpg";
        //await DescribeImageFromStreamAsync(imageFilePath, endpoint, key);

        //static async Task DescribeImageFromStreamAsync(string imageFilePath, string endpoint, string subscriptionKey)
        //{
        //    if (!File.Exists(imageFilePath))
        //    {
        //        Console.WriteLine("\nInvalid file path");
        //        return;
        //    }
        //    try
        //    {
        //        HttpClient client = new HttpClient();

        //        // Request headers.
        //        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
        //        string uri = $"{endpoint}/vision/v2.0/describe";
        //        // Read the contents of the specified local image into a byte array.
        //        byte[] byteData = GetImageAsByteArray(imageFilePath);
        //        // Add the byte array as an octet stream to the request body.
        //        using (ByteArrayContent content = new ByteArrayContent(byteData))
        //        {
        //            // This example uses the "application/octet-stream" content type.
        //            // The other content types you can use are "application/json" and "multipart/form-data".
        //            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        //            // Asynchronously call the REST API method.
        //            HttpResponseMessage response = await client.PostAsync(uri, content);
        //            // Asynchronously get the JSON response.
        //            string contentString = await response.Content.ReadAsStringAsync();
        //            // Display the JSON response.
        //            Console.WriteLine("\nResponse:\n\n{0}\n", JToken.Parse(contentString).ToString());
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("\n" + e.Message);
        //    }
        //}

        //static byte[] GetImageAsByteArray(string imageFilePath)
        //{
        //    // Open a read-only file stream for the specified file.
        //    using (FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
        //    {
        //        // Read the file's contents into a byte array.
        //        BinaryReader binaryReader = new BinaryReader(fileStream);
        //        return binaryReader.ReadBytes((int)fileStream.Length);
        //    }
        //}
    }
}
