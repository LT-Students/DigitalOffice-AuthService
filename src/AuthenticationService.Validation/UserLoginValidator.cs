using FluentValidation;
using LT.DigitalOffice.AuthenticationService.Models.Dto;

namespace LT.DigitalOffice.AuthenticationService.Validation
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