namespace UserDocumentProcessor.Domain.Entities
{
    public class UserEntity
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }

        // Navigation property
        public List<DocumentEntity> Documents { get; private set; } = new();

        protected UserEntity() { } // For EF Core

        public UserEntity(string name, string email)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Email = email ?? throw new ArgumentNullException(nameof(email));
        }

        public void AddDocument(DocumentEntity doc)
        {
            if (doc == null) throw new ArgumentNullException(nameof(doc));
            Documents.Add(doc);
        }
    }
}
