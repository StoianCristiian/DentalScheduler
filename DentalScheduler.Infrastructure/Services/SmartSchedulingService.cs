using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DentalScheduler.Application.DTOs.AI;
using DentalScheduler.Application.Interfaces;
namespace DentalScheduler.Infrastructure.Services
{
    public class SmartSchedulingService : ISmartSchedulingService
    {
        private readonly HttpClient _httpClient;
        public SmartSchedulingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<SchedulingResponseDto?> GetRecommendationsAsync(SchedulingRequestDto request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/schedule/recommend", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SchedulingResponseDto>();
            }
            return null;
        }
    }
}
