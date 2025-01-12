using MusicFiles.Core.Enums;

namespace MusicFiles.Core.Domain.Entities
{
    public class MusicFileEntity
    {
        public Guid Id { get; private set; } // Unique Id for Entity
        public string? UserPublicId { get; private set; }
        public string? FileName { get; private set; }
        public string? S3Key { get; private set; }
        public string? Category { get; private set; }
        public MediaType? MediaType { get; private set; } // Enum for media types
        public DateTimeOffset? LastModified { get; private set; } // Timezone-aware timestamp
        
        // trying parameterless constructor to satisfy EF Core
        public MusicFileEntity() { }

        // Constructor with all arguments
        public MusicFileEntity(Guid id, string? userPublicId, string? fileName, string? s3Key, string? category, MediaType mediaType, DateTimeOffset? lastModified)
        {
            Id = id;
            UserPublicId = userPublicId;
            FileName = fileName;
            S3Key = s3Key;
            Category = category;
            MediaType = mediaType;
            LastModified = lastModified;
        }
        
        // Overload for new Id for Entity
        public MusicFileEntity(string? userPublicId, string? fileName, string? s3Key, string? category, MediaType mediaType, DateTimeOffset? lastModified)
            : this(Guid.NewGuid(), userPublicId, fileName, s3Key, category, mediaType, lastModified) { }
    }
    
}