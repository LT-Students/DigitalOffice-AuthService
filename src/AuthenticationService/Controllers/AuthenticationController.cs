using LT.DigitalOffice.AuthenticationService.Business.Interfaces;
using LT.DigitalOffice.AuthentificationService.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LT.DigitalOffice.AuthenticationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController
    {
        [HttpPost("login")]
        public async Task<UserLoginResult> LoginUser(
                [FromServices] IUserLoginCommand command,
                [FromBody] UserLoginInfoRequest userCredentials)
        {
            return await command.Execute(userCredentials);
        }

        [HttpPost("forgotPassword")]
        public void ForgotPassword([FromServices] IFogotPasswordCommand command, string userEmail)
        {
            return command.Execute();
        }
    }
}