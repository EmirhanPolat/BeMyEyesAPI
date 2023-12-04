namespace BeMyEyes.Application.Interfaces.AIServices
{
    public interface IComputerVisionService
    {
        Task<(int, string)> GetDescriptionsInImage(byte[] byteData);

        Task<IDictionary<string, double>> GetObjectsInImage(byte[] byteData);

        Task<IDictionary<string, double>> GetTagsInImage(byte[] byteData);

        Task<string> WhatsInTheImage(byte[] byteData);

        Task<IDictionary<string, double>> GetWordsInImage(byte[] imageBytes);
    }
}
