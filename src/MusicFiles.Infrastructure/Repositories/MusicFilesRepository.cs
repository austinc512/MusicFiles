using Microsoft.EntityFrameworkCore;
using MusicFiles.Core.Domain.Entities;
using MusicFiles.Core.RepositoryContracts;
using MusicFiles.Infrastructure.Data;

namespace MusicFiles.Infrastructure.Repositories
{
    public class MusicFilesRepository(MusicFilesDbContext context) : IMusicFilesRepository
    {
        // If a repository method calls DbContext.SaveChangesAsync(), ensure no other service
        // modifies the same DbContext instance concurrently to avoid potential concurrency issues.
        public async Task<MusicFileEntity> AddAsync(MusicFileEntity entity)
        {
            await context.MusicFiles.AddAsync(entity);
            // If SaveChangesAsync() completes without throwing an exception,
            // you can safely assume the database transaction was successful.
            // Therefore, returning the same entity object after SaveChangesAsync()
            // reflects the state of the record in the database.
            await context.SaveChangesAsync();
            return entity;
        }

        public async Task<MusicFileEntity?> GetByIdAsync(Guid id)
        {
            // querying by the unique Id of the MusicFileEntity
            return await context.MusicFiles.FindAsync(id);
        }

        public async Task<List<MusicFileEntity>> GetByUserAsync(string userPublicId)
        {
            return await context.MusicFiles
                .Where(m => m.UserPublicId == userPublicId)
                .ToListAsync();
        }
    }
}