using Microsoft.EntityFrameworkCore;
using UserDocumentProcessor.Domain.Entities;
using UserDocumentProcessor.Domain.Enums;
using UserDocumentProcessor.Domain.Repositories;

namespace UserDocumentProcessor.Infrastructure.Persistence.Repositories;
public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _context;

    public DocumentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(DocumentEntity document)
    {
        await _context.Documents.AddAsync(document);
    }

    public async Task<DocumentEntity?> GetByIdAsync(Guid id)
    {
        return await _context.Documents.FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<IReadOnlyList<string>> GetPaths()
    {
        return await _context.Documents.Where(p => !p.IsDeleted).Select(p => p.Path).ToListAsync();
    }

    public async Task<IEnumerable<DocumentEntity>> GetStalesAsync()
    {
        var dateTime = DateTime.UtcNow.AddDays(-1);
        return await _context.Documents.Where(p => !p.IsDeleted && p.ProcessStatus != ProcessStatus.Success && p.CreationDateTime < dateTime).ToListAsync();
    }

    public async Task UpdateAsync(DocumentEntity document)
    {
        _context.Documents.Update(document);
    }
}
