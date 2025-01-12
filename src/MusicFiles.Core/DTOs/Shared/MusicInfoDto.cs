using System.ComponentModel.DataAnnotations;
using MusicFiles.Core.Enums;

namespace MusicFiles.Core.DTOs.Shared
{
    public class MusicInfoDto
    {
        [Required(ErrorMessage = "Id is required")]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "UserPublicId is required")]
        public string? UserPublicId { get; set; }
        [Required(ErrorMessage = "FileName is required")]
        public string? FileName { get; set; }
        [Required(ErrorMessage = "Category is required")]
        public string? Category { get; set; }
        [Required(ErrorMessage = "MediaType is required")]
        public MediaType? MediaType { get; set; }
        [Required(ErrorMessage = "LastModified is required")]
        public DateTimeOffset? LastModified  { get; set; } 
    }
}

