using MusicFiles.Core.DTOs.Response;
using MusicFiles.Core.DTOs.Shared;

namespace MusicFiles.Core.ServiceContracts
{
    public interface IMusicDataService
    {
        public Task<List<MusicInfoDto>> ListMusicMediaFilesByPublicId(string id);
        public Task<MusicResponseDto> CreateMusicMediaFile(MusicInfoDto sheetMusicRequest, string s3Key);
    }    
}