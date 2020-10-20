using LT.DigitalOffice.AuthenticationService.Business.Interfaces;
using LT.DigitalOffice.AuthenticationService.Models.Dto.Requests;
using LT.DigitalOffice.AuthenticationService.Models.Dto.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LT.DigitalOffice.AuthenticationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController
    {
        [HttpPost("login")]
        public async Task<LoginResult> LoginUser(
            [FromServices] ILoginCommand command,
            [FromBody] LoginRequest userCredentials)
        {
            return await command.Execute(userCredentials);
        }
    }
}