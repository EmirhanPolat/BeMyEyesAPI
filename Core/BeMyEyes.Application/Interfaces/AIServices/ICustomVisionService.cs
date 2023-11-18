namespace BeMyEyes.Application.Interfaces.AIServices
{
    public interface ICustomVisionService
    {
        Task<(double, string)> PredictImageTags(byte[] byteData);
    }
}
