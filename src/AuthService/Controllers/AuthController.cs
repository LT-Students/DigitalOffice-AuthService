using LT.DigitalOffice.AuthService.Business.Commands.Interfaces;
using LT.DigitalOffice.AuthService.Models.Dto.Requests;
using LT.DigitalOffice.AuthService.Models.Dto.Responses;
using LT.DigitalOffice.Kernel.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LT.DigitalOffice.AuthService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController
    {
        [HttpPost("login")]
        public async Task<LoginResult> LoginUser(
            [FromServices] ILoginCommand command,
            [FromBody] LoginRequest userCredentials)
        {
            return await command.Execute(userCredentials);
        }

        [HttpPost("refreshtoken")]
        public OperationResultResponse<string> RefreshToken(
            [FromServices] IRefreshTokenCommand command,
            [FromQuery] string token)
        {
            return command.Execute(token);
        }
    }
}
