using System;
using System.Threading;
using System.Threading.Tasks;
using DentalScheduler.Application.Appointments.Commands.CreateAppointment;
using DentalScheduler.Application.Interfaces;
using DentalScheduler.Domain.Entities;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;

namespace DentalScheduler.Application.Tests.Appointments.Commands;

public class CreateAppointmentCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IPaymentService> _paymentServiceMock;
    private readonly CreateAppointmentCommandHandler _handler;

    public CreateAppointmentCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _paymentServiceMock = new Mock<IPaymentService>();

        _contextMock.Setup(c => c.Appointments).ReturnsDbSet(new List<Appointment>());
        
        _handler = new CreateAppointmentCommandHandler(
            _contextMock.Object,
            _currentUserServiceMock.Object,
            _paymentServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequestAndCost_CreatesAppointmentAndPaymentIntent()
    {
        // Arrange
        var command = new CreateAppointmentCommand
        {
            PatientId = Guid.NewGuid(),
            DentistId = Guid.NewGuid(),
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1),
            Cost = 100
        };

        _currentUserServiceMock.Setup(c => c.GetCurrentUserId()).Returns("some-user");
        _paymentServiceMock.Setup(p => p.CreatePaymentIntentAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()))
                           .ReturnsAsync("pi_test");
        _paymentServiceMock.Setup(p => p.GetPaymentIntentAsync("pi_test"))
                           .ReturnsAsync(new PaymentIntentDetails { ClientSecret = "secret" });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AppointmentId.Should().NotBeEmpty();
        result.PaymentIntentClientSecret.Should().Be("secret");
        
        _contextMock.Verify(c => c.Appointments.Add(It.IsAny<Appointment>()), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DoctorSchedulingWithThemselves_ThrowsInvalidOperationException()
    {
        // Arrange
        var dentistId = Guid.NewGuid();
        var command = new CreateAppointmentCommand
        {
            PatientId = Guid.NewGuid(),
            DentistId = dentistId
        };

        _currentUserServiceMock.Setup(c => c.GetCurrentUserId()).Returns(dentistId.ToString());

        // Act & Assert
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
                      .Should().ThrowAsync<InvalidOperationException>()
                      .WithMessage("A doctor cannot schedule an appointment with themselves.");
    }

    [Fact]
    public async Task Handle_PaymentServiceFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new CreateAppointmentCommand
        {
            PatientId = Guid.NewGuid(),
            DentistId = Guid.NewGuid(),
            Cost = 100
        };

        _currentUserServiceMock.Setup(c => c.GetCurrentUserId()).Returns("some-user");
        _paymentServiceMock.Setup(p => p.CreatePaymentIntentAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()))
                           .ThrowsAsync(new Exception("Payment failed"));

        // Act & Assert
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
                      .Should().ThrowAsync<InvalidOperationException>()
                      .WithMessage("*Failed to create payment intent*");
    }
}
