using LT.DigitalOffice.AuthenticationService.Models.Dto.Requests;
using LT.DigitalOffice.AuthenticationService.Models.Dto.Responses;
using System.Threading.Tasks;

namespace LT.DigitalOffice.AuthenticationService.Business.Interfaces
{
    /// <summary>
    /// Represents the command pattern.
    /// Provides a method to log in a user.
    /// </summary>
    public interface ILoginCommand
    {
        /// <summary>
        /// Method for getting user id and jwt by email and password.
        /// </summary>
        /// <param name="request">Request model with user email and password.</param>
        /// <returns>Response model with user id and jwt.</returns>
        /// <exception cref="FluentValidation.ValidationException">Thrown when user data is incorrect.</exception>
        /// <exception cref="Kernel.Exceptions.ForbiddenException">Thrown when incorrect login or password.</exception>
        Task<LoginResult> Execute(LoginRequest request);
    }
}