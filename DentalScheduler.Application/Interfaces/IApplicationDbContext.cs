using DentalScheduler.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DentalScheduler.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Appointment> Appointments { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
