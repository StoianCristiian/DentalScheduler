using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DentalScheduler.Application.DTOs.AI
{
    public class SchedulingResponseDto
    {
        [JsonPropertyName("proposals")]
        public List<ProposedSlotDto> Proposals { get; set; } = new();
    }
}
