using LT.DigitalOffice.AuthService.Models.Dto.Requests;
using LT.DigitalOffice.AuthService.Models.Dto.Responses;

namespace LT.DigitalOffice.AuthService.Business.Commands.Interfaces
{
    /// <summary>
    /// Represents interface for a command in command pattern.
    /// </summary>
    public interface ILoginCommand
    {
        /// <summary>
        /// Method for getting user id and jwt by email and password.
        /// </summary>
        /// <param name="request">Request model with user email and password.</param>
        /// <returns>Response model with user id and jwt</returns>
        LoginResult Execute(LoginRequest request);
    }
}
