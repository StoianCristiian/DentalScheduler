using DentalScheduler.Application.Interfaces;
using DentalScheduler.Domain.Entities;
using DentalScheduler.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DentalScheduler.Infrastructure.Persistance;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Appointment> Appointments => Set<Appointment>();

    public IQueryable<UserProjection> Users =>
        Set<ApplicationUser>().Select(u => new UserProjection
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email ?? string.Empty,
            ProfilePictureUrl = u.ProfilePictureUrl
        });

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Important pentru Identity
        
        // Configureăm tipul coloanei Cost pentru a evita warning-urile
        modelBuilder.Entity<Appointment>()
            .Property(a => a.Cost)
            .HasColumnType("decimal(18,2)");
    }
}