using Microsoft.EntityFrameworkCore;
using UserDocumentProcessor.Domain.Entities;
using UserDocumentProcessor.Domain.Repositories;

namespace UserDocumentProcessor.Infrastructure.Persistence.Repositories;
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(UserEntity user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task<UserEntity?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .Include(u => u.Documents)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}

