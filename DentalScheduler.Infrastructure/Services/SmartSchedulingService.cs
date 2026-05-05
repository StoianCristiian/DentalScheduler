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
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/v1/schedule/recommend", request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<SchedulingResponseDto>();
                }

                var errorContext = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[SmartSchedulingService] AI Service returned non-success status code: {response.StatusCode} Payload: {errorContext}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SmartSchedulingService] Error communicating with AI service: {ex.Message}");
                return new SchedulingResponseDto { Proposals = new System.Collections.Generic.List<ProposedSlotDto>() };
            }
        }
    }
}
