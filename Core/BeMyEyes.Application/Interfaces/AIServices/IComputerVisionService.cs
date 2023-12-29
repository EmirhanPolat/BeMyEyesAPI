namespace BeMyEyes.Application.Interfaces.AIServices
{
    public interface IComputerVisionService
    {
        Task<string> DescribeImage(byte[] byteData);

        Task<IDictionary<string, double>> GetWordsInImage(byte[] imageBytes);
    }
}
