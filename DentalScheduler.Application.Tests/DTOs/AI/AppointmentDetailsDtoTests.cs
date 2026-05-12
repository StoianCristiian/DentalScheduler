using System;
using DentalScheduler.Application.DTOs.AI;
using FluentAssertions;
using Xunit;

namespace DentalScheduler.Application.Tests.DTOs.AI;

public class AppointmentDetailsDtoTests
{
    [Fact]
    public void Properties_Should_Be_Set_And_Retrieved_Correctly()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();

        // Act
        var dto = new AppointmentDetailsDto
        {
            Id = id,
            TimeWindow = new TimeWindowDto(),
            Complexity = 2
        };

        // Assert
        dto.Id.Should().Be(id);
        dto.TimeWindow.Should().NotBeNull();
        dto.Complexity.Should().Be(2);
    }
}
