using FluentValidation;
using LT.DigitalOffice.AuthService.Models.Dto.Requests;
using LT.DigitalOffice.AuthService.Validation.Interfaces;

namespace LT.DigitalOffice.AuthService.Validation
{
  public class LoginValidator : AbstractValidator<LoginRequest>, ILoginValidator
  {
    public LoginValidator()
    {
      RuleFor(user => user.LoginData)
        .NotEmpty()
        .WithMessage("Login data must not be empty.");

      RuleFor(user => user.Password)
        .NotEmpty()
        .WithMessage("Password must not be empty.");
    }
  }
}
