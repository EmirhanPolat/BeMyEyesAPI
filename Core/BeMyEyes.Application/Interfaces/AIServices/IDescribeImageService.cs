namespace BeMyEyes.Application.Interfaces.AIServices
{
    public interface IDescribeImageService
    {
        Task<(int, string)> GetDescriptionsInImage(byte[] byteData);

        Task<IDictionary<string, double>> GetObjectsInImage(byte[] byteData);

        Task<IDictionary<string, double>> GetTagsInImage(byte[] byteData);
    }
}
