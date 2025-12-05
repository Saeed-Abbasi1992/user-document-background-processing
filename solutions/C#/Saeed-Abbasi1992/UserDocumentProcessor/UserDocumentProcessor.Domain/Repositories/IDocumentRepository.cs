using UserDocumentProcessor.Domain.Entities;

namespace UserDocumentProcessor.Domain.Repositories;
public interface IDocumentRepository
{
    Task AddAsync(DocumentEntity document);
    Task<DocumentEntity?> GetByIdAsync(Guid id);
    Task UpdateAsync(DocumentEntity document);
    Task<IEnumerable<DocumentEntity>> GetStalesAsync();
    Task<IReadOnlyList<string>> GetPaths();
}