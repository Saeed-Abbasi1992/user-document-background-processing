namespace UserDocumentProcessor.Application.Users.Commands
{
    public class RegisterUserResultDto
    {
        public Guid UserId { get; set; }
        public required string Status { get; set; }
        public required string Message { get; set; }
    }
}
