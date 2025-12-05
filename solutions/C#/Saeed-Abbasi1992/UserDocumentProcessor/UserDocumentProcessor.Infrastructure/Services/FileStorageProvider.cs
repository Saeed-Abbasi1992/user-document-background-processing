using UserDocumentProcessor.Application.Interfaces;

namespace UserDocumentProcessor.Infrastructure.Services
{
    public class FileStorageProvider : IFileStorageProvider
    {
        private readonly string _basePath;

        public FileStorageProvider()
        {
            _basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        }

        public string[] GetFilesPath()
        {
            var uploadRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            return Directory.GetFiles(uploadRoot, "*", SearchOption.AllDirectories);
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
            {
                File.Delete(fileName);
            }

            return Task.CompletedTask;
        }
    }
}
