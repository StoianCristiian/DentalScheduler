using System;
using System.Text.Json.Serialization;

namespace DentalScheduler.Application.DTOs.AI
{
    public class SchedulingPreferencesDto
    {
        [JsonPropertyName("preferred_date_start")]
        public DateTime? PreferredDateStart { get; set; }

        [JsonPropertyName("preferred_date_end")]
        public DateTime? PreferredDateEnd { get; set; }

        [JsonPropertyName("time_of_day")]
        public string? TimeOfDay { get; set; }

        [JsonPropertyName("is_emergency")]
        public bool IsEmergency { get; set; }
    }
}
