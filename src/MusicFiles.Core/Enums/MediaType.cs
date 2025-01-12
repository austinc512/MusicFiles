namespace MusicFiles.Core.Enums
{
    /// <summary>
    /// Enum definition for the types of files that can be stored in S3.
    /// This may need to be more global later, but I'm not sure.
    /// </summary>
    public enum MediaType
    {
        Pdf,
        Jpg,
        Jpeg,
        Png,
        Mp3,
        Mp4,
        Other // Catch-all for unsupported types
    }
}