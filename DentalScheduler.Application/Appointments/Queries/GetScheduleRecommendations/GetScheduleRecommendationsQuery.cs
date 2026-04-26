using MediatR;
using System;
using DentalScheduler.Application.DTOs.AI;

namespace DentalScheduler.Application.Appointments.Queries.GetScheduleRecommendations;

public class GetScheduleRecommendationsQuery : IRequest<SchedulingResponseDto>
{
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public int ProcedureDurationMinutes { get; set; } = 30;
    public int ProcedureComplexity { get; set; } = 1;
    public DateTime? PreferredDateStart { get; set; }
    public DateTime? PreferredDateEnd { get; set; }
    public string? TimeOfDay { get; set; }
    public bool IsEmergency { get; set; }
}
