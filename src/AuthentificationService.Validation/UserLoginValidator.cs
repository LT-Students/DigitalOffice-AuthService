using FluentValidation;
using LT.DigitalOffice.AuthentificationService.Models.Dto;

namespace LT.DigitalOffice.AuthentificationService.Validation
{
    public class UserLoginValidator : AbstractValidator<UserLoginInfoRequest>
    {
        public UserLoginValidator()
        {
            RuleFor(user => user.Email)
                .NotEmpty();

            RuleFor(user => user.Password)
                .NotEmpty();
        }
    }
}