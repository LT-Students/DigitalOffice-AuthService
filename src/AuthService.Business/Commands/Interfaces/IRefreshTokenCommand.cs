using LT.DigitalOffice.AuthService.Models.Dto.Requests;
using LT.DigitalOffice.AuthService.Models.Dto.Responses;
using LT.DigitalOffice.Kernel.Attributes;

namespace LT.DigitalOffice.AuthService.Business.Commands.Interfaces
{
  [AutoInject]
  public interface IRefreshTokenCommand
  {
    LoginResult Execute(RefreshRequest request);
  }
}
