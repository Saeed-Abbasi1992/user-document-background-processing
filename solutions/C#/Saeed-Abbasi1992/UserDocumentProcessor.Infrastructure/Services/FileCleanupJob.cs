using Microsoft.Extensions.Logging;
using UserDocumentProcessor.Application.Interfaces;
using UserDocumentProcessor.Domain;
using UserDocumentProcessor.Domain.Repositories;

namespace UserDocumentProcessor.Infrastructure.Services
{
    public class FileCleanupJob : IFileCleanupJob
    {
        private readonly ILogger<FileCleanupJob> _logger;
        private readonly IDocumentRepository _documentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageProvider _fileStorageProvider;

        public FileCleanupJob(ILogger<FileCleanupJob> logger,
            IDocumentRepository documentRepository,
            IUnitOfWork unitOfWork,
            IFileStorageProvider fileStorageProvider)
        {
            _logger = logger;
            _documentRepository = documentRepository;
            _unitOfWork = unitOfWork;
            _fileStorageProvider = fileStorageProvider;
        }

        public async Task CleanupAsync()
        {
            _logger.LogInformation("Nightly cleanup job started. Timestamp: {Timestamp}", DateTime.UtcNow);

            int deletedCount = 0;
            var staleDocuments = await _documentRepository.GetStalesAsync();
            if (staleDocuments != null && staleDocuments.Count() > 0)
            {
                foreach (var document in staleDocuments)
                {
                    try
                    {
                        if (File.Exists(document.Path))
                            File.Delete(document.Path);

                        document.MarkDeleted();
                        await _documentRepository.UpdateAsync(document);
                        deletedCount++;
                        _logger.LogInformation("Deleted stale DB file. FilePath: {FilePath}, DocumentId: {DocumentId}", document.Path, document.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to delete file. FilePath: {FilePath}", document.Path);
                    }
                }

                await _unitOfWork.SaveChangesAsync();
            }

            var dbDocumentsPaths = await _documentRepository.GetPaths();

            var fileStoragePaths = _fileStorageProvider.GetFilesPath();
            foreach (var file in fileStoragePaths)
            {
                var fileExistsInDb = dbDocumentsPaths.Any(d => d == file);
                if (!fileExistsInDb)
                {
                    try
                    {
                        await _fileStorageProvider.DeleteFileAsync(file);
                        deletedCount++;
                        _logger.LogInformation("Deleted orphan file. FilePath: {FilePath}", file);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to delete orphan file. FilePath: {FilePath}", file);
                    }
                }
            }

            _logger.LogInformation("Cleanup completed. TotalDeletedFiles: {TotalDeletedFiles}, Timestamp: {Timestamp}", deletedCount, DateTime.UtcNow);

            _logger.LogInformation("[Job] Cleanup completed.");
        }
    }
}
