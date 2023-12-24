namespace BeMyEyes.Application.Interfaces.AIServices
{
    public interface ISpeechService
    {
        Task<string> GetSpeech();
    }
}
