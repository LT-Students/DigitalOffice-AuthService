using LT.DigitalOffice.AuthenticationService.Models.Dto.Requests;
using LT.DigitalOffice.AuthenticationService.Models.Dto.Responses;
using System.Threading.Tasks;

namespace LT.DigitalOffice.AuthenticationService.Business.Interfaces
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
        Task<LoginResult> Execute(LoginRequest request);
    }
}