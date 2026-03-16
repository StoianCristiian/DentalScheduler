using DentalScheduler.Application.Admin.DTOs;
using DentalScheduler.Application.Interfaces;
using DentalScheduler.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalScheduler.Application.Admin.Queries.GetDashboardStats;

public record GetDashboardStatsQuery : IRequest<AdminDashboardStatsDto>;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, AdminDashboardStatsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public GetDashboardStatsQueryHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<AdminDashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var totalPatients = await _identityService.GetCountInRoleAsync("Patient");
        var totalDoctors = await _identityService.GetCountInRoleAsync("Doctor");
        
        var totalAppointments = await _context.Appointments.CountAsync(cancellationToken);
        
        var totalRevenue = await _context.Appointments
            .Where(a => a.Status == AppointmentStatus.Completed && a.Cost != null)
            .SumAsync(a => a.Cost, cancellationToken) ?? 0;

        return new AdminDashboardStatsDto
        {
            TotalPatients = totalPatients,
            TotalDoctors = totalDoctors,
            TotalAppointments = totalAppointments,
            TotalRevenue = totalRevenue
        };
    }
}

