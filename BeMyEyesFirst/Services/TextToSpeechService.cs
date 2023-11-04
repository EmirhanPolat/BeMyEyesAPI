using Microsoft.CognitiveServices.Speech;

namespace BeMyEyesFirst.Services
{
    public class TextToSpeechService
    {
        public static string subscriptionKey = Environment.GetEnvironmentVariable("RESOURCE_SUBSCRIPTION_KEY");
        public static string region = Environment.GetEnvironmentVariable("RESOURCE_REGION");

        public async void TranslateTextToSpeech()
        {
            var speechConfig = SpeechConfig.FromSubscription(subscriptionKey, region);

            // The language of the voice that speaks.
            speechConfig.SpeechSynthesisVoiceName = "tr-TR-EmelNeural";

            using (var speechSynthesizer = new SpeechSynthesizer(speechConfig))
            {
                // Get text from the console and synthesize to the default speaker.
                Console.WriteLine("Enter some text that you want to speak >");
                string text = Console.ReadLine();

                var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(text);
                OutputSpeechSynthesisResult(speechSynthesisResult, text);
            }
        }

        private static void OutputSpeechSynthesisResult(SpeechSynthesisResult speechSynthesisResult, string text)
        {
            switch (speechSynthesisResult.Reason)
            {
                case ResultReason.SynthesizingAudioCompleted:
                    Console.WriteLine($"Speech synthesized for text: [{text}]");
                    break;
                case ResultReason.Canceled:
                    var cancellation = SpeechSynthesisCancellationDetails.FromResult(speechSynthesisResult);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                        Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
