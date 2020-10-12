using FluentValidation;
using LT.DigitalOffice.AuthenticationService.Models.Dto.Requests;
using LT.DigitalOffice.AuthenticationService.Validation.Interfaces;

namespace LT.DigitalOffice.AuthenticationService.Validation
{
    public class LoginValidator : AbstractValidator<LoginRequest>, ILoginValidator
    {
        public LoginValidator()
        {
            RuleFor(user => user.LoginData)
                .NotEmpty();

            RuleFor(user => user.Password)
                .NotEmpty();
        }
    }
}