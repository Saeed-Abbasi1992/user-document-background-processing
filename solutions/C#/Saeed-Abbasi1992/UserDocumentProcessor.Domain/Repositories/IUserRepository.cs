using UserDocumentProcessor.Domain.Entities;

namespace UserDocumentProcessor.Domain.Repositories
{
    public interface IUserRepository
    {
        Task AddAsync(UserEntity user);
        Task<UserEntity?> GetByIdAsync(Guid id);
    }
}
