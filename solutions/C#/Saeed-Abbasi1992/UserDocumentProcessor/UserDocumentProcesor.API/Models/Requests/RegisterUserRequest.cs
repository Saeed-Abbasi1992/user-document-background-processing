namespace UserDocumentProcessor.API.Models.Requests
{
    public class RegisterUserRequest
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required IFormFile Document { get; set; }
    }
}
