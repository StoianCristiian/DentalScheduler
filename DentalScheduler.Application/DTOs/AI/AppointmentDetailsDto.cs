using System;
using System.Text.Json.Serialization;

namespace DentalScheduler.Application.DTOs.AI
{
    public class AppointmentDetailsDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("time_window")]
        public TimeWindowDto TimeWindow { get; set; } = new();

        [JsonPropertyName("complexity")]
        public int Complexity { get; set; } = 1;
    }
}
