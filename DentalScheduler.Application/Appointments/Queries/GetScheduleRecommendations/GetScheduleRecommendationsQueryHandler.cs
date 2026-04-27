using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DentalScheduler.Application.DTOs.AI;
using DentalScheduler.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalScheduler.Application.Appointments.Queries.GetScheduleRecommendations;

public class GetScheduleRecommendationsQueryHandler : IRequestHandler<GetScheduleRecommendationsQuery, SchedulingResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ISmartSchedulingService _aiService;

    public GetScheduleRecommendationsQueryHandler(IApplicationDbContext context, ISmartSchedulingService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    public async Task<SchedulingResponseDto> Handle(GetScheduleRecommendationsQuery request, CancellationToken cancellationToken)
    {
        // 1. Determina fereastra de analiza
        var startDate = request.PreferredDateStart ?? DateTime.Now.Date; // Time local per server pt a evite de-sync cu datele date de UI 
        var endDate = request.PreferredDateEnd ?? startDate.AddDays(14);

        if (endDate < startDate)
        {
            endDate = startDate.AddDays(14);
        }

        var queryStart = startDate;
        var queryEnd = endDate.AddDays(1).AddTicks(-1);

        // 2. Extrage programarile existente din baza de date pentru acel doctor in acea perioada
        var existingAppointments = await _context.Appointments
            .Where(a => a.DentistId == request.DoctorId &&
                        a.StartAt >= queryStart &&
                        a.StartAt <= queryEnd &&
                        a.Status != Domain.Enums.AppointmentStatus.Cancelled &&
                        a.Status != Domain.Enums.AppointmentStatus.Rejected)
            .ToListAsync(cancellationToken);

        var existingApptDtos = existingAppointments.Select(a => {
            // Fortam normalizarea orei la timpu vizual din fata ecranului tau local.
            // Daca in db e 05:00 Unspecified dar in UI era 08:00, e fixat catre local time
            var s = a.StartAt;
            var e = a.EndAt;
            
            // Fix for EndAt being 0001-01-01 in the database
            if (e.Year <= 1)
            {
                e = s.AddMinutes(30);
            }

            return new AppointmentDetailsDto
            {
                Id = a.Id.ToString(),
                TimeWindow = new TimeWindowDto 
                { 
                    StartTime = s, 
                    EndTime = e 
                },
                Complexity = 2
            };
        }).ToList();

        // 3. Simuleaza disponibilitatea doctorului (Luni -> Vineri, 08:00 - 16:00)
        var doctorAvailabilities = new List<TimeWindowDto>();
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
            {
                var availStart = date.AddHours(8); // Exact 08:00 PM in ziua testata
                var availEnd = date.AddHours(16); // Exact 16:00 PM
                
                // Dacă verificăm disponibilitatea pentru ziua curentă, ajustăm ora
                if (date.Date == DateTime.Now.Date && DateTime.Now.Hour >= 8)
                {
                    availStart = DateTime.Now.AddHours(1); 
                    var minutesRound = (availStart.Minute / 15 + 1) * 15;
                    availStart = availStart.Date.AddHours(availStart.Hour).AddMinutes(minutesRound);
                }

                if (availStart < availEnd)
                {
                    doctorAvailabilities.Add(new TimeWindowDto
                    {
                        StartTime = availStart,
                        EndTime = availEnd
                    });
                }
            }
        }

        // 4. Construieste payload-ul catre modulul de AI
        var aiRequest = new SchedulingRequestDto
        {
            PatientId = request.PatientId.ToString(),
            DoctorId = request.DoctorId.ToString(),
            ProcedureDurationMinutes = request.ProcedureDurationMinutes,
            ProcedureComplexity = request.ProcedureComplexity,
            DoctorAvailability = doctorAvailabilities,
            ExistingAppointments = existingApptDtos,
            Preferences = new SchedulingPreferencesDto
            {
                PreferredDateStart = request.PreferredDateStart,
                PreferredDateEnd = request.PreferredDateEnd,
                TimeOfDay = request.TimeOfDay,
                IsEmergency = request.IsEmergency
            }
        };

        // 5. Contacteaza serviciul AI via interfața HTTP
        var aiResponse = await _aiService.GetRecommendationsAsync(aiRequest);

        if (aiResponse == null)
        {
            return new SchedulingResponseDto { Proposals = new List<ProposedSlotDto>() };
        }

        return aiResponse;
    }
}
