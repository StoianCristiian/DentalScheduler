using System.Threading.Tasks;
using DentalScheduler.Application.DTOs.AI;

namespace DentalScheduler.Application.Interfaces
{
    public interface ISmartSchedulingService
    {
        Task<SchedulingResponseDto?> GetRecommendationsAsync(SchedulingRequestDto request);
    }
}
