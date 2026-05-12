using System;
using System.Threading;
using System.Threading.Tasks;
using DentalScheduler.Application.Appointments.Commands.CreateAppointment;
using DentalScheduler.Application.DTOs;
using DentalScheduler.Application.Interfaces;
using DentalScheduler.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace DentalScheduler.Application.Tests.Appointments.Commands.CreateAppointment;

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

        _contextMock.Setup(x => x.Appointments).ReturnsDbSet(new List<Appointment>());
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _handler = new CreateAppointmentCommandHandler(_contextMock.Object, _currentUserServiceMock.Object, _paymentServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenDoctorBooksWithThemselves_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var command = new CreateAppointmentCommand { DentistId = doctorId };
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(doctorId.ToString());

        // Act & Assert
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("A doctor cannot schedule an appointment with themselves.");
    }

    [Fact]
    public async Task Handle_WithoutCost_ShouldCreateAppointmentWithoutPayment()
    {
        // Arrange
        var command = new CreateAppointmentCommand
        {
            PatientId = Guid.NewGuid(),
            DentistId = Guid.NewGuid(),
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddMinutes(30),
            Cost = 0
        };
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(Guid.NewGuid().ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PaymentIntentClientSecret.Should().BeNull();
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _paymentServiceMock.Verify(x => x.CreatePaymentIntentAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithCost_ShouldCreatePaymentIntent()
    {
        // Arrange
        var command = new CreateAppointmentCommand
        {
            PatientId = Guid.NewGuid(),
            DentistId = Guid.NewGuid(),
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddMinutes(30),
            Cost = 100
        };
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(Guid.NewGuid().ToString());
        
        _paymentServiceMock.Setup(x => x.CreatePaymentIntentAsync(100, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("intent-id");
            
        var paymentDetails = new PaymentIntentDetails { Id = "intent-id", ClientSecret = "secret_123" };
        _paymentServiceMock.Setup(x => x.GetPaymentIntentAsync("intent-id"))
            .ReturnsAsync(paymentDetails);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PaymentIntentClientSecret.Should().Be("secret_123");
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCostAndPaymentFails_ShouldThrowException()
    {
        // Arrange
        var command = new CreateAppointmentCommand
        {
            PatientId = Guid.NewGuid(),
            DentistId = Guid.NewGuid(),
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddMinutes(30),
            Cost = 100
        };
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(Guid.NewGuid().ToString());
        
        _paymentServiceMock.Setup(x => x.CreatePaymentIntentAsync(100, It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Payment failed"));

        // Act & Assert
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to create payment intent: Payment failed");
    }
}
