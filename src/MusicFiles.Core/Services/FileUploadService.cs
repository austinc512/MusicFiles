using MusicFiles.Core.ServiceContracts;

namespace MusicFiles.Core.Services
{
    public class FileUploadService(IS3Service s3Service) : IFileUploadService
    {
        public string GeneratePreSignedUploadUrl(string userPublicId, string fileName, TimeSpan expiration)
        {
            // biz logic here
            return s3Service.GeneratePreSignedUploadUrl(userPublicId, fileName, expiration);
        }
    }  
}