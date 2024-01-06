using System;
namespace BeMyEyes.Application.Interfaces.AIServices
{
	public interface IVideoIntelligenceService
	{
        Task<string> GetVideoSummarization(byte[] byteData);
    }
}

