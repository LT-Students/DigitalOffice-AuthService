using LT.DigitalOffice.AuthService.Business.Commands.Interfaces;
using LT.DigitalOffice.AuthService.Models.Dto.Requests;
using LT.DigitalOffice.AuthService.Models.Dto.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController
    {
        [HttpPost("login")]
        public LoginResult LoginUser(
            [FromServices] ILoginCommand command,
            [FromBody] LoginRequest userCredentials)
        {
            return command.Execute(userCredentials);
        }
    }
}