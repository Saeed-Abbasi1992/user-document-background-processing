using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserDocumentProcessor.API.Models.Requests;
using UserDocumentProcessor.Application.Users.Commands;

namespace UserDocumentProcessor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Registers a new user and uploads a document
        /// </summary>
        /// <param name="request">User registration data with document</param>
        /// <returns>Registered user information</returns>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromForm] RegisterUserRequest request)
        {
            var command = new RegisterUserCommand
            {
                Name = request.Name,
                Email = request.Email,
                Document = request.Document
            };

            var result = await _mediator.Send(command);

            return Created("", result);
        }
    }
}
