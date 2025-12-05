using UserDocumentProcessor.Domain.Enums;

namespace UserDocumentProcessor.Domain.Entities
{
    public class DocumentEntity
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string Path { get; private set; }
        public string? ProcessResultPath { get; set; }
        public ProcessStatus ProcessStatus { get; private set; } = ProcessStatus.Pending;
        public bool IsDeleted { get; private set; }
        public string FileName { get; set; }
        public DateTime CreationDateTime { get; set; } = DateTime.UtcNow;
        protected DocumentEntity() { } // For EF Core

        public DocumentEntity(Guid userId, string path, string fileName)
        {
            UserId = userId;
            Path = path ?? throw new ArgumentNullException(nameof(path));
            FileName = fileName;
        }

        public void MarkProcessing()
        {
            ProcessStatus = ProcessStatus.Processing;
        }

        public void MarkProcessed()
        {
            ProcessStatus = ProcessStatus.Success;
        }

        public void MarkFailed()
        {
            ProcessStatus = ProcessStatus.Failed;
        }

        public void MarkDeleted()
        {
            IsDeleted = true;
        }
    }
}
