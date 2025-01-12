using MusicFiles.Core.Domain.Entities;

namespace MusicFiles.Core.RepositoryContracts;

public interface IMusicFilesRepository
{
     Task<MusicFileEntity> AddAsync(MusicFileEntity entity);
     Task<MusicFileEntity?> GetByIdAsync(Guid id);
     Task<List<MusicFileEntity>> GetByUserAsync(string userPublicId);
}