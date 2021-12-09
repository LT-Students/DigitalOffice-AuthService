using System.Threading.Tasks;
using LT.DigitalOffice.AuthService.Models.Dto.Requests;
using LT.DigitalOffice.AuthService.Models.Dto.Responses;
using LT.DigitalOffice.Kernel.Attributes;

namespace LT.DigitalOffice.AuthService.Business.Commands.Interfaces
{
  /// <summary>
  /// Represents interface for a command in command pattern.
  /// </summary>
  [AutoInject]
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
