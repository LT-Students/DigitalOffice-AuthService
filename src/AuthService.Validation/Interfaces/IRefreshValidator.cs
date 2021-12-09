using FluentValidation;
using LT.DigitalOffice.AuthService.Models.Dto.Requests;
using LT.DigitalOffice.Kernel.Attributes;

namespace LT.DigitalOffice.AuthService.Validation.Interfaces
{
  [AutoInject]
  public interface IRefreshValidator : IValidator<RefreshRequest>
  {
  }
}
