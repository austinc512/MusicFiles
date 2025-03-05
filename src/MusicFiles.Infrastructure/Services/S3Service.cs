using Amazon.S3;
using Amazon.S3.Model;
using MusicFiles.Core.ServiceContracts;

namespace MusicFiles.Infrastructure.Services
{
    // This Infra service is consumed by FileUploadService (Core layer),
    // which delegates S3-related operations to this class.
    public class S3Service(IAmazonS3 amazonS3Client, string bucketName) : IS3Service
    {
        /// <summary>
        /// This method is solely for debugging purposes to troubleshoot S3 connections.
        /// It may be worth removing in the future to prevent usage.
        /// </summary>
        /// <returns>A list of S3 Bucket Names</returns>
        public async Task<IEnumerable<string?>> ListBuckets()
        {
            var listResponse = await amazonS3Client.ListBucketsAsync();
            var bucketNames = listResponse.Buckets.Select(x => x.BucketName).ToList();

            foreach (var bucket in bucketNames)
            {
                Console.WriteLine(bucket);
            } 

            return bucketNames;
        }
        
        public string GeneratePreSignedUploadUrl(string userPublicId, string fileName, TimeSpan expiration)
        {
            var key = $"{userPublicId}/{fileName}";

            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = key,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.Add(expiration)
            };

            return amazonS3Client.GetPreSignedURL(request);
        }
    }
}