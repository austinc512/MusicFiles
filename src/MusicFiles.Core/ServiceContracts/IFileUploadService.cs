namespace MusicFiles.Core.ServiceContracts
{
    public interface IFileUploadService
    {
        string GeneratePreSignedUploadUrl(string userPublicId, string fileName, TimeSpan expiration);
    }   
}