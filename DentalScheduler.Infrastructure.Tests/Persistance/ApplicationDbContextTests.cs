using System;
using System.Linq;
using DentalScheduler.Domain.Entities;
using DentalScheduler.Infrastructure.Identity;
using DentalScheduler.Infrastructure.Persistance;
using DentalScheduler.Application.Interfaces; // Added for IApplicationDbContext
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DentalScheduler.Infrastructure.Tests.Persistance;

public class ApplicationDbContextTests
{
    private ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public void Can_Save_And_Retrieve_Appointment()
    {
        // Arrange
        using var context = CreateContext();
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            DentistId = Guid.NewGuid(),
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1),
            Cost = 100.50m
        };

        // Act
        context.Appointments.Add(appointment);
        context.SaveChanges();

        // Assert
        var retrieved = context.Appointments.FirstOrDefault(a => a.Id == appointment.Id);
        retrieved.Should().NotBeNull();
        retrieved.Cost.Should().Be(100.50m);
    }

    [Fact]
    public void Users_Projection_Should_Return_Mapped_Data()
    {
        // Arrange
        using var context = CreateContext();
        var user = new ApplicationUser
        {
            Id = "user1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            ProfilePictureUrl = "pic.jpg"
        };

        context.Set<ApplicationUser>().Add(user); // Fixed Add method call
        context.SaveChanges();

        // Act
        var projection = ((IApplicationDbContext)context).Users.FirstOrDefault(u => u.Id == "user1");

        // Assert
        projection.Should().NotBeNull();
        projection.FirstName.Should().Be("John");
        projection.LastName.Should().Be("Doe");
        projection.Email.Should().Be("john@example.com");
        projection.ProfilePictureUrl.Should().Be("pic.jpg");
    }
}
