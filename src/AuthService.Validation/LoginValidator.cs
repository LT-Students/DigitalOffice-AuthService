using FluentValidation;
using LT.DigitalOffice.AuthService.Models.Dto.Requests;
using LT.DigitalOffice.AuthService.Validation.Interfaces;

namespace LT.DigitalOffice.AuthService.Validation
{
    public class LoginValidator : AbstractValidator<LoginRequest>, ILoginValidator
    {
        public LoginValidator()
        {
            RuleFor(user => user.LoginData.Trim())
                .NotEmpty();

            RuleFor(user => user.Password.Trim())
                .NotEmpty();
        }
    }
}