namespace UserDocumentProcessor.Application.Interfaces
{
    public interface IFileCleanupJob
    {
        Task CleanupAsync();
    }
}
