using System.IO;
using System.Threading.Tasks;

namespace DentalScheduler.Application.Interfaces
{
    public interface IAwsS3Service
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string userId);
        Task DeleteFileAsync(string fileUrl);
        Task<string> GetPresignedUrlAsync(string fileUrl, int expirationInMinutes = 60);
    }
}
