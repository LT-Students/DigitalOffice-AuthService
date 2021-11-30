using FluentValidation;
using LT.DigitalOffice.AuthService.Models.Dto.Requests;
using LT.DigitalOffice.AuthService.Validation.Interfaces;

namespace LT.DigitalOffice.AuthService.Validation
{
  public class RefreshValidator : AbstractValidator<RefreshRequest>, IRefreshValidator
  {
    public RefreshValidator()
    {
      RuleFor(request => request.RefreshToken.Trim())
        .NotEmpty()
        .WithMessage("Token must not be empty.");
    }
  }
}
