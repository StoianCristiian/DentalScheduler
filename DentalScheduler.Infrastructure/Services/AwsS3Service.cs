using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using DentalScheduler.Application.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace DentalScheduler.Infrastructure.Services
{
    public class AwsS3Service : IAwsS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public AwsS3Service(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _bucketName = configuration["AWS_BUCKET_NAME"] ?? "dental-scheduler-profile-pictures";
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string userId)
        {
            if (fileStream == null || fileStream.Length == 0)
                throw new ArgumentException("Niciun fișier încărcat.");

            var uniqueFileName = $"profiles/{userId}_{Guid.NewGuid()}{Path.GetExtension(fileName)}";

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = fileStream,
                Key = uniqueFileName,
                BucketName = _bucketName,
                ContentType = contentType
            };

            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(uploadRequest);

            return uniqueFileName;
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return;

            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = fileUrl
            };

            await _s3Client.DeleteObjectAsync(deleteObjectRequest);
        }

        public async Task<string> GetPresignedUrlAsync(string fileUrl, int expirationInMinutes = 60)
        {
            if (string.IsNullOrEmpty(fileUrl)) return null;
            if (fileUrl.StartsWith("http")) return fileUrl; // Already a URL or default picture maybe

            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = fileUrl,
                Expires = DateTime.UtcNow.AddMinutes(expirationInMinutes)
            };

            return await _s3Client.GetPreSignedURLAsync(request);
        }
    }
}
