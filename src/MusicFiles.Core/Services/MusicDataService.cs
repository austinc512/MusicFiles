using MusicFiles.Core.Domain.Entities;
using MusicFiles.Core.DTOs.Response;
using MusicFiles.Core.DTOs.Shared;
using MusicFiles.Core.Enums;
using MusicFiles.Core.RepositoryContracts;
using MusicFiles.Core.ServiceContracts;

namespace MusicFiles.Core.Services
{
    // Core Service responsibilities:
    // Implements business logic and orchestrates the functionality of multiple repositories or external services.
    // Acts as the "brain" of your application, enforcing the rules and requirements of your domain.
    // Contains reusable methods that abstract business rules, which can be used by multiple controllers or other Core services.
    
    // Validation responsibilities:
    // Validates the domain-specific business rules and logic (e.g., checking that a user is authorized to access a resource, ensuring file sizes or types conform to business policies).
    // Can perform structural or basic validation if needed, particularly when interacting with external systems like repositories or APIs.
    // Example: Verify that a user’s file quota is not exceeded before inserting a record in the database.
    public class MusicDataService : IMusicDataService
    {
        private IMusicFilesRepository _musicFilesRepository;
        public MusicDataService(IMusicFilesRepository musicFilesRepository)
        {
            _musicFilesRepository = musicFilesRepository;
        }
        // modify for size parameter later.
        public async Task<List<MusicInfoDto>> ListMusicMediaFilesByPublicId(string userPublicId)
        {
            var musicFileEntities = await _musicFilesRepository.GetByUserAsync(userPublicId);

            var response = musicFileEntities.Select((item) => new MusicInfoDto()
                {
                    // including default values
                    Id = item.Id,
                    UserPublicId = item.UserPublicId ?? string.Empty,
                    FileName = item.FileName ?? "Untitled",
                    Category = item.Category ?? "Unknown",
                    MediaType = item.MediaType ?? MediaType.Other,
                    LastModified = item.LastModified
                }
            )
            .ToList();

            return response;
        }

        public async Task<MusicResponseDto> CreateMusicMediaFile(MusicInfoDto musicFileRequest, string s3Key)
        {
            
            var sheetMusicEntity = new MusicFileEntity(
                Guid.NewGuid(),
                musicFileRequest.UserPublicId,
                musicFileRequest.FileName,
                s3Key,
                musicFileRequest.Category,
                musicFileRequest.MediaType ?? MediaType.Other,
                musicFileRequest.LastModified
                );
            
            var response = await _musicFilesRepository.AddAsync(sheetMusicEntity);
            
            return new MusicResponseDto()
            {
                PublicId = response.UserPublicId ?? string.Empty,
                FileName = response.FileName ?? "Unknown File Name",
                Category = response.Category ?? "Unknown",
                LastModified  = response.LastModified // I don't want to use DateTime.Now if this property wasn't set.
            };
        }
    }
}