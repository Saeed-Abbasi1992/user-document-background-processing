namespace UserDocumentProcessor.Application.Interfaces
{
    public interface IFileStorageProvider
    {
        Task<string> SaveFileAsync(Guid userId, string fileName, Stream fileStream);

        Task DeleteFileAsync(string fileName);

        string[] GetFilesPath();
    }
}
