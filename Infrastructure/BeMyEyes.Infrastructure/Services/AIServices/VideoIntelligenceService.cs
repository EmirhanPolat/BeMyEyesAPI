﻿using Azure.Core.GeoJson;
using BeMyEyes.Application.Interfaces.AIServices;
using Google.Cloud.VideoIntelligence.V1;
using Google.LongRunning;
using Google.Protobuf;
using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using static Azure.Core.HttpHeader;

namespace BeMyEyes.Infrastructure.Services.AIServices
{
	public class VideoIntelligenceService : IVideoIntelligenceService
    {
        private static VideoIntelligenceServiceClient client;
        private readonly IConfiguration _configuration;
        private string chat_key;

        public VideoIntelligenceService(IConfiguration configuration)
		{
            // We can access the Environment variable this way too !!! 
            string keyFilePath = "C:/Users/Emirhan/Desktop/KOC/SmartHat/acoustic-patrol-409018-51b0c6911bce.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", keyFilePath);
            _configuration = configuration;
            GetResourceVariables();
            client = VideoIntelligenceServiceClient.Create();
        }


        public async Task<string> GetVideoSummarization(byte[] byteData)
        {
            var analysis = await GetVideoAnalysis(byteData);

            var client = new HttpClient();
            var requestUri = "https://api.openai.com/v1/chat/completions"; // The API endpoint

            // Set up the request headers
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {chat_key}"); // Replace with your API key

            var helperPrompt = "Summarize in 5-6 sentences (80 words max) by describing what's in the frame" +
                "The summary should be clear and descriptive, without using technical jargon. " +
                $"Data: {analysis}";


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
                max_tokens = 100,
                temperature = 0.0
            };

            
            var jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
       
            var response = await client.PostAsync(requestUri, content);
            var responseString = await response.Content.ReadAsStringAsync();

            var jsonResponse = JObject.Parse(responseString);

            var message = jsonResponse["choices"][0]["message"]["content"].ToString();
            return message;
        }


        public async Task<string> GetVideoAnalysis(byte[] byteData)
        {
            var videoByteString = ByteString.CopyFrom(byteData);

            AnnotateVideoRequest request = new AnnotateVideoRequest
            {
              
                Features =
                {
                    Feature.LabelDetection,
                    Feature.ObjectTracking,

                },
                VideoContext = new VideoContext
                {
                    LabelDetectionConfig = new LabelDetectionConfig
                    {
                        FrameConfidenceThreshold = 0.5f,
                        LabelDetectionMode = LabelDetectionMode.ShotMode,
                    },
                    PersonDetectionConfig = new PersonDetectionConfig
                    {
                        IncludeBoundingBoxes = true,
                        IncludeAttributes = true,
                   
                    }
                },
                InputContent = videoByteString,
            };
            Operation<AnnotateVideoResponse, AnnotateVideoProgress> response = await client.AnnotateVideoAsync(request);
            Operation<AnnotateVideoResponse, AnnotateVideoProgress> completedResponse = await response.PollUntilCompletedAsync();
            VideoAnnotationResults analysis = completedResponse.Result.AnnotationResults[0];
            AnnotationResult annotationResult = AnalyzeVideo(analysis);
            if(annotationResult == null)
            {
                return "";
            }

            string jsonAnnotationResult = JsonConvert.SerializeObject(annotationResult);
            return jsonAnnotationResult;
        }

        private static AnnotationResult AnalyzeVideo(VideoAnnotationResults analysis)
        {
            var annotationResult = new AnnotationResult();
            var annotatedObjects = new List<AnnotatedObject>();
            var annotateLabels = new List<AnnotatedLabel>();
          

            // Extract frames based on your criteria

            foreach (LabelAnnotation label in analysis.ShotLabelAnnotations)
            {
                AnnotatedLabel annotatedLabel = new AnnotatedLabel();
                var segments = new List<Segment>();
                string labelName = label.Entity.Description;


                foreach (LabelSegment labelSegment in label.Segments)
                {
                    if (labelSegment.Confidence < 0.70)
                    {
                        continue;
                    }
                    else
                    {
                        long endTimeOffset = labelSegment.Segment.EndTimeOffset.Seconds;
                        long startTimeOffset = labelSegment.Segment.StartTimeOffset.Seconds;

                        var newSegment = new Segment();
                        newSegment.endTimeOffset = endTimeOffset;
                        newSegment.startTimeOffset = startTimeOffset;

                        segments.Add(newSegment);
                    }
                }

                if (segments.Count == 0)
                {
                    continue;
                }

                annotatedLabel.description = labelName;
                annotatedLabel.segments = segments;

                annotateLabels.Add(annotatedLabel);

            }

            foreach (ObjectTrackingAnnotation obj in analysis.ObjectAnnotations)
            {

                string objectName = obj.Entity.Description;
                AnnotatedObject annotatedObject = new AnnotatedObject();
                List<Frame> frames = new List<Frame>();

                int count = obj.Frames.Count;

                for (int i = 0; i < count; i+=5)
                {

                    ObjectTrackingFrame frame = obj.Frames[i];
                    double bottom = frame.NormalizedBoundingBox.Bottom;
                    double left = frame.NormalizedBoundingBox.Left;
                    double right = frame.NormalizedBoundingBox.Right;
                    double top = frame.NormalizedBoundingBox.Top;
                    long timeOffset = frame.TimeOffset.Seconds;

                    Frame newFrame = new Frame();

                    ObjectBoundingBox newBoundingBox = new ObjectBoundingBox();

                    newBoundingBox.bottom = bottom;
                    newBoundingBox.left = left;
                    newBoundingBox.right = right;
                    newBoundingBox.top = top;

                    newFrame.timeOffset = timeOffset;


                    frames.Add(newFrame);     
                }

                annotatedObject.description = objectName;
                annotatedObject.frames = frames;

                annotatedObjects.Add(annotatedObject);
            }

            annotationResult.annotatedLabel = annotateLabels;
            annotationResult.annotatedObjects = annotatedObjects;

            return annotationResult;
        }

        public class ObjectBoundingBox
        {
            public double bottom { get; set; }
            public double left { get; set; }
            public double right { get; set; }
            public double top { get; set; }
        }

        public class Frame 
        {
            public long timeOffset { get; set; }
            public ObjectBoundingBox boundingBox { get; set; }
        }

        public class Segment
        {
            public long endTimeOffset { get; set; }
            public long startTimeOffset { get; set; }
        }

        public class AnnotatedLabel
        {
            public string description { get; set; }
            public List<Segment> segments { get; set; }
        }

        public class AnnotatedObject
        {
            public string description { get; set; }
            public List<Frame> frames { get; set; }
        }

        public class AnnotationResult
        {
            public List<AnnotatedObject> annotatedObjects { get; set; }
            public List<AnnotatedLabel> annotatedLabel { get; set; }
        }


        private void GetResourceVariables()
        {
            try
            {
                chat_key = _configuration["OPENAI-KEY"];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetResourceVariables: {ex.Message}");
            }
        }
    }
}

