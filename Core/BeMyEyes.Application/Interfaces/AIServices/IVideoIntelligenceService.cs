using System;
namespace BeMyEyes.Application.Interfaces.AIServices
{
	public interface IVideoIntelligenceService
	{
        Task<double> GetVideoSummarization(byte[] byteData);
    }
}

