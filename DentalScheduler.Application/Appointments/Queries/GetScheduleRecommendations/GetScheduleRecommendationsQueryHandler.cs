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
        // 1. Determina fereastra de analiza (ex: urmatoarele 14 zile daca nu se specifica)
        var startDate = request.PreferredDateStart ?? DateTime.UtcNow.Date;
        var endDate = request.PreferredDateEnd ?? startDate.AddDays(14);

        if (endDate < startDate)
        {
            endDate = startDate.AddDays(14);
        }

        // --- CORECGERE: Conversie la UTC ---
        // Asigură-te că datele trimise către AI și aduse din BD sunt comparabile ca fus orar.
        startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc).AddDays(1).AddTicks(-1); // Până la sfârșitul zilei endDate

        // 2. Extrage programarile existente din baza de date pentru acel doctor in acea perioada
        var existingAppointments = await _context.Appointments
            .Where(a => a.DentistId == request.DoctorId &&
                        a.StartAt >= startDate &&
                        a.StartAt <= endDate &&
                        a.Status != Domain.Enums.AppointmentStatus.Cancelled &&
                        a.Status != Domain.Enums.AppointmentStatus.Rejected)
            .ToListAsync(cancellationToken);

        var existingApptDtos = existingAppointments.Select(a => new AppointmentDetailsDto
        {
            Id = a.Id.ToString(),
            TimeWindow = new TimeWindowDto { StartTime = DateTime.SpecifyKind(a.StartAt, DateTimeKind.Utc), EndTime = DateTime.SpecifyKind(a.EndAt, DateTimeKind.Utc) },
            Complexity = 2 // Poate fi modelat pe baza TreatmentType
        }).ToList();

        // 3. Simuleaza disponibilitatea doctorului (Luni -> Vineri, 08:00 - 16:00)
        var doctorAvailabilities = new List<TimeWindowDto>();
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
            {
                // IMPORTANT: Verifică ca ora curentă să nu fi trecut pentru ziua de azi.
                var availStart = DateTime.SpecifyKind(date.AddHours(8), DateTimeKind.Utc);
                var availEnd = DateTime.SpecifyKind(date.AddHours(16), DateTimeKind.Utc);
                
                // Dacă verificăm disponibilitatea pentru ziua curentă, ajustăm ora de start la ora curentă + 1 oră buffer
                if (date.Date == DateTime.UtcNow.Date && DateTime.UtcNow.Hour >= 8)
                {
                    availStart = DateTime.UtcNow.AddHours(1); // Nu propune programări în trecut, lasă minim 1 ora
                    // Ajustăm la un multiplu de 15 minute
                    var minutesRound = (availStart.Minute / 15 + 1) * 15;
                    availStart = availStart.Date.AddHours(availStart.Hour).AddMinutes(minutesRound);
                }

                // Adaugă fereastra doar dacă e mai mare strict validă
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
