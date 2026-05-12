using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using DentalScheduler.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace DentalScheduler.Infrastructure.Tests.Services;

public class AwsS3ServiceTests
{
    private readonly Mock<IAmazonS3> _s3ClientMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly AwsS3Service _awsS3Service;

    public AwsS3ServiceTests()
    {
        _s3ClientMock = new Mock<IAmazonS3>();
        _configMock = new Mock<IConfiguration>();
        _configMock.Setup(c => c["AWS_BUCKET_NAME"]).Returns("test-bucket");

        _awsS3Service = new AwsS3Service(_s3ClientMock.Object, _configMock.Object);
    }

    [Fact]
    public async Task UploadFileAsync_WhenStreamIsNull_ShouldThrowArgumentException()
    {
        // Act & Assert
        await FluentActions.Invoking(() => _awsS3Service.UploadFileAsync(null, "test.jpg", "image/jpeg", "user1"))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Niciun fișier încărcat.");
    }

    [Fact]
    public async Task DeleteFileAsync_WithValidUrl_ShouldCallDeleteObjectAsync()
    {
        // Arrange
        var fileUrl = "profiles/user1_123.jpg";
        _s3ClientMock.Setup(x => x.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), default))
            .ReturnsAsync(new DeleteObjectResponse());

        // Act
        await _awsS3Service.DeleteFileAsync(fileUrl);

        // Assert
        _s3ClientMock.Verify(x => x.DeleteObjectAsync(It.Is<DeleteObjectRequest>(req => req.BucketName == "test-bucket" && req.Key == fileUrl), default), Times.Once);
    }

    [Fact]
    public async Task DeleteFileAsync_WithEmptyUrl_ShouldDoNothing()
    {
        // Act
        await _awsS3Service.DeleteFileAsync("");

        // Assert
        _s3ClientMock.Verify(x => x.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), default), Times.Never);
    }

    [Fact]
    public async Task GetPresignedUrlAsync_WithValidKey_ShouldReturnUrl()
    {
        // Arrange
        var fileUrl = "profiles/user1_123.jpg";
        var expectedUrl = "https://test-bucket.s3.amazonaws.com/" + fileUrl;
        _s3ClientMock.Setup(x => x.GetPreSignedURLAsync(It.IsAny<GetPreSignedUrlRequest>()))
            .ReturnsAsync(expectedUrl);

        // Act
        var result = await _awsS3Service.GetPresignedUrlAsync(fileUrl);

        // Assert
        result.Should().Be(expectedUrl);
    }

    [Fact]
    public async Task GetPresignedUrlAsync_WithHttpUrl_ShouldReturnSameUrl()
    {
        // Arrange
        var fileUrl = "http://already-a-url.com/pic.jpg";

        // Act
        var result = await _awsS3Service.GetPresignedUrlAsync(fileUrl);

        // Assert
        result.Should().Be(fileUrl);
    }

    [Fact]
    public async Task GetPresignedUrlAsync_WithEmptyUrl_ShouldReturnNull()
    {
        // Act
        var result = await _awsS3Service.GetPresignedUrlAsync("");

        // Assert
        result.Should().BeNull();
    }
}

