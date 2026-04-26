using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DentalScheduler.Application.DTOs.AI
{
    public class SchedulingRequestDto
    {
        [JsonPropertyName("patient_id")]
        public string PatientId { get; set; } = string.Empty;

        [JsonPropertyName("doctor_id")]
        public string DoctorId { get; set; } = string.Empty;

        [JsonPropertyName("procedure_duration_minutes")]
        public int ProcedureDurationMinutes { get; set; }

        [JsonPropertyName("procedure_complexity")]
        public int ProcedureComplexity { get; set; }

        [JsonPropertyName("doctor_availability")]
        public List<TimeWindowDto> DoctorAvailability { get; set; } = new();

        [JsonPropertyName("existing_appointments")]
        public List<AppointmentDetailsDto> ExistingAppointments { get; set; } = new();

        [JsonPropertyName("preferences")]
        public SchedulingPreferencesDto? Preferences { get; set; }
    }
}
