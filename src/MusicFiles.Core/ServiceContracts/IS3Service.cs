namespace MusicFiles.Core.ServiceContracts
{
    public interface IS3Service
    {
        Task<IEnumerable<string?>> ListBuckets();
        string GeneratePreSignedUploadUrl(string userPublicId, string fileName, TimeSpan expiration);
    }
}