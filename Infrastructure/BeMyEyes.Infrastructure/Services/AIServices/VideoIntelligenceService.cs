using BeMyEyes.Application.Interfaces.AIServices;
using Google.Cloud.VideoIntelligence.V1;
using Google.LongRunning;
using Google.Protobuf;
using Newtonsoft.Json;
using System;

namespace BeMyEyes.Infrastructure.Services.AIServices
{
	public class VideoIntelligenceService : IVideoIntelligenceService
    {
        private static VideoIntelligenceServiceClient client;
        public VideoIntelligenceService()
		{
            // We can access the Environment variable this way too !!! 
            // string keyFilePath = "/Users/ardapoyraz/Documents/Koc_University/Semester9/Proje_API_KEY/acoustic-patrol-409018-51b0c6911bce.json";
            // Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", keyFilePath);

            client = VideoIntelligenceServiceClient.Create();
        }

        public async Task<double> GetVideoSummarization(byte[] byteData)
        {
            var videoByteString = ByteString.CopyFrom(byteData);

            AnnotateVideoRequest request = new AnnotateVideoRequest
            {
                Features =
                {
                    // Feature.Unspecified,
                    Feature.LabelDetection,
                },
                InputContent = videoByteString,
            };
            // Make the request
            Operation<AnnotateVideoResponse, AnnotateVideoProgress> response = await client.AnnotateVideoAsync(request);

            // Poll until the returned long-running operation is complete
            Operation<AnnotateVideoResponse, AnnotateVideoProgress> completedResponse = await response.PollUntilCompletedAsync();
            // Retrieve the operation result
            AnnotateVideoResponse result = completedResponse.Result;

            // Or get the name of the operation
            string operationName = response.Name;
            // This name can be stored, then the long-running operation retrieved later by name
            Operation<AnnotateVideoResponse, AnnotateVideoProgress> retrievedResponse = await client.PollOnceAnnotateVideoAsync(operationName);
            // Check if the retrieved long-running operation has completed
            if (retrievedResponse.IsCompleted)
            {
                // If it has completed, then access the result
                AnnotateVideoResponse retrievedResult = retrievedResponse.Result;
            }
            return 1.5;
        }
    }
}

