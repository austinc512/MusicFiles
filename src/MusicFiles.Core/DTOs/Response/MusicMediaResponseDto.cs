namespace MusicFiles.Core.DTOs.Response
{
    public class MusicMediaResponseDto
    {
        public string? PublicId { get; set; } 
        public string? FileName { get; set; }
        public string? Category { get; set; }
        public DateTime? LastModified { get; set; }
    }    
}
