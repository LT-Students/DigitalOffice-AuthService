using System.Threading.Tasks;
using LT.DigitalOffice.AuthenticationService.Models.Dto;

namespace LT.DigitalOffice.AuthenticationService.Business.Interfaces
{
    /// <summary>
    /// Represents the command pattern.
    /// Provides a method to log in a user.
    /// </summary>
    public interface IUserLoginCommand
    {
        /// <summary>
        /// According to the user's data, returns his ID and token.
        /// </summary>
        /// <param name="request">Request model with user email and password.</param>
        /// <returns>Response model with user id and jwt.</returns>
        /// <exception cref="ValidationException">Thrown when user data is incorrect.</exception>
        /// <exception cref="Kernel.Exceptions.ForbiddenException">Thrown when incorrect login or password.</exception>
        Task<UserLoginResult> Execute(UserLoginInfoRequest request);
    }
}