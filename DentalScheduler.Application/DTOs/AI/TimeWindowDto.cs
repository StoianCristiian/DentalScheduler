using System;
using System.Text.Json.Serialization;

namespace DentalScheduler.Application.DTOs.AI
{
    public class TimeWindowDto
    {
        [JsonPropertyName("start_time")]
        public DateTime StartTime { get; set; }

        [JsonPropertyName("end_time")]
        public DateTime EndTime { get; set; }
    }
}
