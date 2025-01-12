namespace MusicFiles.Core.DTOs.Response
{
    public class MusicResponseDto
    {
        public string? PublicId { get; set; } 
        public string? FileName { get; set; }
        public string? Category { get; set; }
        public DateTimeOffset? LastModified { get; set; }
    }
}