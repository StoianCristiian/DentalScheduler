using System;
using System.Text.Json.Serialization;

namespace DentalScheduler.Application.DTOs.AI
{
    public class ProposedSlotDto
    {
        [JsonPropertyName("doctor_id")]
        public string DoctorId { get; set; } = string.Empty;

        [JsonPropertyName("start_time")]
        public DateTime StartTime { get; set; }

        [JsonPropertyName("end_time")]
        public DateTime EndTime { get; set; }

        [JsonPropertyName("score")]
        public double Score { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;
    }
}
