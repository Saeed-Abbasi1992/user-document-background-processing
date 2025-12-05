using MediatR;
using Microsoft.AspNetCore.Http;

namespace UserDocumentProcessor.Application.Users.Commands
{
    public class RegisterUserCommand : IRequest<RegisterUserResultDto>
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required IFormFile Document { get; set; }
    }
}
