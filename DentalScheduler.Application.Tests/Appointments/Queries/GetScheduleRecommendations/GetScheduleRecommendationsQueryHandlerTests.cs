using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DentalScheduler.Application.Appointments.Queries.GetScheduleRecommendations;
using DentalScheduler.Application.DTOs.AI;
using DentalScheduler.Application.Interfaces;
using DentalScheduler.Domain.Entities;
using DentalScheduler.Domain.Enums;
using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace DentalScheduler.Application.Tests.Appointments.Queries.GetScheduleRecommendations;

public class GetScheduleRecommendationsQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ISmartSchedulingService> _aiServiceMock;
    private readonly GetScheduleRecommendationsQueryHandler _handler;

    public GetScheduleRecommendationsQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _aiServiceMock = new Mock<ISmartSchedulingService>();
        _handler = new GetScheduleRecommendationsQueryHandler(_contextMock.Object, _aiServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnRecommendations_WhenAiServiceReturnsData()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var query = new GetScheduleRecommendationsQuery
        {
            DoctorId = doctorId,
            PatientId = Guid.NewGuid(),
            ProcedureDurationMinutes = 30
        };

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            DentistId = doctorId,
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddMinutes(30)
        };
        appointment.SetStatus(AppointmentStatus.Confirmed);

        var appointments = new List<Appointment> { appointment };

        _contextMock.Setup(x => x.Appointments).ReturnsDbSet(appointments);

        var expectedResponse = new SchedulingResponseDto
        {
            Proposals = new List<ProposedSlotDto>
            {
                new ProposedSlotDto { StartTime = DateTime.UtcNow.AddDays(1), EndTime = DateTime.UtcNow.AddDays(1).AddMinutes(30) }
            }
        };

        _aiServiceMock.Setup(x => x.GetRecommendationsAsync(It.IsAny<SchedulingRequestDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Proposals.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WhenAiServiceReturnsNull_ShouldReturnEmptyProposals()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var query = new GetScheduleRecommendationsQuery
        {
            DoctorId = doctorId
        };
        
        _contextMock.Setup(x => x.Appointments).ReturnsDbSet(new List<Appointment>());
        _aiServiceMock.Setup(x => x.GetRecommendationsAsync(It.IsAny<SchedulingRequestDto>()))
            .ReturnsAsync((SchedulingResponseDto?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Proposals.Should().BeEmpty();
    }
}
