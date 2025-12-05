using Microsoft.Extensions.Options;
using UserDocumentProcessor.Application.Interfaces;

namespace UserDocumentProcessor.Infrastructure.Services
{
    public class FileStorageProvider : IFileStorageProvider
    {
        private readonly string _basePath;

        public FileStorageProvider(IOptions<FileStorageSettings> options)
        {
            _basePath = options.Value.BasePath;
        }

        public string[] GetFilesPath()
        {
            if (!Directory.Exists(_basePath))
                return Array.Empty<string>();

            return Directory.GetFiles(_basePath, "*", SearchOption.AllDirectories);
        }

        public async Task<string> SaveFileAsync(Guid userId, string fileName, Stream fileStream)
        {
            var userDirectory = Path.Combine(_basePath, userId.ToString());
            Directory.CreateDirectory(userDirectory);

            var filePath = Path.Combine(userDirectory, fileName);

            using (var output = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(output);
            }

            return filePath;
        }

        public Task DeleteFileAsync(string fileName)
        {
            if (File.Exists(fileName))
                File.Delete(fileName);

            return Task.CompletedTask;
        }
    }
}
