using System.ComponentModel.DataAnnotations;

namespace MusicFiles.Core.DTOs.Request;

    public class PreSignedUrlDto
    {
        /// <summary>
        /// This DTO defines how you request a pre-signed URL from S3.
        /// Currently, I only really need a file name, but this could be extended to require other metadata later
        /// </summary>

        [Required(ErrorMessage = "FileName must be supplied for PreSignedUrl")]
        // regex pattern match for file name here?
        public string? FileName { get; set; }
    }